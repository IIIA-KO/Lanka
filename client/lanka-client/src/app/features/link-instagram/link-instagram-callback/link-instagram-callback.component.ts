import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { AgentService } from '../../../core/api/agent';
import { InstagramStatusService } from '../../../core/services/instagram-status.service';
import { IInstagramStatus } from '../../../core/models/instagram';
import { InstagramStatusBannerComponent } from '../../../shared/components/instagram-status-banner/instagram-status-banner.component';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-link-instagram-callback',
  templateUrl: './link-instagram-callback.component.html',
  styleUrl: './link-instagram-callback.component.css',
  imports: [CommonModule, InstagramStatusBannerComponent, TranslateModule],
})
export class LinkInstagramCallbackComponent implements OnInit, OnDestroy {
  public status$!: Observable<IInstagramStatus | null>;
  public isProcessing = true;
  public fallbackMessage = 'LINK_INSTAGRAM_CALLBACK.PREPARING';

  private returnUrl = '/profile';
  private subscriptions: Subscription[] = [];
  private navigationTimeout?: number;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(AgentService);
  private readonly instagramStatusService = inject(InstagramStatusService);

  constructor() {
    // Get returnUrl from query params if available
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/profile';
  }

  public ngOnInit(): void {
    void this.initialize();
  }

  public goBack(): void {
    this.router.navigate([this.returnUrl]);
  }

  public ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    if (this.navigationTimeout) {
      clearTimeout(this.navigationTimeout);
    }
  }

  private async initialize(): Promise<void> {
    this.instagramStatusService.init({ force: true });
    await this.instagramStatusService.ensureSignalRConnection();
    await this.instagramStatusService.syncLatestStatuses();
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

    const params = new URLSearchParams(window.location.search);
    const code = params.get('code');

    if (code) {
      this.instagramStatusService.markOperationAsPending(
        'linking',
        'LINK_INSTAGRAM_CALLBACK.STARTING'
      );

      this.api.Users.linkInstagram(code).subscribe({
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

    this.navigationTimeout = window.setTimeout(() => this.router.navigate([this.returnUrl]), delay);
  }

  private verifyAndNavigate(): void {
    if (this.navigationTimeout) {
      clearTimeout(this.navigationTimeout);
    }

    void this.instagramStatusService
      .syncLatestStatuses()
      .finally(() => this.scheduleNavigation());
  }
}
