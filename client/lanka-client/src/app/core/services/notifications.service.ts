import { Injectable, DestroyRef, inject, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { INotification, NotificationsAgent } from '../api/notifications.agent';
import { SignalRService } from './signalr.service';
import { BloggersAgent } from '../api/bloggers.agent';

@Injectable({ providedIn: 'root' })
export class NotificationsService implements OnDestroy {
  public readonly unreadCount$ = new BehaviorSubject<number>(0);

  private readonly notificationsSubject = new BehaviorSubject<INotification[]>([]);
  private readonly agent = inject(NotificationsAgent);
  private readonly signalR = inject(SignalRService);
  private readonly bloggersAgent = inject(BloggersAgent);
  private readonly destroyRef = inject(DestroyRef);
  private activeChatThreadId: string | null = null;
  private currentBloggerId: string | null = null;

  constructor() {
    this.bloggersAgent.getProfile().subscribe({
      next: profile => { this.currentBloggerId = profile.id.toLowerCase(); },
    });

    this.signalR.campaignNotification$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());

    this.signalR.chatMessageSent$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(message => {
        if (
          message.isSystem ||
          message.isDeleted ||
          message.threadId === this.activeChatThreadId ||
          !this.isForCurrentBlogger(message.recipientBloggerId)
        ) {
          return;
        }

        const senderName = `${message.senderFirstName ?? ''} ${message.senderLastName ?? ''}`.trim();
        const notification: INotification = {
          id: `chat-${message.id}`,
          threadId: message.threadId,
          title: senderName || 'New message',
          body: message.content,
          isRead: false,
          createdAtUtc: message.createdAtUtc,
          type: 'chat',
        };

        const existing = this.notificationsSubject.value.filter(n => n.id !== notification.id);
        const notifications = [notification, ...existing];
        this.notificationsSubject.next(notifications);
        this.unreadCount$.next(notifications.filter(n => !n.isRead).length);
      });
  }

  public get notifications$(): Observable<INotification[]> {
    return this.notificationsSubject.asObservable();
  }

  public load(): void {
    this.agent.getNotifications().subscribe({
      next: (notifications) => {
        this.notificationsSubject.next(notifications);
        this.unreadCount$.next(notifications.filter(n => !n.isRead).length);
      },
    });
  }

  public markRead(id: string): void {
    const notification = this.notificationsSubject.value.find(n => n.id === id);
    if (notification?.type === 'chat') {
      const updated = this.notificationsSubject.value.map(n =>
        n.id === id ? { ...n, isRead: true } : n
      );
      this.notificationsSubject.next(updated);
      this.unreadCount$.next(updated.filter(n => !n.isRead).length);
      return;
    }

    this.agent.markRead(id).subscribe({
      next: () => {
        const updated = this.notificationsSubject.value.map(n =>
          n.id === id ? { ...n, isRead: true } : n
        );
        this.notificationsSubject.next(updated);
        this.unreadCount$.next(updated.filter(n => !n.isRead).length);
      },
    });
  }

  public markAllRead(): void {
    const hasPersistedNotifications = this.notificationsSubject.value.some(n => n.type !== 'chat');
    if (!hasPersistedNotifications) {
      this.markAllReadLocally();
      return;
    }

    this.agent.markAllRead().subscribe({
      next: () => this.markAllReadLocally(),
    });
  }

  public setActiveChatThread(threadId: string | null): void {
    this.activeChatThreadId = threadId;

    if (!threadId) {
      return;
    }

    const updated = this.notificationsSubject.value.map(n =>
      n.threadId === threadId ? { ...n, isRead: true } : n
    );
    this.notificationsSubject.next(updated);
    this.unreadCount$.next(updated.filter(n => !n.isRead).length);
  }

  public ngOnDestroy(): void {
    this.notificationsSubject.complete();
    this.unreadCount$.complete();
  }

  private markAllReadLocally(): void {
    const updated = this.notificationsSubject.value.map(n => ({ ...n, isRead: true }));
    this.notificationsSubject.next(updated);
    this.unreadCount$.next(0);
  }

  private isForCurrentBlogger(recipientBloggerId?: string | null): boolean {
    return !!recipientBloggerId &&
      !!this.currentBloggerId &&
      recipientBloggerId.toLowerCase() === this.currentBloggerId;
  }
}
