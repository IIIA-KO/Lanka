import { Injectable, OnDestroy, inject } from '@angular/core';
import { BehaviorSubject, Observable, Subscription, firstValueFrom } from 'rxjs';
import { AuthService } from './auth/auth.service';
import {
  InstagramOperationState,
  InstagramOperationType,
  IInstagramStatus,
  IInstagramStatusResponse,
} from '../models/instagram';
import {
  InstagramStatusNotification,
  SignalRService,
} from './signalr.service';
import { UsersAgent } from '../api/users.agent';

interface InitOptions {
  force?: boolean;
}

@Injectable({ providedIn: 'root' })
export class InstagramStatusService implements OnDestroy {
  private static readonly CONNECTION_WAIT_MS = 150;

  private readonly authService = inject(AuthService);
  private readonly signalRService = inject(SignalRService);

  private readonly linkingStatusSubject =
    new BehaviorSubject<IInstagramStatus | null>(null);
  private readonly renewalStatusSubject =
    new BehaviorSubject<IInstagramStatus | null>(null);
  private readonly subscriptions: Subscription[] = [];
  private statusSyncPromise: Promise<void> | null = null;

  private hasInitialized = false;
  private connectionPromise: Promise<void> | null = null;

  private readonly usersAgent = inject(UsersAgent);
  constructor() {
    this.subscriptions.push(
      this.signalRService.instagramLinking$.subscribe((notification: InstagramStatusNotification) =>
        this.setStatusFromNotification('linking', notification)
      ),
      this.signalRService.instagramRenewal$.subscribe((notification: InstagramStatusNotification) =>
        this.setStatusFromNotification('renewal', notification)
      )
    );
  }

  public get linkingStatus$(): Observable<IInstagramStatus | null> {
    return this.linkingStatusSubject.asObservable();
  }

  public get renewalStatus$(): Observable<IInstagramStatus | null> {
    return this.renewalStatusSubject.asObservable();
  }

  public get currentLinkingStatus(): IInstagramStatus | null {
    return this.linkingStatusSubject.value;
  }

  public get currentRenewalStatus(): IInstagramStatus | null {
    return this.renewalStatusSubject.value;
  }

  public init(options: InitOptions = {}): void {
    const { force = false } = options;

    if (this.hasInitialized && !force) {
      return;
    }

    this.hasInitialized = true;

    // When force is true (callback pages), don't auto-sync - let caller control the flow
    // This prevents duplicate requests when the component explicitly calls syncLatestStatuses()
    if (!force) {
      void this.ensureSignalRConnection();
      void this.syncLatestStatuses();
    }
  }

  public async ensureSignalRConnection(): Promise<boolean> {
    if (this.signalRService.isConnected) {
      // Wait a small buffer for backend to complete group assignment
      await this.delay(InstagramStatusService.CONNECTION_WAIT_MS);
      return true;
    }

    if (this.connectionPromise) {
      await this.connectionPromise;
      // Wait for group assignment after connection
      await this.delay(InstagramStatusService.CONNECTION_WAIT_MS);
      return this.signalRService.isConnected;
    }

    const token = this.authService.getToken();
    if (!token) {
      console.warn('[InstagramStatusService] No token available for SignalR connection');
      return false;
    }

    let connectionSuccessful = false;

    this.connectionPromise = this.signalRService
      .startConnection(token)
      .then(() => {
        connectionSuccessful = true;
      })
      .catch((error) => {
        console.error('[InstagramStatusService] Failed to start SignalR connection', error);
        connectionSuccessful = false;
      })
      .finally(() => {
        this.connectionPromise = null;
      });

    await this.connectionPromise;

    if (connectionSuccessful) {
      // Wait for backend to complete group assignment via OnConnectedAsync
      await this.delay(InstagramStatusService.CONNECTION_WAIT_MS);
    }

    return connectionSuccessful;
  }

  public async syncLatestStatuses(): Promise<void> {
    await Promise.all([
      this.syncLinkingStatus(),
      this.syncRenewalStatus(),
    ]);
  }

  public async syncLinkingStatus(): Promise<void> {
    await this.fetchStatus('linking', () => this.usersAgent.getLinkInstagramStatus());
  }

  public async syncRenewalStatus(): Promise<void> {
    await this.fetchStatus('renewal', () => this.usersAgent.getRenewInstagramStatus());
  }

