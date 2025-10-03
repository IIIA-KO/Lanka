import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { AgentService } from '../../../core/api/agent';
import { SignalRService, InstagramStatusNotification } from '../../../core/services/signalr.service';

@Component({
  selector: 'app-link-instagram-callback',
  templateUrl: './link-instagram-callback.component.html',
  imports: [CommonModule, TranslateModule],
})
export class LinkInstagramCallbackComponent implements OnInit, OnDestroy {
  public message = 'Loading...';
  public isProcessing = false;
  private isFetched = false;
  private returnUrl = '/pact';
  private readonly subscriptions: Subscription[] = [];
  private fallbackTimeout?: ReturnType<typeof setTimeout>;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(AgentService);
  private readonly signalRService = inject(SignalRService);
  private readonly translate = inject(TranslateService);

  constructor() {
    // Get returnUrl from query params if available
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/pact';
  }

  public ngOnInit(): void {
    if (this.isFetched) {
      return;
    }

    this.isFetched = true;
    this.message = 'Linking Your Instagram Account...';

    // Set up SignalR subscription for real-time updates
    console.warn('[LinkInstagramCallback] Setting up SignalR subscription');
    const signalRSub = this.signalRService.instagramLinking$.subscribe({
      next: (notification: InstagramStatusNotification) => {
        console.warn('[LinkInstagramCallback] Received notification:', notification);
        this.handleStatusUpdate(notification);
      },
      error: (error) => {
        console.error('[LinkInstagramCallback] SignalR notification error:', error);
      }
    });
    this.subscriptions.push(signalRSub);
    
    // Also check connection state
    this.signalRService.connectionState.subscribe(state => {
      console.warn('[LinkInstagramCallback] SignalR connection state:', state);
    });

    const params = new URLSearchParams(window.location.search);
    const code = params.get('code');
    console.warn('[LinkInstagramCallback] Linking Instagram with code:', code);

    if (code) {
      this.api.Users.linkInstagram(code).subscribe({
        next: (response) => {
          const res = response as { status: number };
          if (res.status === 202) {
            this.message = 'Linking your Instagram account...';
            this.isProcessing = true;
            // Now we wait for SignalR notifications for real-time updates
          } else {
            // In case backend still returns 204 for some paths
            this.message = 'Instagram account linked successfully!';
            this.isProcessing = false;
            setTimeout(() => this.router.navigate([this.returnUrl]), 2000);
          }
        },
        error: (error) => {
          this.message = 'Failed to start linking Instagram account';
          this.isProcessing = false;
          console.error('[LinkInstagramCallback] Error:', error);
        },
      });
    } else {
      this.message = 'No code provided for linking Instagram account';
      console.warn('[LinkInstagramCallback] No code found in URL parameters');
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
    console.warn('[LinkInstagramCallback] Received linking status update:', notification);
    
    switch (notification.status) {
      case 'processing':
        this.message = 'Processing Instagram account linking...';
        this.isProcessing = true;
        break;
      case 'completed':
        this.message = 'Instagram account linked successfully!';
        this.isProcessing = false;
        setTimeout(() => this.router.navigate([this.returnUrl]), 1500);
        break;
      case 'failed':
        this.message = notification.message || 'Failed to link Instagram account';
        this.isProcessing = false;
        break;
      default:
        this.message = notification.message || `Status: ${notification.status}`;
        this.isProcessing = false;
    }
  }
}
