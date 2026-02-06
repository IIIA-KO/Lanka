import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { AgentService } from '../../../core/api/agent';
import { InstagramStatusService } from '../../../core/services/instagram-status.service';
import { IInstagramStatus } from '../../../core/models/instagram';
import { InstagramStatusBannerComponent } from '../../../shared/components/instagram-status-banner/instagram-status-banner.component';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-link-instagram-callback',
  templateUrl: './link-instagram-callback.component.html',
  styleUrl: '../shared/callback-shared.css',
  imports: [
    CommonModule,
    InstagramStatusBannerComponent,
    TranslateModule,
    ButtonModule,
    CardModule,
    ProgressSpinnerModule,
  ],
})
export class LinkInstagramCallbackComponent implements OnInit, OnDestroy {
  public status$!: Observable<IInstagramStatus | null>;
  public isProcessing = true;
  public fallbackMessage = 'LINK_INSTAGRAM_CALLBACK.PREPARING';

  private returnUrl = '/profile';
  private subscriptions: Subscription[] = [];
  private navigationTimeout?: ReturnType<typeof setTimeout>;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(AgentService);
  private readonly instagramStatusService = inject(InstagramStatusService);

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
    this.subscriptions.forEach(sub => sub.unsubscribe());
    if (this.navigationTimeout) {
      clearTimeout(this.navigationTimeout);
    }
  }

  private async initialize(): Promise<void> {
    this.instagramStatusService.init({ force: true });

    // Ensure SignalR is connected first
    await this.instagramStatusService.ensureSignalRConnection();

    await this.instagramStatusService.syncLinkingStatus();
    this.status$ = this.instagramStatusService.linkingStatus$;

    const statusSub = this.status$.subscribe((status) => {
      if (!status) {
        return;
      }

      this.isProcessing = !status.isFinal;

      if (status.status === 'completed') {
        this.fallbackMessage = 'LINK_INSTAGRAM_CALLBACK.LINKED_REDIRECT';
        this.verifyAndNavigate();
      }

      if (status.status === 'failed') {
        this.fallbackMessage = status.message;
      }
    });

    this.subscriptions.push(statusSub);

    // If a linking operation is already running (page refresh, previous attempt),
    // just wait for the result instead of firing a duplicate POST
    const currentStatus = this.instagramStatusService.currentLinkingStatus;
    if (currentStatus && !currentStatus.isFinal) {
      this.fallbackMessage = 'LINK_INSTAGRAM_CALLBACK.PROCESSING';
      return;
    }

    if (currentStatus?.status === 'completed') {
      return;
    }

    const params = new URLSearchParams(window.location.search);
    const authCode = params.get('code');

    if (authCode) {
      this.instagramStatusService.markOperationAsPending(
        'linking',
        'LINK_INSTAGRAM_CALLBACK.STARTING'
      );

      this.api.Users.linkInstagram(authCode).subscribe({
        next: (response) => {
          if (response.status === 202) {
            this.instagramStatusService.markOperationAsPending(
              'linking',
              'LINK_INSTAGRAM_CALLBACK.WAITING_CONFIRMATION'
            );
          } else {
            this.instagramStatusService.setManualStatus(
              'linking',
              'completed',
              'LINK_INSTAGRAM_CALLBACK.LINKED_SUCCESS'
            );
          }
        },
        error: (error) => {
          this.isProcessing = false;
          this.instagramStatusService.setManualStatus(
            'linking',
            'failed',
            error?.message || 'LINK_INSTAGRAM_CALLBACK.START_FAILED'
          );
        },
      });
    } else {
      this.isProcessing = false;
      this.instagramStatusService.setManualStatus(
        'linking',
        'failed',
        'LINK_INSTAGRAM_CALLBACK.NO_CODE'
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

    // Navigate directly — no need to re-sync since we already know linking completed
    this.scheduleNavigation();
  }
}