  public markOperationAsPending(
    operation: InstagramOperationType,
    message?: string
  ): void {
    this.setManualStatus(operation, 'pending', message);
  }

  public setManualStatus(
    operation: InstagramOperationType,
    status: InstagramOperationState,
    message?: string
  ): void {
    if (status === 'not_found') {
      this.updateSubject(operation, null);
      return;
    }

    const statusEntry: IInstagramStatus = {
      operation,
      status,
      message: message ?? this.getDefaultMessage(operation, status),
      timestamp: new Date().toISOString(),
      isFinal: this.isFinalStatus(status),
    };

    this.updateSubject(operation, statusEntry);
  }

  public ngOnDestroy(): void {
    this.subscriptions.forEach((subscription) => subscription.unsubscribe());
    this.linkingStatusSubject.complete();
    this.renewalStatusSubject.complete();
  }

  private async fetchStatus(
    operation: InstagramOperationType,
    fetcher: () => Observable<IInstagramStatusResponse>
  ): Promise<void> {
    try {
      const response = await firstValueFrom(fetcher());
      this.applyStatusResponse(operation, response);
    } catch (error) {
      console.warn(`[InstagramStatusService] Failed to fetch ${operation} status:`, error);
    }
  }

  private delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  private applyStatusResponse(
    operation: InstagramOperationType,
    response?: IInstagramStatusResponse | null
  ): void {
    if (!response) {
      return;
    }

    const normalizedStatus = this.normalizeStatus(response.status);

    if (normalizedStatus === 'not_found') {
      this.updateSubject(operation, null);
      return;
    }

    const statusEntry: IInstagramStatus = {
      operation,
      status: normalizedStatus,
      message: response.message ?? this.getDefaultMessage(operation, normalizedStatus),
      timestamp: response.timestamp ?? new Date().toISOString(),
      isFinal: this.isFinalStatus(normalizedStatus),
    };

    this.updateSubject(operation, statusEntry);
  }

  private setStatusFromNotification(
    operation: InstagramOperationType,
    notification: InstagramStatusNotification
  ): void {
    console.warn(`[InstagramStatusService] Received ${operation} notification:`, notification);

    const normalizedStatus = this.normalizeStatus(notification.status);

    if (normalizedStatus === 'not_found') {
      this.updateSubject(operation, null);
      return;
    }

    const statusEntry: IInstagramStatus = {
      operation,
      status: normalizedStatus,
      message:
        notification.message ??
        this.getDefaultMessage(operation, normalizedStatus),
      timestamp: notification.timestamp ?? new Date().toISOString(),
      isFinal: this.isFinalStatus(normalizedStatus),
    };

    this.updateSubject(operation, statusEntry);
  }

  private normalizeStatus(status?: string): InstagramOperationState {
    switch (status) {
      case 'pending':
      case 'processing':
      case 'completed':
      case 'failed':
        return status;
      default:
        return 'not_found';
    }
  }

  private isFinalStatus(status: InstagramOperationState): boolean {
    return status === 'completed' || status === 'failed';
  }

  private getDefaultMessage(
    operation: InstagramOperationType,
    status: InstagramOperationState
  ): string {
    const isLinking = operation === 'linking';

    switch (status) {
      case 'pending':
        return isLinking
          ? 'Waiting for Instagram to confirm the account link.'
          : 'Waiting for Instagram to confirm the access renewal.';
      case 'processing':
        return isLinking
          ? 'Instagram is validating your account link request.'
          : 'Instagram is refreshing your access token.';
      case 'completed':
        return isLinking
          ? 'Instagram account linked successfully.'
          : 'Instagram access renewed successfully.';
      case 'failed':
        return isLinking
          ? 'Instagram account linking failed. Please try again.'
          : 'Instagram access renewal failed. Please try again.';
      default:
        return 'No Instagram operation in progress.';
    }
  }

  private updateSubject(
    operation: InstagramOperationType,
    status: IInstagramStatus | null
  ): void {
    const subject = this.getSubject(operation);
    subject.next(status);
  }

  private getSubject(
    operation: InstagramOperationType
  ): BehaviorSubject<IInstagramStatus | null> {
    return operation === 'linking'
      ? this.linkingStatusSubject
      : this.renewalStatusSubject;
  }
}
