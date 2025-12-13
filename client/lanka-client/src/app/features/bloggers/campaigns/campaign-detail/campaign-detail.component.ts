import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { catchError, of } from 'rxjs';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { DialogModule } from 'primeng/dialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { TextareaModule } from 'primeng/textarea';
import { FormsModule } from '@angular/forms';

import { CampaignsAgent } from '../../../../core/api/campaigns.agent';
import { AuthService } from '../../../../core/services/auth/auth.service';
import { SnackbarService } from '../../../../core/services/snackbar/snackbar.service';
import { ICampaign, CampaignStatus } from '../../../../core/models/campaigns';
import { DeliverableTemplatesService } from '../../../../core/services/deliverable-templates.service';

@Component({
  standalone: true,
  selector: 'app-campaign-detail',
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ButtonModule,
    CardModule,
    TagModule,
    DividerModule,
    DialogModule,
    ProgressSpinnerModule,
    MessageModule,
    TextareaModule
  ],
  templateUrl: './campaign-detail.component.html',
  styleUrls: ['./campaign-detail.component.css']
})
export class CampaignDetailComponent implements OnInit {
  public campaign: ICampaign | null = null;
  public loading = false;
  public error: string | null = null;
  public isCreator = false;
  public isClient = false;

  // Dialog states
  public showAcceptDialog = false;
  public showRejectDialog = false;
  public showDoneDialog = false;
  public showCompleteDialog = false;
  public showCancelDialog = false;
  public rejectReason = '';
  public processingAction = false;

  public readonly CampaignStatus = CampaignStatus;
  public readonly templatesService = inject(DeliverableTemplatesService);

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly campaignsAgent = inject(CampaignsAgent);
  private readonly authService = inject(AuthService);
  private readonly snackbarService = inject(SnackbarService);

  public ngOnInit(): void {
    const campaignId = this.route.snapshot.paramMap.get('id');
    if (campaignId) {
      this.loadCampaign(campaignId);
    } else {
      this.error = 'Campaign ID not found';
    }
  }

  public getStatusColor(status: string): string {
    switch (status) {
      case CampaignStatus.Pending:
        return 'warn';
      case CampaignStatus.Confirmed:
        return 'info';
      case CampaignStatus.Done:
        return 'secondary';
      case CampaignStatus.Completed:
        return 'success';
      case CampaignStatus.Rejected:
      case CampaignStatus.Cancelled:
        return 'danger';
      default:
        return 'info';
    }
  }

  public canAccept(): boolean {
    return this.isCreator && this.campaign?.status === CampaignStatus.Pending;
  }

  public canReject(): boolean {
    return this.isCreator && this.campaign?.status === CampaignStatus.Pending;
  }

  public canMarkAsDone(): boolean {
    return this.isCreator && this.campaign?.status === CampaignStatus.Confirmed;
  }

  public canComplete(): boolean {
    return this.isClient && this.campaign?.status === CampaignStatus.Done;
  }

  public canCancel(): boolean {
    return this.campaign?.status === CampaignStatus.Confirmed;
  }

  public openAcceptDialog(): void {
    this.showAcceptDialog = true;
  }

  public openRejectDialog(): void {
    this.rejectReason = '';
    this.showRejectDialog = true;
  }

  public openDoneDialog(): void {
    this.showDoneDialog = true;
  }

  public openCompleteDialog(): void {
    this.showCompleteDialog = true;
  }

  public openCancelDialog(): void {
    this.showCancelDialog = true;
  }

  public confirmAccept(): void {
    if (!this.campaign) return;

    this.processingAction = true;
    this.campaignsAgent.confirmCampaign(this.campaign.id).pipe(
      catchError(() => {
        this.snackbarService.showError('Failed to accept campaign');
        return of(null);
      })
    ).subscribe({
      next: () => {
        this.snackbarService.showSuccess('Campaign accepted successfully');
        this.showAcceptDialog = false;
        this.loadCampaign(this.campaign!.id);
      },
      complete: () => {
        this.processingAction = false;
      }
    });
  }

  public confirmReject(): void {
    if (!this.campaign) return;

    this.processingAction = true;
    this.campaignsAgent.rejectCampaign(this.campaign.id).pipe(
      catchError(() => {
        this.snackbarService.showError('Failed to reject campaign');
        return of(null);
      })
    ).subscribe({
      next: () => {
        this.snackbarService.showSuccess('Campaign rejected');
        this.showRejectDialog = false;
        this.loadCampaign(this.campaign!.id);
      },
      complete: () => {
        this.processingAction = false;
      }
    });
  }

  public confirmDone(): void {
    if (!this.campaign) return;

    this.processingAction = true;
    this.campaignsAgent.markCampaignAsDone(this.campaign.id).pipe(
      catchError(() => {
        this.snackbarService.showError('Failed to mark campaign as done');
        return of(null);
      })
    ).subscribe({
      next: () => {
        this.snackbarService.showSuccess('Campaign marked as done');
        this.showDoneDialog = false;
        this.loadCampaign(this.campaign!.id);
      },
      complete: () => {
        this.processingAction = false;
      }
    });
  }

  public confirmComplete(): void {
    if (!this.campaign) return;

    this.processingAction = true;
    this.campaignsAgent.completeCampaign(this.campaign.id).pipe(
      catchError(() => {
        this.snackbarService.showError('Failed to complete campaign');
        return of(null);
      })
    ).subscribe({
      next: () => {
        this.snackbarService.showSuccess('Campaign completed successfully');
        this.showCompleteDialog = false;
        this.loadCampaign(this.campaign!.id);
      },
      complete: () => {
        this.processingAction = false;
      }
    });
  }

  public confirmCancel(): void {
    if (!this.campaign) return;

    this.processingAction = true;
    this.campaignsAgent.cancelCampaign(this.campaign.id).pipe(
      catchError(() => {
        this.snackbarService.showError('Failed to cancel campaign');
        return of(null);
      })
    ).subscribe({
      next: () => {
        this.snackbarService.showSuccess('Campaign cancelled');
        this.showCancelDialog = false;
        this.loadCampaign(this.campaign!.id);
      },
      complete: () => {
        this.processingAction = false;
      }
    });
  }

  public goBack(): void {
    this.router.navigate(['/campaigns']);
  }

  public getDeliverableFormatClass(format: string): string {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    return this.templatesService.getFormatBadgeClass(format as any);
  }

  private loadCampaign(id: string): void {
    this.loading = true;
    this.error = null;

    this.campaignsAgent.getCampaign(id).pipe(
      catchError((error) => {
        this.error = error.message || 'Failed to load campaign';
        return of(null);
      })
    ).subscribe({
      next: (campaign) => {
        if (campaign) {
          this.campaign = campaign;
          this.detectUserRole();
        }
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  private detectUserRole(): void {
    if (!this.campaign) return;

    const currentUserId = this.authService.getUserIdFromToken();
    if (!currentUserId) return;

    // Creator is the offer owner (creatorId)
    this.isCreator = this.campaign.creatorId === currentUserId;
    // Client is the campaign proposer (clientId)
    this.isClient = this.campaign.clientId === currentUserId;
  }
}
