import { Injectable, DestroyRef, inject, OnDestroy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { INotification, NotificationsAgent } from '../api/notifications.agent';
import { SignalRService } from './signalr.service';

@Injectable({ providedIn: 'root' })
export class NotificationsService implements OnDestroy {
  private readonly notificationsSubject = new BehaviorSubject<INotification[]>([]);
  private readonly agent = inject(NotificationsAgent);
  private readonly signalR = inject(SignalRService);
  private readonly destroyRef = inject(DestroyRef);

  public readonly notifications$ = this.notificationsSubject.asObservable();
  public readonly unreadCount$ = new BehaviorSubject<number>(0);

  constructor() {
    this.signalR.campaignNotification$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
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
    this.agent.markAllRead().subscribe({
      next: () => {
        const updated = this.notificationsSubject.value.map(n => ({ ...n, isRead: true }));
        this.notificationsSubject.next(updated);
        this.unreadCount$.next(0);
      },
    });
  }

  public ngOnDestroy(): void {
    this.notificationsSubject.complete();
    this.unreadCount$.complete();
  }
}
