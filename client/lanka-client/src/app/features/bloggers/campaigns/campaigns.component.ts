import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Observable, catchError, map, of } from 'rxjs';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { TooltipModule } from 'primeng/tooltip';

import { CampaignsAgent } from '../../../core/api/campaigns.agent';
import { SearchAgent, ISearchResult } from '../../../core/api/search.agent';
import { AuthService } from '../../../core/services/auth/auth.service';
import { ICampaign, CampaignStatus } from '../../../core/models/campaigns';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';
import { FriendlyErrorService, FriendlyHttpError } from '../../../core/services/friendly-error.service';

@Component({
  standalone: true,
  selector: 'app-campaigns',
  imports: [
    CommonModule,
    ButtonModule,
    CardModule,
    TagModule,
    SelectModule,
    ProgressSpinnerModule,
    MessageModule,
    TooltipModule
  ],
  templateUrl: './campaigns.component.html',
  styleUrls: ['./campaigns.component.css']
})
export class CampaignsComponent implements OnInit {
  public campaigns: ICampaign[] = [];
  public loading = false;
  public error: string | null = null;
  public emptyStateMessage = 'Your campaigns from brands will appear here.';

  private readonly campaignsAgent = inject(CampaignsAgent);
  private readonly searchAgent = inject(SearchAgent);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackbarService = inject(SnackbarService);
  private readonly friendlyErrorService = inject(FriendlyErrorService);

  public ngOnInit(): void {
    this.loadCampaigns();
  }

  public loadCampaigns(): void {
    this.loading = true;
    this.error = null;

    const currentUserId = this.authService.getUserIdFromToken()?.toLowerCase() ?? null;

    this.searchAgent
      .searchDocuments({
        q: '*',
        size: 50,
        itemTypes: SearchableItemType.Campaign,
        onlyActive: true,
      })
      .pipe(
        map((response) =>
          response.results
            .map((result) => this.mapSearchResultToCampaign(result))
            .filter((campaign): campaign is ICampaign => campaign !== null)
            .filter((campaign) =>
              !currentUserId || campaign.creatorId?.toLowerCase() === currentUserId
            )
            .sort(
              (a, b) =>
                new Date(b.scheduledOnUtc).getTime() - new Date(a.scheduledOnUtc).getTime()
            )
        ),
        catchError((error: unknown) => {
          const friendlyError =
            error instanceof FriendlyHttpError
              ? error
              : this.friendlyErrorService.toFriendlyError(error, {
                  fallbackMessage: 'We could not load your campaigns right now. Please try again shortly.',
                });

          if (friendlyError.status === 404 || friendlyError.status === 204) {
            this.error = null;
            this.emptyStateMessage = 'You have no active campaigns yet. They will appear here once brands add you to a campaign.';
            return of([] as ICampaign[]);
          }

          this.error = friendlyError.message;
          return of([] as ICampaign[]);
        })
      )
      .subscribe({
        next: (campaigns) => {
          this.campaigns = campaigns;
        },
        complete: () => {
          this.loading = false;
        },
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

  public onCampaignAction(actionOrEvent: string | { value?: string }, campaignId: string): void {
    const action = typeof actionOrEvent === 'string' ? actionOrEvent : actionOrEvent.value;

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

  public getAvailableActions(status: string): {label: string, value: string, icon: string}[] {
    const actions = [];

    switch (status) {
      case CampaignStatus.Pending:
        actions.push(
          {label: 'Confirm', value: 'confirm', icon: 'pi pi-check'},
          {label: 'Reject', value: 'reject', icon: 'pi pi-times'}
        );
        break;
      case CampaignStatus.Confirmed:
        actions.push(
          {label: 'Mark as Done', value: 'done', icon: 'pi pi-flag'},
          {label: 'Cancel', value: 'cancel', icon: 'pi pi-ban'}
        );
        break;
      case CampaignStatus.Done:
        actions.push(
          {label: 'Complete', value: 'complete', icon: 'pi pi-check-circle'}
        );
        break;
    }

    return actions;
  }

  private mapSearchResultToCampaign(result: ISearchResult): ICampaign | null {
    const metadata = result.metadata ?? {};

    const status = this.normalizeStatus(metadata['status']);
    const scheduledOnUtc = this.extractIsoString(metadata['scheduledOnUtc']);

    if (!scheduledOnUtc) {
      return null;
    }

    const offerId = this.extractGuidString(metadata['offerId']);
    const clientId = this.extractGuidString(metadata['clientId']);
    const creatorId = this.extractGuidString(metadata['creatorId']);

    const price = metadata['price'] ? JSON.parse(String(metadata['price'])) : { amount: 0, currency: 'USD' };
    const expectedCompletionDate = this.extractIsoString(metadata['expectedCompletionDate']) ?? new Date().toISOString();
    const deliverables = metadata['deliverables'] ? JSON.parse(String(metadata['deliverables'])) : [];
    const createdAt = this.extractIsoString(metadata['createdAt']) ?? new Date().toISOString();
    const updatedAt = this.extractIsoString(metadata['updatedAt']) ?? new Date().toISOString();

    return {
      id: result.id,
      name: result.title ?? 'Untitled Campaign',
      description: result.content ?? '',
      status,
      offerId,
      clientId,
      creatorId,
      price,
      expectedCompletionDate,
      deliverables,
      scheduledOnUtc,
      createdAt,
      updatedAt
    };
  }

  private normalizeStatus(statusValue: unknown): CampaignStatus {
    const status = String(statusValue ?? '').toLowerCase();
    switch (status) {
      case 'pending':
        return CampaignStatus.Pending;
      case 'confirmed':
        return CampaignStatus.Confirmed;
      case 'rejected':
        return CampaignStatus.Rejected;
      case 'done':
        return CampaignStatus.Done;
      case 'completed':
        return CampaignStatus.Completed;
      case 'cancelled':
        return CampaignStatus.Cancelled;
      default:
        return CampaignStatus.Pending;
    }
  }

  private extractIsoString(value: unknown): string | null {
    if (!value) {
      return null;
    }

    if (typeof value === 'string') {
      return value;
    }

    if (value instanceof Date) {
      return value.toISOString();
    }

    return null;
  }

  private extractGuidString(value: unknown): string {
    return value ? String(value) : '';
  }
}

const SearchableItemType = {
  Campaign: '2',
} as const;
