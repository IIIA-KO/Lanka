import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Observable, catchError, of } from 'rxjs';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { TooltipModule } from 'primeng/tooltip';

import { CampaignsAgent } from '../../../core/api/campaigns.agent';
import { ICampaign, CampaignStatus } from '../../../core/models/campaigns';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';

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
  
  // Mock data for demonstration - in real app this would come from API
  public mockCampaigns: ICampaign[] = [
    {
      id: '1',
      name: 'Summer Collection Campaign',
      description: 'Promote our new summer fashion collection',
      status: CampaignStatus.Pending,
      offerId: 'offer-1',
      clientId: 'client-1', 
      creatorId: 'creator-1',
      scheduledOnUtc: new Date().toISOString()
    },
    {
      id: '2',
      name: 'Tech Product Review',
      description: 'Review our latest smartphone model',
      status: CampaignStatus.Confirmed,
      offerId: 'offer-2',
      clientId: 'client-2',
      creatorId: 'creator-2', 
      scheduledOnUtc: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString()
    }
  ];

  private readonly campaignsAgent = inject(CampaignsAgent);
  private readonly router = inject(Router);
  private readonly snackbarService = inject(SnackbarService);

  public ngOnInit(): void {
    this.loadCampaigns();
  }

  public loadCampaigns(): void {
    // Using mock data for now since there's no "get all campaigns" endpoint
    this.campaigns = this.mockCampaigns;
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

  public onCampaignAction(event: { value?: string }, campaignId: string): void {
    const action = event.value || event;
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
    // For now, just show campaign details in the console
    // In a real app, this would navigate to a campaign details page
    const campaign = this.campaigns.find(c => c.id === campaignId);
    if (campaign) {
      this.snackbarService.showSuccess(`Viewing campaign: ${campaign.name}`);
    }
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
}
