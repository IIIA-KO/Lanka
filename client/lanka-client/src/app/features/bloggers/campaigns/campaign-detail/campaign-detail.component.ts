import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject, takeUntil, catchError, of, forkJoin } from 'rxjs';

import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { RatingModule } from 'primeng/rating';
import { DialogModule } from 'primeng/dialog';
import { FormsModule } from '@angular/forms';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { CampaignsAgent } from '../../../../core/api/campaigns.agent';
import { BloggersAgent } from '../../../../core/api/bloggers.agent';
import { SnackbarService } from '../../../../core/services/snackbar/snackbar.service';
import { ICampaign, CampaignStatus } from '../../../../core/models/campaigns';

@Component({
  standalone: true,
  selector: 'app-campaign-detail',
  imports: [
    DatePipe,
    DecimalPipe,
    RouterModule,
    FormsModule,
    TranslateModule,
    ButtonModule,
    TagModule,
    DividerModule,
    ProgressSpinnerModule,
    MessageModule,
    ConfirmDialogModule,
    RatingModule,
    DialogModule,
    TooltipModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './campaign-detail.component.html',
  styleUrls: ['./campaign-detail.component.css'],
})
export class CampaignDetailComponent implements OnInit, OnDestroy {
  public campaign: ICampaign | null = null;
  public creatorProfile: import('../../../../core/models/blogger').IBloggerProfile | null = null;
  public clientProfile:  import('../../../../core/models/blogger').IBloggerProfile | null = null;
  public loading = true;
  public error: string | null = null;
  public isCreator = false;
  public isClient = false;
  public processingAction = false;

  // Review dialog
  public showReviewDialog = false;
  public reviewRating = 0;
  public reviewComment = '';
  public reviewSubmitted = false;

  public readonly CampaignStatus = CampaignStatus;

  private currentUserId: string | null = null;
  private readonly destroy$ = new Subject<void>();

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly campaignsAgent = inject(CampaignsAgent);
  private readonly bloggersAgent = inject(BloggersAgent);
  private readonly snackbarService = inject(SnackbarService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translate = inject(TranslateService);

  public ngOnInit(): void {
    const campaignId = this.route.snapshot.paramMap.get('id');
    if (!campaignId) {
      this.error = this.translate.instant('CAMPAIGNS.DETAIL.ERROR_NO_ID');
      this.loading = false;
      return;
    }

    forkJoin({
      profile:  this.bloggersAgent.getProfile().pipe(catchError(() => of(null))),
      campaign: this.campaignsAgent.getCampaign(campaignId).pipe(catchError(() => of(null))),
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe(({ profile, campaign }) => {
        if (!campaign) {
          this.error = this.translate.instant('CAMPAIGNS.DETAIL.ERROR_LOAD');
          this.loading = false;
          return;
        }
        this.campaign = campaign;
        this.currentUserId = profile?.id?.toLowerCase() ?? null;
        this.isCreator = !!this.currentUserId && campaign.creatorId?.toLowerCase() === this.currentUserId;
        this.isClient  = !!this.currentUserId && campaign.clientId?.toLowerCase()  === this.currentUserId;

        // Fetch participant profiles for photos
        forkJoin({
          creator: this.bloggersAgent.getBlogger(campaign.creatorId).pipe(catchError(() => of(null))),
          client:  this.bloggersAgent.getBlogger(campaign.clientId).pipe(catchError(() => of(null))),
        })
          .pipe(takeUntil(this.destroy$))
          .subscribe(({ creator, client }) => {
            this.creatorProfile = creator;
            this.clientProfile  = client;
            this.loading = false;
          });
      });
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Status helpers ────────────────────────────────────────────────────────
  public getStatusSeverity(status: string): string {
    switch (status) {
      case CampaignStatus.Pending:   return 'warning';
      case CampaignStatus.Confirmed: return 'success';
      case CampaignStatus.Done:      return 'info';
      case CampaignStatus.Completed: return 'success';
      case CampaignStatus.Rejected:  return 'danger';
      case CampaignStatus.Cancelled: return 'secondary';
      default: return 'secondary';
    }
  }

  public getStatusIcon(status: string): string {
    switch (status) {
      case CampaignStatus.Pending:   return 'pi pi-clock';
      case CampaignStatus.Confirmed: return 'pi pi-check';
      case CampaignStatus.Done:      return 'pi pi-flag';
      case CampaignStatus.Completed: return 'pi pi-check-circle';
      case CampaignStatus.Rejected:  return 'pi pi-times';
      case CampaignStatus.Cancelled: return 'pi pi-ban';
      default: return 'pi pi-circle';
    }
  }

  // ── Timeline ─────────────────────────────────────────────────────────────
  public get timelineEvents(): { label: string; date: string | undefined; icon: string; active: boolean; terminal?: boolean }[] {
    if (!this.campaign) { return []; }
    const c = this.campaign;
    const s = c.status as CampaignStatus;

    const reachedConfirmed = s === CampaignStatus.Confirmed || s === CampaignStatus.Done || s === CampaignStatus.Completed;
    const reachedDone      = s === CampaignStatus.Done || s === CampaignStatus.Completed;
    const reachedCompleted = s === CampaignStatus.Completed;

    const events = [
      { label: this.translate.instant('CAMPAIGNS.DETAIL.DATE_PROPOSED'),  date: c.pendedOnUtc,    icon: 'pi pi-send',         active: true },
      { label: this.translate.instant('CAMPAIGNS.DETAIL.DATE_CONFIRMED'),  date: c.confirmedOnUtc,  icon: 'pi pi-check',        active: reachedConfirmed },
      { label: this.translate.instant('CAMPAIGNS.DETAIL.DATE_SCHEDULED'),  date: c.scheduledOnUtc,  icon: 'pi pi-calendar',     active: reachedConfirmed },
      { label: this.translate.instant('CAMPAIGNS.DETAIL.DATE_DONE'),       date: c.doneOnUtc,       icon: 'pi pi-flag',         active: reachedDone },
      { label: this.translate.instant('CAMPAIGNS.DETAIL.DATE_COMPLETED'),  date: c.completedOnUtc,  icon: 'pi pi-check-circle', active: reachedCompleted, terminal: true },
    ];

    if (s === CampaignStatus.Rejected) {
      events.push({ label: this.translate.instant('CAMPAIGNS.DETAIL.DATE_REJECTED'), date: c.rejectedOnUtc, icon: 'pi pi-times', active: true, terminal: true });
    }
    if (s === CampaignStatus.Cancelled) {
      events.push({ label: this.translate.instant('CAMPAIGNS.DETAIL.DATE_CANCELLED'), date: c.cancelledOnUtc, icon: 'pi pi-ban', active: true, terminal: true });
    }

    return events;
  }

  // ── Step context ─────────────────────────────────────────────────────────
  public get stepContext(): { myTurn: boolean; icon: string; headlineKey: string; bodyKey: string } | null {
    if (!this.campaign) { return null; }
    const s = this.campaign.status as CampaignStatus;

    if (this.isCreator) {
      switch (s) {
        case CampaignStatus.Pending:
          return { myTurn: true,  icon: 'pi pi-inbox',        headlineKey: 'CAMPAIGNS.DETAIL.STEP_CREATOR_PENDING_HEADLINE',   bodyKey: 'CAMPAIGNS.DETAIL.STEP_CREATOR_PENDING_BODY' };
        case CampaignStatus.Confirmed:
          return { myTurn: true,  icon: 'pi pi-play-circle',  headlineKey: 'CAMPAIGNS.DETAIL.STEP_CREATOR_CONFIRMED_HEADLINE', bodyKey: 'CAMPAIGNS.DETAIL.STEP_CREATOR_CONFIRMED_BODY' };
        case CampaignStatus.Done:
          return { myTurn: false, icon: 'pi pi-hourglass',    headlineKey: 'CAMPAIGNS.DETAIL.STEP_CREATOR_DONE_HEADLINE',      bodyKey: 'CAMPAIGNS.DETAIL.STEP_CREATOR_DONE_BODY' };
        case CampaignStatus.Completed:
          return { myTurn: false, icon: 'pi pi-check-circle', headlineKey: 'CAMPAIGNS.DETAIL.STEP_COMPLETED_HEADLINE',         bodyKey: 'CAMPAIGNS.DETAIL.STEP_COMPLETED_BODY' };
        default:
          return { myTurn: false, icon: 'pi pi-ban',          headlineKey: 'CAMPAIGNS.DETAIL.STEP_TERMINAL_HEADLINE',          bodyKey: 'CAMPAIGNS.DETAIL.STEP_TERMINAL_BODY' };
      }
    }

    if (this.isClient) {
      switch (s) {
        case CampaignStatus.Pending:
          return { myTurn: false, icon: 'pi pi-hourglass',    headlineKey: 'CAMPAIGNS.DETAIL.STEP_CLIENT_PENDING_HEADLINE',   bodyKey: 'CAMPAIGNS.DETAIL.STEP_CLIENT_PENDING_BODY' };
        case CampaignStatus.Confirmed:
          return { myTurn: false, icon: 'pi pi-eye',          headlineKey: 'CAMPAIGNS.DETAIL.STEP_CLIENT_CONFIRMED_HEADLINE', bodyKey: 'CAMPAIGNS.DETAIL.STEP_CLIENT_CONFIRMED_BODY' };
        case CampaignStatus.Done:
          return { myTurn: true,  icon: 'pi pi-flag',         headlineKey: 'CAMPAIGNS.DETAIL.STEP_CLIENT_DONE_HEADLINE',      bodyKey: 'CAMPAIGNS.DETAIL.STEP_CLIENT_DONE_BODY' };
        case CampaignStatus.Completed:
          return { myTurn: false, icon: 'pi pi-check-circle', headlineKey: 'CAMPAIGNS.DETAIL.STEP_COMPLETED_HEADLINE',        bodyKey: 'CAMPAIGNS.DETAIL.STEP_COMPLETED_BODY' };
        default:
          return { myTurn: false, icon: 'pi pi-ban',          headlineKey: 'CAMPAIGNS.DETAIL.STEP_TERMINAL_HEADLINE',         bodyKey: 'CAMPAIGNS.DETAIL.STEP_TERMINAL_BODY' };
      }
    }

    return null;
  }

  // ── Actions ──────────────────────────────────────────────────────────────
  public canConfirm():   boolean { return this.isCreator && this.campaign?.status === CampaignStatus.Pending; }
  public canReject():    boolean { return this.isCreator && this.campaign?.status === CampaignStatus.Pending; }
  public canMarkDone():  boolean { return this.isCreator && this.campaign?.status === CampaignStatus.Confirmed; }
  public canCancel():    boolean { return (this.isCreator || this.isClient) && this.campaign?.status === CampaignStatus.Confirmed; }
  public canComplete():  boolean { return this.isClient  && this.campaign?.status === CampaignStatus.Done; }
  public canReview():    boolean { return this.isClient  && this.campaign?.status === CampaignStatus.Completed && !this.reviewSubmitted; }

  public onConfirm(): void {
    this.confirmationService.confirm({
      message: this.translate.instant('CAMPAIGNS.DETAIL.CONFIRM_DIALOG_CONFIRM_MSG'),
      header: this.translate.instant('CAMPAIGNS.DETAIL.CONFIRM_DIALOG_CONFIRM_HEADER'),
      icon: 'pi pi-check-circle',
      acceptButtonStyleClass: 'p-button-success',
      accept: () => this.execute(() => this.campaignsAgent.confirmCampaign(this.campaign!.id), 'CAMPAIGNS.DETAIL.SUCCESS_CONFIRMED'),
    });
  }

  public onReject(): void {
    this.confirmationService.confirm({
      message: this.translate.instant('CAMPAIGNS.DETAIL.CONFIRM_DIALOG_REJECT_MSG'),
      header: this.translate.instant('CAMPAIGNS.DETAIL.CONFIRM_DIALOG_REJECT_HEADER'),
      icon: 'pi pi-times-circle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.execute(() => this.campaignsAgent.rejectCampaign(this.campaign!.id), 'CAMPAIGNS.DETAIL.SUCCESS_REJECTED'),
    });
  }

  public onMarkDone(): void {
    this.execute(() => this.campaignsAgent.markCampaignAsDone(this.campaign!.id), 'CAMPAIGNS.DETAIL.SUCCESS_DONE');
  }

  public onCancel(): void {
    this.confirmationService.confirm({
      message: this.translate.instant('CAMPAIGNS.DETAIL.CONFIRM_DIALOG_CANCEL_MSG'),
      header: this.translate.instant('CAMPAIGNS.DETAIL.CONFIRM_DIALOG_CANCEL_HEADER'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.execute(() => this.campaignsAgent.cancelCampaign(this.campaign!.id), 'CAMPAIGNS.DETAIL.SUCCESS_CANCELLED'),
    });
  }

  public onComplete(): void {
    this.execute(() => this.campaignsAgent.completeCampaign(this.campaign!.id), 'CAMPAIGNS.DETAIL.SUCCESS_COMPLETED');
  }

  public submitReview(): void {
    if (!this.campaign || !this.reviewRating) { return; }
    this.processingAction = true;
    this.campaignsAgent.createReview({
      campaignId: this.campaign.id,
      rating: this.reviewRating,
      comment: this.reviewComment,
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.reviewSubmitted = true;
          this.showReviewDialog = false;
          this.processingAction = false;
          this.snackbarService.showSuccess(this.translate.instant('CAMPAIGNS.DETAIL.REVIEW_SUBMITTED'));
        },
        error: () => {
          this.processingAction = false;
          this.snackbarService.showError(this.translate.instant('CAMPAIGNS.REVIEW.ERROR'));
        },
      });
  }

  public participantRoute(id: string): string[] {
    return id?.toLowerCase() === this.currentUserId ? ['/profile'] : ['/bloggers', id];
  }

  public goBack(): void {
    this.router.navigate(['/campaigns']);
  }

  private execute(action: () => import('rxjs').Observable<unknown>, successMsgKey: string): void {
    if (!this.campaign) { return; }
    this.processingAction = true;
    const id = this.campaign.id;
    action()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.processingAction = false;
          this.snackbarService.showSuccess(this.translate.instant(successMsgKey));
          this.reload(id);
        },
        error: () => {
          this.processingAction = false;
          this.snackbarService.showError(this.translate.instant('CAMPAIGNS.DETAIL.ERROR_ACTION'));
        },
      });
  }

  private reload(id: string): void {
    this.loading = true;
    this.campaignsAgent.getCampaign(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: c => { this.campaign = c; this.loading = false; },
        error: () => { this.loading = false; },
      });
  }
}
