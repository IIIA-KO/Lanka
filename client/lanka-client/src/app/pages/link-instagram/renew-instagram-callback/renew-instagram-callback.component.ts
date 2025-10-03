import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { AgentService } from '../../../core/api/agent';
import { SignalRService, InstagramStatusNotification } from '../../../core/services/signalr.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-renew-instagram-callback',
  template: `
    <div class="flex items-center justify-center h-screen bg-gray-50">
      <div class="text-center">
        <div class="text-[1.50rem] text-gray-700 mb-8">
          {{ message }}
        </div>
        <div *ngIf="isProcessing" class="mt-4">
          <div class="inline-flex items-center">
            <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-gray-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            <span class="text-gray-600">Processing...</span>
          </div>
        </div>
      </div>
    </div>
  `,
  imports: [CommonModule, TranslateModule],
})
export class RenewInstagramCallbackComponent implements OnInit, OnDestroy {
  public message = 'Loading...';
  public isProcessing = false;
  private isFetched = false;
  private returnUrl = '/profile';
  private readonly subscriptions: Subscription[] = [];
  private fallbackTimeout?: ReturnType<typeof setTimeout>;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(AgentService);
  private readonly signalRService = inject(SignalRService);

  constructor() {
    // Get returnUrl from query params if available, default to profile for renewal
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/profile';
  }

  public ngOnInit(): void {
    if (this.isFetched) {
      return;
    }
    this.isFetched = true;
    this.message = 'Renewing Instagram Access...';

    // Set up SignalR subscription for real-time updates
    const signalRSub = this.signalRService.instagramRenewal$.subscribe({
      next: (notification: InstagramStatusNotification) => {
        this.handleStatusUpdate(notification);
      },
      error: (error) => {
        console.error('[RenewInstagramCallback] SignalR notification error:', error);
      }
    });
    this.subscriptions.push(signalRSub);

    // Also check connection state
    this.signalRService.connectionState.subscribe(state => {
      console.warn('[RenewInstagramCallback] SignalR connection state:', state);
    });

    const params = new URLSearchParams(window.location.search);
    const code = params.get('code');

    if (code) {
      this.api.Users.renewInstagramAccess(code).subscribe({
        next: (response) => {
          const res = response as { status: number };
          if (res.status === 202) {
            this.message = 'Renewing your Instagram access...';
            this.isProcessing = true;
          } else {
            this.message = 'Instagram access renewed successfully!';
            this.isProcessing = false;
            setTimeout(() => this.router.navigate([this.returnUrl]), 2000);
          }
        },
        error: (error) => {
          this.message = 'Failed to start access renewal';
          this.isProcessing = false;
          console.error('[RenewInstagramCallback] Error:', error);
        },
      });
    } else {
      this.message = 'No code provided for renewal';
      this.router.navigate([this.returnUrl]);
    }
  }

  public ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    if (this.fallbackTimeout) {
      clearTimeout(this.fallbackTimeout);
    }
  }

  private handleStatusUpdate(notification: InstagramStatusNotification): void {
    console.warn('[RenewInstagramCallback] Received renewal status update:', notification);

    switch (notification.status) {
      case 'processing':
        this.message = 'Processing Instagram access renewal...';
        this.isProcessing = true;
        break;
      case 'completed':
        this.message = 'Instagram access renewed successfully!';
        this.isProcessing = false;
        setTimeout(() => this.router.navigate([this.returnUrl]), 1500);
        break;
      case 'failed':
        this.message = notification.message || 'Failed to renew Instagram access';
        this.isProcessing = false;
        break;
      default:
        this.message = notification.message || `Status: ${notification.status}`;
        this.isProcessing = false;
    }
  }
}

