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
      this.instagramLinking$.next(notification);
    });

    this.hubConnection.on('InstagramRenewalStatus', (notification: InstagramStatusNotification) => {
      this.instagramRenewal$.next(notification);
    });

    this.hubConnection.onclose(() => {
      console.warn('[SignalRService] Connection closed');
      this.connectionStateSubject.next('disconnected');
    });

    this.hubConnection.onreconnected(() => {
      console.warn('[SignalRService] Reconnected');
      this.connectionStateSubject.next('connected');
      this.joinUserGroup();
    });

    try {
      await this.hubConnection.start();
      console.warn('[SignalRService] Connection established');
      this.connectionStateSubject.next('connected');
      await this.joinUserGroup();
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

  private async joinUserGroup(): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      try {
        const userId = this.getUserId();
        if (userId) {
          await this.hubConnection.invoke('JoinUserGroup', userId);
        }
      } catch (error) {
        console.error('[SignalRService] Error joining user group:', error);
      }
    }
  }

  private getUserId(): string | null {
    try {
      const accessToken = localStorage.getItem('access_token');
      if (accessToken) {
        const payload = JSON.parse(atob(accessToken.split('.')[1]));
        const possibleKeys = [
          'bloggerId',
          'BloggerId',
          'blogger_id',
          'userId',
          'UserId',
          'user_id',
          'id',
          'sub'
        ];

        for (const key of possibleKeys) {
          const value = payload[key];
          if (typeof value === 'string' && value.length > 0) {
            return value;
          }
        }
      }
    } catch (error) {
      console.error('[SignalRService] Error getting user ID:', error);
    }
    return null;
  }

}
