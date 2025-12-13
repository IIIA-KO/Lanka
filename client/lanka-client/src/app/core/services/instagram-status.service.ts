import { Injectable, OnDestroy, inject } from '@angular/core';
import { BehaviorSubject, Observable, Subscription, firstValueFrom, forkJoin } from 'rxjs';
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

  // Status updates flow through SignalR notifications with an HTTP snapshot fallback
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

  public init(options: InitOptions = {}): void {
    const { force = false } = options;

    if (this.hasInitialized && !force) {
      return;
    }

    this.hasInitialized = true;
    void this.ensureSignalRConnection();
    void this.syncLatestStatuses();
  }

  public async ensureSignalRConnection(): Promise<void> {
    if (this.signalRService.isConnected) {
      return;
    }

    if (this.connectionPromise) {
      return this.connectionPromise;
    }

    const token = this.authService.getToken();
    if (!token) {
      return;
    }

    this.connectionPromise = this.signalRService
      .startConnection(token)
      .catch((error) => {
        console.error('[InstagramStatusService] Failed to start SignalR connection', error);
      })
      .finally(() => {
        this.connectionPromise = null;
      });

    return this.connectionPromise;
  }

  public async syncLatestStatuses(): Promise<void> {
    if (this.statusSyncPromise) {
      return this.statusSyncPromise;
    }

    this.statusSyncPromise = (async () => {
      try {
        const responses = await firstValueFrom(
          forkJoin({
            linking: this.usersAgent.getLinkInstagramStatus(),
            renewal: this.usersAgent.getRenewInstagramStatus(),
          })
        );

        this.applyStatusResponse('linking', responses.linking);
        this.applyStatusResponse('renewal', responses.renewal);
      } catch (error) {
        console.error('[InstagramStatusService] Failed to fetch latest Instagram statuses', error);
      } finally {
        this.statusSyncPromise = null;
      }
    })();

    return this.statusSyncPromise;
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
