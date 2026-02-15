import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject, BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';

export interface InstagramStatusNotification {
  type: 'instagram_linking' | 'instagram_renewal';
  status: 'pending' | 'processing' | 'completed' | 'failed';
  message?: string;
  timestamp: string;
}

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  // Observables for different notification types
  public instagramLinking$ = new Subject<InstagramStatusNotification>();
  public instagramRenewal$ = new Subject<InstagramStatusNotification>();
  public readonly connectionState$;

  private hubConnection: HubConnection | null = null;
  private readonly connectionStateSubject = new BehaviorSubject<'disconnected' | 'connecting' | 'connected'>('disconnected');

  constructor() {
    this.connectionState$ = this.connectionStateSubject.asObservable();
  }

  public get connectionState(): Observable<'disconnected' | 'connecting' | 'connected'> {
    return this.connectionState$;
  }

  public get isConnected(): boolean {
    return this.hubConnection?.state === 'Connected';
  }

  public get currentState(): 'disconnected' | 'connecting' | 'connected' {
    return this.connectionStateSubject.value;
  }

  public async startConnection(accessToken: string): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      return;
    }

    this.connectionStateSubject.next('connecting');

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/instagram`, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    // Set up event handlers
    this.hubConnection.on('InstagramLinkingStatus', (notification: InstagramStatusNotification) => {
      console.warn('[SignalRService] Received InstagramLinkingStatus:', notification);
      this.instagramLinking$.next(notification);
    });

    this.hubConnection.on('InstagramRenewalStatus', (notification: InstagramStatusNotification) => {
      console.warn('[SignalRService] Received InstagramRenewalStatus:', notification);
      this.instagramRenewal$.next(notification);
    });

    this.hubConnection.onclose(() => {
      console.warn('[SignalRService] Connection closed');
      this.connectionStateSubject.next('disconnected');
    });

    this.hubConnection.onreconnecting(() => {
      console.warn('[SignalRService] Reconnecting...');
      this.connectionStateSubject.next('connecting');
    });

    this.hubConnection.onreconnected(() => {
      console.warn('[SignalRService] Reconnected');
      this.connectionStateSubject.next('connected');
      // Backend automatically joins user to their group on connection via claims
    });

    try {
      await this.hubConnection.start();
      console.warn('[SignalRService] Connection established - backend will auto-join user group via claims');
      this.connectionStateSubject.next('connected');
      // Backend automatically joins user to their group on connection via claims
    } catch (error) {
      console.error('[SignalRService] Error starting connection:', error);
      this.connectionStateSubject.next('disconnected');
      throw error;
    }
  }

  public async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
      this.connectionStateSubject.next('disconnected');
    }
  }
}
