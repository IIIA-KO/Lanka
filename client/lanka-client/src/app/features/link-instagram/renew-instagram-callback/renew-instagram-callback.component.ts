import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Observable, Subscription } from 'rxjs';
import { AgentService } from '../../../core/api/agent';
import { InstagramStatusService } from '../../../core/services/instagram-status.service';
import { IInstagramStatus } from '../../../core/models/instagram';
import { InstagramStatusBannerComponent } from '../../../shared/components/instagram-status-banner/instagram-status-banner.component';
import { FriendlyErrorService } from '../../../core/services/friendly-error.service';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-renew-instagram-callback',
  standalone: true,
  imports: [
    CommonModule,
    InstagramStatusBannerComponent,
    TranslateModule,
    ButtonModule,
    CardModule,
    ProgressSpinnerModule,
  ],
  templateUrl: './renew-instagram-callback.component.html',
  styleUrl: '../shared/callback-shared.css',
})
export class RenewInstagramCallbackComponent implements OnInit, OnDestroy {
  public status$!: Observable<IInstagramStatus | null>;
  public isProcessing = true;
  public fallbackMessage = 'RENEW_INSTAGRAM_CALLBACK.PREPARING';

  private returnUrl = '/profile';
  private subscriptions: Subscription[] = [];
  private navigationTimeout?: ReturnType<typeof setTimeout>;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(AgentService);
  private readonly instagramStatusService = inject(InstagramStatusService);
  private readonly friendlyErrorService = inject(FriendlyErrorService);

  constructor() {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/profile';
  }

  public ngOnInit(): void {
    void this.initialize();
  }

  public goBack(): void {
    this.cleanup();
    this.router.navigate([this.returnUrl]);
  }

  public ngOnDestroy(): void {
    this.cleanup();
  }

  private cleanup(): void {
    this.subscriptions.forEach((subscription) => subscription.unsubscribe());
    if (this.navigationTimeout) {
      clearTimeout(this.navigationTimeout);
    }
  }

  private async initialize(): Promise<void> {
    this.instagramStatusService.init({ force: true });

    // Ensure SignalR is connected first
    await this.instagramStatusService.ensureSignalRConnection();

    await this.instagramStatusService.syncRenewalStatus();
    this.status$ = this.instagramStatusService.renewalStatus$;

    const statusSub = this.status$.subscribe((status) => {
      if (!status) {
        return;
      }

      this.isProcessing = !status.isFinal;

      if (status.status === 'completed') {
        this.fallbackMessage = 'RENEW_INSTAGRAM_CALLBACK.SUCCESS_REDIRECT';
        this.verifyAndNavigate();
      }

      if (status.status === 'failed') {
        this.fallbackMessage = status.message;
      }
    });

    this.subscriptions.push(statusSub);

    // If a renewal operation is already running (page refresh, previous attempt),
    // just wait for the result instead of firing a duplicate POST
    const currentStatus = this.instagramStatusService.currentRenewalStatus;
    if (currentStatus && !currentStatus.isFinal) {
      this.fallbackMessage = 'RENEW_INSTAGRAM_CALLBACK.PROCESSING';
      return;
    }

    if (currentStatus?.status === 'completed') {
      return;
    }

    const params = new URLSearchParams(window.location.search);
    const authCode = params.get('code');

    if (authCode) {
      this.instagramStatusService.markOperationAsPending(
        'renewal',
        'RENEW_INSTAGRAM_CALLBACK.STARTING'
      );

      this.api.Users.renewInstagramAccess(authCode).subscribe({
        next: (response) => {
          if (response.status === 202) {
            this.instagramStatusService.markOperationAsPending(
              'renewal',
              'RENEW_INSTAGRAM_CALLBACK.WAITING_CONFIRMATION'
            );
          } else {
            this.instagramStatusService.setManualStatus(
              'renewal',
              'completed',
              'RENEW_INSTAGRAM_CALLBACK.SUCCESS'
            );
          }
        },
        error: (error) => {
          this.isProcessing = false;
          const friendlyError = this.friendlyErrorService.toFriendlyError(error, {
            badRequestMessage: 'RENEW_INSTAGRAM_CALLBACK.REJECTED',
            fallbackMessage: 'RENEW_INSTAGRAM_CALLBACK.START_FAILED',
            networkMessage: 'RENEW_INSTAGRAM_CALLBACK.NETWORK_ERROR'
          });

          this.instagramStatusService.setManualStatus(
            'renewal',
            'failed',
            friendlyError.message
          );
        },
      });
    } else {
      this.isProcessing = false;
      this.instagramStatusService.setManualStatus(
        'renewal',
        'failed',
        'RENEW_INSTAGRAM_CALLBACK.NO_CODE'
      );
    }
  }

  private scheduleNavigation(delay = 2000): void {
    if (this.navigationTimeout) {
      clearTimeout(this.navigationTimeout);
    }

    this.navigationTimeout = setTimeout(() => this.router.navigate([this.returnUrl]), delay);
  }

  private verifyAndNavigate(): void {
    if (this.navigationTimeout) {
      clearTimeout(this.navigationTimeout);
    }

    // Navigate directly — no need to re-sync since we already know renewal completed
    this.scheduleNavigation();
  }
}
