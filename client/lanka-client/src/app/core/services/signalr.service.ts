import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject, BehaviorSubject } from 'rxjs';
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
  
  private hubConnection: HubConnection | null = null;
  private connectionState$ = new BehaviorSubject<'disconnected' | 'connecting' | 'connected'>('disconnected');

  constructor() {
    // Empty constructor
  }

  public get connectionState(): typeof this.connectionState$ {
    return this.connectionState$.asObservable();
  }

  public get isConnected(): boolean {
    return this.hubConnection?.state === 'Connected';
  }

  public async startConnection(accessToken: string): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      return;
    }

    this.connectionState$.next('connecting');

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
      this.connectionState$.next('disconnected');
    });

    this.hubConnection.onreconnected(() => {
      console.warn('[SignalRService] Reconnected');
      this.connectionState$.next('connected');
      this.joinUserGroup();
    });

    try {
      await this.hubConnection.start();
      console.warn('[SignalRService] Connection established');
      this.connectionState$.next('connected');
      await this.joinUserGroup();
    } catch (error) {
      console.error('[SignalRService] Error starting connection:', error);
      this.connectionState$.next('disconnected');
      throw error;
    }
  }

  public async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
      this.connectionState$.next('disconnected');
    }
  }

  private async joinUserGroup(): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      try {
        // Get user ID from token or storage - this is a simplified approach
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
        return payload.sub || payload.user_id || payload.userId || payload.id;
      }
    } catch (error) {
      console.error('[SignalRService] Error getting user ID:', error);
    }
    return null;
  }

}
