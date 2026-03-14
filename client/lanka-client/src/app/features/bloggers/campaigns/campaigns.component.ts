import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { Observable, Subject, catchError, of, takeUntil } from 'rxjs';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { SelectButtonModule } from 'primeng/selectbutton';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { TooltipModule } from 'primeng/tooltip';
import { FormsModule } from '@angular/forms';

import { CampaignsAgent } from '../../../core/api/campaigns.agent';
import { AuthService } from '../../../core/services/auth/auth.service';
import { ICampaign, CampaignStatus } from '../../../core/models/campaigns';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';

export type RoleFilter = 'all' | 'creator' | 'client';

@Component({
  standalone: true,
  selector: 'app-campaigns',
  imports: [
    DatePipe,
    FormsModule,
    ButtonModule,
    CardModule,
    TagModule,
    SelectButtonModule,
    ProgressSpinnerModule,
    MessageModule,
    TooltipModule
  ],
  templateUrl: './campaigns.component.html',
  styleUrls: ['./campaigns.component.css']
})
export class CampaignsComponent implements OnInit, OnDestroy {
  public allCampaigns: ICampaign[] = [];
  public loading = false;
  public error: string | null = null;

  private readonly destroy$ = new Subject<void>();

  public roleFilter: RoleFilter = 'all';
  public roleOptions = [
    { label: 'All', value: 'all' },
    { label: 'As Creator', value: 'creator' },
    { label: 'As Client', value: 'client' }
  ];

  private currentUserId: string | null = null;

  private readonly campaignsAgent = inject(CampaignsAgent);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackbarService = inject(SnackbarService);

  public get campaigns(): ICampaign[] {
    if (this.roleFilter === 'all' || !this.currentUserId) {
      return this.allCampaigns;
    }
    return this.allCampaigns.filter(c =>
      this.roleFilter === 'creator'
        ? c.creatorId?.toLowerCase() === this.currentUserId
        : c.clientId?.toLowerCase() === this.currentUserId
    );
  }

  public ngOnInit(): void {
    this.currentUserId = this.authService.getUserIdFromToken()?.toLowerCase() ?? null;
    this.loadCampaigns();
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public loadCampaigns(): void {
    if (!this.currentUserId) {
      this.error = 'Unable to identify current user.';
      return;
    }

    this.loading = true;
    this.error = null;

    this.campaignsAgent.getBloggerCampaigns(this.currentUserId)
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => {
          this.error = 'We could not load your campaigns right now. Please try again shortly.';
          return of([] as ICampaign[]);
        })
      )
      .subscribe({
        next: (campaigns) => {
          this.allCampaigns = campaigns;
          this.loading = false;
        }
      });
  }

  public getStatusColor(status: string): string {
    switch (status) {
      case CampaignStatus.Pending: return 'warning';
      case CampaignStatus.Confirmed: return 'success';
      case CampaignStatus.Rejected: return 'danger';
      case CampaignStatus.Done: return 'info';
      case CampaignStatus.Completed: return 'success';
      case CampaignStatus.Cancelled: return 'secondary';
      default: return 'secondary';
    }
  }

  public getStatusIcon(status: string): string {
    switch (status) {
      case CampaignStatus.Pending: return 'pi pi-clock';
      case CampaignStatus.Confirmed: return 'pi pi-check';
      case CampaignStatus.Rejected: return 'pi pi-times';
      case CampaignStatus.Done: return 'pi pi-flag';
      case CampaignStatus.Completed: return 'pi pi-check-circle';
      case CampaignStatus.Cancelled: return 'pi pi-ban';
      default: return 'pi pi-circle';
    }
  }

  public onCampaignAction(action: string, campaignId: string): void {
    if (!action) {
      return;
    }

    this.loading = true;
    let actionObservable: Observable<unknown>;

    switch (action) {
      case 'confirm':
        actionObservable = this.campaignsAgent.confirmCampaign(campaignId);
        break;
      case 'reject':
        actionObservable = this.campaignsAgent.rejectCampaign(campaignId);
        break;
      case 'done':
        actionObservable = this.campaignsAgent.markCampaignAsDone(campaignId);
        break;
      case 'complete':
        actionObservable = this.campaignsAgent.completeCampaign(campaignId);
        break;
      case 'cancel':
        actionObservable = this.campaignsAgent.cancelCampaign(campaignId);
        break;
      case 'pend':
        actionObservable = this.campaignsAgent.pendCampaign(campaignId);
        break;
      default:
        this.loading = false;
        return;
    }

    actionObservable.pipe(
      takeUntil(this.destroy$),
      catchError(error => {
        this.snackbarService.showError('Error updating campaign: ' + error.message);
        return of(null);
      })
    ).subscribe({
      next: () => {
        this.snackbarService.showSuccess(`Campaign ${action}ed successfully`);
        this.loadCampaigns();
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  public viewCampaign(campaignId: string): void {
    this.router.navigate(['/campaigns', campaignId]);
  }

  public getAvailableActions(campaign: ICampaign): { label: string; value: string; icon: string }[] {
    const actions: { label: string; value: string; icon: string }[] = [];
    const isCreator = campaign.creatorId?.toLowerCase() === this.currentUserId;
    const isClient = campaign.clientId?.toLowerCase() === this.currentUserId;

    switch (campaign.status) {
      case CampaignStatus.Pending:
        if (isCreator) {
          actions.push(
            { label: 'Confirm', value: 'confirm', icon: 'pi pi-check' },
            { label: 'Reject', value: 'reject', icon: 'pi pi-times' }
          );
        }
        break;
      case CampaignStatus.Confirmed:
        if (isCreator) {
          actions.push(
            { label: 'Mark as Done', value: 'done', icon: 'pi pi-flag' },
            { label: 'Cancel', value: 'cancel', icon: 'pi pi-ban' }
          );
        }
        break;
      case CampaignStatus.Done:
        if (isClient) {
          actions.push(
            { label: 'Complete', value: 'complete', icon: 'pi pi-check-circle' }
          );
        }
        break;
    }

    return actions;
  }

  public getRoleBadge(campaign: ICampaign): string {
    if (campaign.creatorId?.toLowerCase() === this.currentUserId) return 'Creator';
    if (campaign.clientId?.toLowerCase() === this.currentUserId) return 'Client';
    return '';
  }
}
