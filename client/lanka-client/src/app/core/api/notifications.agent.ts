import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';

export interface INotification {
  id: string;
  campaignId?: string | null;
  threadId?: string | null;
  title: string;
  body: string;
  isRead: boolean;
  createdAtUtc: string;
  type?: 'campaign' | 'chat';
}

const BASE_URL = environment.apiUrl;

@Injectable({ providedIn: 'root' })
export class NotificationsAgent {
  private readonly http = inject(HttpClient);

  public getNotifications(): Observable<INotification[]> {
    return this.http.get<INotification[]>(`${BASE_URL}/notifications`);
  }

  public markRead(id: string): Observable<void> {
    return this.http.put<void>(`${BASE_URL}/notifications/${id}/read`, {});
  }

  public markAllRead(): Observable<void> {
    return this.http.put<void>(`${BASE_URL}/notifications/read-all`, {});
  }
}
