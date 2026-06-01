import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

const BASE_URL = environment.apiUrl;

export interface IChatMessage {
  id: string;
  threadId: string;
  senderBloggerId?: string | null;
  senderFirstName: string;
  senderLastName: string;
  content: string;
  isSystem: boolean;
  isDeleted: boolean;
  editedAtUtc?: string | null;
  readAtUtc?: string | null;
  createdAtUtc: string;
  recipientBloggerId?: string | null;
}

export interface IChatMessageDeletedEvent {
  threadId: string;
  messageId: string;
}

export interface IChatMessagesReadEvent {
  threadId: string;
  readerBloggerId: string;
  readAtUtc: string;
}

export interface IChatThread {
  id: string;
  otherParticipantId: string;
  otherParticipantFirstName: string;
  otherParticipantLastName: string;
  otherParticipantInstagramUsername?: string | null;
  otherParticipantProfilePhoto?: string | null;
  campaignId?: string | null;
  campaignName?: string | null;
  offerId?: string | null;
  offerName?: string | null;
  lastMessageContent?: string | null;
  lastMessageIsSystem: boolean;
  lastMessageCreatedAtUtc?: string | null;
  unreadCount: number;
  updatedAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class ChatAgent {
  private readonly http = inject(HttpClient);

  public getThreads(): Observable<IChatThread[]> {
    return this.http.get<IChatThread[]>(`${BASE_URL}/chats`);
  }

  public startThread(
    participantBloggerId: string,
    offerId?: string | null,
    campaignId?: string | null
  ): Observable<IChatThread> {
    return this.http.post<IChatThread>(`${BASE_URL}/chats/start`, {
      participantBloggerId,
      offerId,
      campaignId,
    });
  }

  public getMessages(threadId: string, before?: string, limit = 30): Observable<IChatMessage[]> {
    let params = new HttpParams().set('limit', limit);
    if (before) {
      params = params.set('before', before);
    }

    return this.http.get<IChatMessage[]>(`${BASE_URL}/chats/${threadId}/messages`, { params });
  }

  public sendMessage(threadId: string, content: string): Observable<IChatMessage> {
    return this.http.post<IChatMessage>(`${BASE_URL}/chats/${threadId}/messages`, { content });
  }

  public editMessage(threadId: string, messageId: string, newContent: string): Observable<IChatMessage> {
    return this.http.patch<IChatMessage>(
      `${BASE_URL}/chats/${threadId}/messages/${messageId}`,
      { newContent }
    );
  }

  public deleteMessage(threadId: string, messageId: string): Observable<void> {
    return this.http.delete<void>(`${BASE_URL}/chats/${threadId}/messages/${messageId}`);
  }

  public markRead(threadId: string): Observable<void> {
    return this.http.put<void>(`${BASE_URL}/chats/${threadId}/messages/read`, {});
  }
}
