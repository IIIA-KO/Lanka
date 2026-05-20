import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { BadgeModule } from 'primeng/badge';
import { PopoverModule } from 'primeng/popover';
import { TooltipModule } from 'primeng/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { NotificationsService } from '../../../core/services/notifications.service';
import { INotification } from '../../../core/api/notifications.agent';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ButtonModule,
    BadgeModule,
    PopoverModule,
    TooltipModule,
    TranslateModule,
  ],
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.css',
})
export class NotificationBellComponent implements OnInit {
  public readonly notificationsService = inject(NotificationsService);
  public readonly notifications$ = this.notificationsService.notifications$;
  public readonly unreadCount$ = this.notificationsService.unreadCount$;

  public ngOnInit(): void {
    this.notificationsService.load();
  }

  public markRead(notification: INotification, event: Event): void {
    event.stopPropagation();
    if (!notification.isRead) {
      this.notificationsService.markRead(notification.id);
    }
  }

  public markAllRead(): void {
    this.notificationsService.markAllRead();
  }

  public trackById(_index: number, notification: INotification): string {
    return notification.id;
  }

  public notificationLink(notification: INotification): unknown[] {
    if (notification.threadId) {
      return ['/chats', notification.threadId];
    }

    return ['/campaigns', notification.campaignId];
  }
}
