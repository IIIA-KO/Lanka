import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { SelectButtonModule } from 'primeng/selectbutton';
import { MessageModule } from 'primeng/message';
import { TooltipModule } from 'primeng/tooltip';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { RatingModule } from 'primeng/rating';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { FormsModule } from '@angular/forms';
import { ConfirmationService } from 'primeng/api';

import { CampaignsAgent } from '../../../core/api/campaigns.agent';
import { BloggersAgent } from '../../../core/api/bloggers.agent';
import { ICampaign, CampaignStatus } from '../../../core/models/campaigns';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';

export type RoleFilter = 'all' | 'creator' | 'client';
export type ViewMode = 'calendar' | 'list';

export interface CalendarDay {
  date: Date;
  isCurrentMonth: boolean;
  isToday: boolean;
  hasCampaign: boolean;
  campaigns: ICampaign[];
}

export interface MonthRow {
  monthDate: Date;
  monthName: string;
  year: number;
  days: CalendarDay[];
  campaigns: ICampaign[];
}

const WEEKDAYS = ['ПН', 'ВТ', 'СР', 'ЧТ', 'ПТ', 'СБ', 'НД'];
const MONTHS_UK = [
  'СІЧЕНЬ', 'ЛЮТИЙ', 'БЕРЕЗЕНЬ', 'КВІТЕНЬ', 'ТРАВЕНЬ', 'ЧЕРВЕНЬ',
  'ЛИПЕНЬ', 'СЕРПЕНЬ', 'ВЕРЕСЕНЬ', 'ЖОВТЕНЬ', 'ЛИСТОПАД', 'ГРУДЕНЬ'
];

@Component({
  standalone: true,
  selector: 'app-campaigns',
  imports: [
    DatePipe,
    FormsModule,
    TranslateModule,
    ButtonModule,
    CardModule,
    TagModule,
    SelectButtonModule,
    MessageModule,
    TooltipModule,
    TableModule,
    DialogModule,
    ConfirmDialogModule,
    RatingModule,
    InputTextModule,
    MultiSelectModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './campaigns.component.html',
  styleUrls: ['./campaigns.component.css']
})
export class CampaignsComponent implements OnInit, OnDestroy {
  public allCampaigns: ICampaign[] = [];
  public loading = false;
  public error: string | null = null;

  public baseDate = new Date();
  public weekdays = WEEKDAYS;

  // ── View mode ──────────────────────────────────────────────────────────────
  public viewMode: ViewMode = 'calendar';
  public get viewOptions() {
    return [
      { label: this.translate.instant('CAMPAIGNS.VIEW.CALENDAR'), value: 'calendar', icon: 'pi pi-calendar' },
      { label: this.translate.instant('CAMPAIGNS.VIEW.LIST'), value: 'list', icon: 'pi pi-list' },
    ];
  }

  // ── Role filter ────────────────────────────────────────────────────────────
  public roleFilter: RoleFilter = 'all';
  public get roleOptions() {
    return [
      { label: this.translate.instant('CAMPAIGNS.ROLE.ALL'), value: 'all' },
      { label: this.translate.instant('CAMPAIGNS.ROLE.CREATOR'), value: 'creator' },
      { label: this.translate.instant('CAMPAIGNS.ROLE.CLIENT'), value: 'client' },
    ];
  }

  // ── List view filters ──────────────────────────────────────────────────────
  public nameFilter = '';
  public statusFilter: string[] = [];
  public get statusOptions() {
    return [
      { label: this.translate.instant('CAMPAIGNS.STATUS.PENDING'),   value: CampaignStatus.Pending },
      { label: this.translate.instant('CAMPAIGNS.STATUS.CONFIRMED'), value: CampaignStatus.Confirmed },
      { label: this.translate.instant('CAMPAIGNS.STATUS.DONE'),      value: CampaignStatus.Done },
      { label: this.translate.instant('CAMPAIGNS.STATUS.COMPLETED'), value: CampaignStatus.Completed },
      { label: this.translate.instant('CAMPAIGNS.STATUS.CANCELLED'), value: CampaignStatus.Cancelled },
      { label: this.translate.instant('CAMPAIGNS.STATUS.REJECTED'),  value: CampaignStatus.Rejected },
    ];
  }

  // ── Review dialog ──────────────────────────────────────────────────────────
  public showReviewDialog = false;
  public reviewCampaignId: string | null = null;
  public reviewRating = 0;
  public reviewComment = '';
  public readonly reviewedCampaignIds = new Set<string>();

  public currentUserId: string | null = null;

  private readonly destroy$ = new Subject<void>();
  private readonly campaignsAgent = inject(CampaignsAgent);
  private readonly bloggersAgent = inject(BloggersAgent);
  private readonly router = inject(Router);
  private readonly snackbarService = inject(SnackbarService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translate = inject(TranslateService);

  // ── Campaigns requiring user action ───────────────────────────────────────
  public get actionRequiredCampaigns(): { campaign: ICampaign; actions: { label: string; value: string; icon: string }[] }[] {
    return this.allCampaigns
      .map(c => ({ campaign: c, actions: this.getAvailableActions(c) }))
      .filter(x => x.actions.length > 0);
  }

  // ── Filtered campaigns (by role) ──────────────────────────────────────────
  public get filteredCampaigns(): ICampaign[] {
    if (this.roleFilter === 'all' || !this.currentUserId) {
      return this.allCampaigns;
    }
    return this.allCampaigns.filter(c =>
      this.roleFilter === 'creator'
        ? c.creatorId?.toLowerCase() === this.currentUserId
        : c.clientId?.toLowerCase() === this.currentUserId
    );
  }

  // Kept for action guards (legacy)
  public get campaigns(): ICampaign[] {
    return this.filteredCampaigns;
  }

  // ── List view: filtered + searched ─────────────────────────────────────────
  public get filteredListCampaigns(): ICampaign[] {
    let campaigns = this.filteredCampaigns;

    if (this.nameFilter.trim()) {
      const query = this.nameFilter.toLowerCase();
      campaigns = campaigns.filter(c => c.name.toLowerCase().includes(query));
    }

    if (this.statusFilter.length > 0) {
      campaigns = campaigns.filter(c => this.statusFilter.includes(c.status));
    }

    return campaigns;
  }

  // ── Month rows (calendar + campaigns per month) ────────────────────────────
  public get monthRows(): MonthRow[] {
    const rows: MonthRow[] = [];
    const filtered = this.filteredCampaigns;

    for (let i = 0; i < 3; i++) {
      const monthDate = new Date(this.baseDate.getFullYear(), this.baseDate.getMonth() + i, 1);
      const year = monthDate.getFullYear();
      const month = monthDate.getMonth();

      const monthCampaigns = filtered.filter(c => {
        const d = new Date(c.scheduledOnUtc);
        return d.getFullYear() === year && d.getMonth() === month;
      });

      rows.push({
        monthDate,
        monthName: `${MONTHS_UK[month]} ${year}`,
        year,
        days: this.buildCalendarDays(year, month, monthCampaigns),
        campaigns: monthCampaigns
      });
    }
    return rows;
  }

  public getMonthName(monthDate: Date): string {
    return monthDate.toLocaleDateString(this.translate.currentLang || 'uk', { month: 'long', year: 'numeric' }).toUpperCase();
  }

  public getMonthCampaignCount(count: number): string {
    const key = count === 1 ? 'CAMPAIGNS.CALENDAR.CAMPAIGN_COUNT_ONE' : 'CAMPAIGNS.CALENDAR.CAMPAIGN_COUNT_OTHER';
    return this.translate.instant(key, { count });
  }

  public getStatusChipClass(status: string): string {
    switch (status) {
      case CampaignStatus.Pending:   return 'chip-pending';
      case CampaignStatus.Confirmed: return 'chip-confirmed';
      case CampaignStatus.Done:      return 'chip-done';
      case CampaignStatus.Completed: return 'chip-completed';
      case CampaignStatus.Cancelled: return 'chip-cancelled';
      case CampaignStatus.Rejected:  return 'chip-rejected';
      default: return 'chip-pending';
    }
  }

  private buildCalendarDays(year: number, month: number, campaigns: ICampaign[]): CalendarDay[] {
    const days: CalendarDay[] = [];
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const today = new Date();

    let firstDayOffset = new Date(year, month, 1).getDay() - 1;
    if (firstDayOffset === -1) { firstDayOffset = 6; }

    for (let i = 0; i < firstDayOffset; i++) {
      const prevDate = new Date(year, month, -firstDayOffset + i + 1);
      days.push({ date: prevDate, isCurrentMonth: false, isToday: false, hasCampaign: false, campaigns: [] });
    }

    for (let d = 1; d <= daysInMonth; d++) {
      const date = new Date(year, month, d);
      const dayCampaigns = campaigns.filter(
        c => new Date(c.scheduledOnUtc).toDateString() === date.toDateString()
      );
      days.push({
        date,
        isCurrentMonth: true,
        isToday: date.toDateString() === today.toDateString(),
        hasCampaign: dayCampaigns.length > 0,
        campaigns: dayCampaigns
      });
    }

    // Pad to complete final row (multiple of 7)
    while (days.length % 7 !== 0) {
      const nextDate = new Date(year, month + 1, days.length - daysInMonth - firstDayOffset + 1);
      days.push({ date: nextDate, isCurrentMonth: false, isToday: false, hasCampaign: false, campaigns: [] });
    }

    return days;
  }

  // ── Lifecycle ──────────────────────────────────────────────────────────────
  public ngOnInit(): void {
    this.loading = true;
    this.bloggersAgent.getProfile()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: profile => {
          this.currentUserId = profile.id.toLowerCase();
          this.loadCampaigns();
        },
        error: () => {
          this.error = this.translate.instant('CAMPAIGNS.ERROR_USER');
          this.loading = false;
        }
      });
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Navigation ─────────────────────────────────────────────────────────────
  public prevMonth(): void {
    this.baseDate = new Date(this.baseDate.getFullYear(), this.baseDate.getMonth() - 1, 1);
    this.loadCampaigns();
  }

  public nextMonth(): void {
    this.baseDate = new Date(this.baseDate.getFullYear(), this.baseDate.getMonth() + 1, 1);
    this.loadCampaigns();
  }

  // ── View mode change ───────────────────────────────────────────────────────
  public onViewModeChange(): void {
    this.nameFilter = '';
    this.statusFilter = [];
    this.loadCampaigns();
  }

  // ── Data Loading ───────────────────────────────────────────────────────────
  public loadCampaigns(): void {
    if (!this.currentUserId) {
      this.error = this.translate.instant('CAMPAIGNS.ERROR_USER');
      return;
    }

    this.loading = true;
    this.error = null;

    let startDate: string | undefined;
    let endDate: string | undefined;

    if (this.viewMode === 'calendar') {
      startDate = new Date(this.baseDate.getFullYear(), this.baseDate.getMonth(), 1).toISOString();
      endDate = new Date(this.baseDate.getFullYear(), this.baseDate.getMonth() + 3, 0, 23, 59, 59).toISOString();
    }

    this.campaignsAgent.getBloggerCampaigns(this.currentUserId, startDate, endDate)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: campaigns => {
          this.allCampaigns = campaigns;
          this.loading = false;
        },
        error: () => {
          this.error = this.translate.instant('CAMPAIGNS.ERROR');
          this.loading = false;
        }
      });
  }

  // ── Status helpers ─────────────────────────────────────────────────────────
  public getStatusColor(status: string): string {
    switch (status) {
      case CampaignStatus.Pending:   return 'warning';
      case CampaignStatus.Confirmed: return 'success';
      case CampaignStatus.Rejected:  return 'danger';
      case CampaignStatus.Done:      return 'info';
      case CampaignStatus.Completed: return 'success';
      case CampaignStatus.Cancelled: return 'secondary';
      default: return 'secondary';
    }
  }

  public getStatusIcon(status: string): string {
    switch (status) {
      case CampaignStatus.Pending:   return 'pi pi-clock';
      case CampaignStatus.Confirmed: return 'pi pi-check';
      case CampaignStatus.Rejected:  return 'pi pi-times';
      case CampaignStatus.Done:      return 'pi pi-flag';
      case CampaignStatus.Completed: return 'pi pi-check-circle';
      case CampaignStatus.Cancelled: return 'pi pi-ban';
      default: return 'pi pi-circle';
    }
  }

  // ── Campaign Actions ───────────────────────────────────────────────────────
  public onCampaignAction(action: string, campaignId: string): void {
    if (!action) { return; }

    if (action === 'review') {
      this.openReviewDialog(campaignId);
      return;
    }

    if (action === 'cancel' || action === 'reject') {
      const isCancel = action === 'cancel';
      this.confirmationService.confirm({
        message: this.translate.instant(isCancel ? 'CAMPAIGNS.CONFIRM_CANCEL' : 'CAMPAIGNS.CONFIRM_REJECT'),
        header: this.translate.instant(isCancel ? 'CAMPAIGNS.CONFIRM_CANCEL_HEADER' : 'CAMPAIGNS.CONFIRM_REJECT_HEADER'),
        icon: 'pi pi-exclamation-triangle',
        acceptButtonStyleClass: 'p-button-danger',
        accept: () => this.executeAction(action, campaignId)
      });
      return;
    }

    this.executeAction(action, campaignId);
  }

  private executeAction(action: string, campaignId: string): void {
    this.loading = true;

    const actionMap: Record<string, () => import('rxjs').Observable<unknown>> = {
      confirm: () => this.campaignsAgent.confirmCampaign(campaignId),
      reject:  () => this.campaignsAgent.rejectCampaign(campaignId),
      done:    () => this.campaignsAgent.markCampaignAsDone(campaignId),
      complete: () => this.campaignsAgent.completeCampaign(campaignId),
      cancel:  () => this.campaignsAgent.cancelCampaign(campaignId),
      pend:    () => this.campaignsAgent.pendCampaign(campaignId),
    };

    const factory = actionMap[action];
    if (!factory) {
      this.loading = false;
      return;
    }

    factory().pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.loading = false;
        this.snackbarService.showSuccess(this.translate.instant(`CAMPAIGNS.DETAIL.SUCCESS_${action.toUpperCase()}`));
        this.loadCampaigns();
      },
      error: (err: Error) => {
        this.loading = false;
        this.snackbarService.showError(this.translate.instant('CAMPAIGNS.DETAIL.ERROR_ACTION') + ': ' + err.message);
      }
    });
  }

  public viewCampaign(campaignId: string): void {
    this.router.navigate(['/campaigns', campaignId]);
  }

  public getAvailableActions(campaign: ICampaign): { label: string; value: string; icon: string }[] {
    const actions: { label: string; value: string; icon: string }[] = [];
    const isCreator = campaign.creatorId?.toLowerCase() === this.currentUserId;
    const isClient  = campaign.clientId?.toLowerCase()  === this.currentUserId;

    switch (campaign.status) {
      case CampaignStatus.Pending:
        if (isCreator) {
          actions.push(
            { label: this.translate.instant('CAMPAIGNS.ACTIONS.CONFIRM'), value: 'confirm', icon: 'pi pi-check' },
            { label: this.translate.instant('CAMPAIGNS.ACTIONS.REJECT'),  value: 'reject',  icon: 'pi pi-times' }
          );
        }
        break;
      case CampaignStatus.Confirmed:
        if (isCreator) {
          actions.push(
            { label: this.translate.instant('CAMPAIGNS.ACTIONS.MARK_DONE'), value: 'done',   icon: 'pi pi-flag' },
            { label: this.translate.instant('CAMPAIGNS.ACTIONS.CANCEL'),    value: 'cancel', icon: 'pi pi-ban' }
          );
        }
        break;
      case CampaignStatus.Done:
        if (isClient) {
          actions.push({ label: this.translate.instant('CAMPAIGNS.ACTIONS.COMPLETE'), value: 'complete', icon: 'pi pi-check-circle' });
        }
        break;
      case CampaignStatus.Completed:
        if (isClient && !this.reviewedCampaignIds.has(campaign.id)) {
          actions.push({ label: this.translate.instant('CAMPAIGNS.ACTIONS.LEAVE_REVIEW'), value: 'review', icon: 'pi pi-star' });
        }
        break;
    }
    return actions;
  }

  public getRoleBadge(campaign: ICampaign): string {
    if (campaign.creatorId?.toLowerCase() === this.currentUserId) { return this.translate.instant('CAMPAIGNS.ROLE_CREATOR'); }
    if (campaign.clientId?.toLowerCase()  === this.currentUserId) { return this.translate.instant('CAMPAIGNS.ROLE_CLIENT'); }
    return '';
  }

  // ── Review dialog ──────────────────────────────────────────────────────────
  public openReviewDialog(campaignId: string): void {
    this.reviewCampaignId = campaignId;
    this.reviewRating = 0;
    this.reviewComment = '';
    this.showReviewDialog = true;
  }

  public cancelReview(): void {
    this.showReviewDialog = false;
    this.reviewCampaignId = null;
  }

  public submitReview(): void {
    if (!this.reviewCampaignId || !this.reviewRating) {
      this.snackbarService.showError(this.translate.instant('CAMPAIGNS.REVIEW.RATING_REQUIRED'));
      return;
    }

    this.campaignsAgent.createReview({
      campaignId: this.reviewCampaignId,
      rating: this.reviewRating,
      comment: this.reviewComment,
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.reviewedCampaignIds.add(this.reviewCampaignId!);
        this.showReviewDialog = false;
        this.snackbarService.showSuccess(this.translate.instant('CAMPAIGNS.REVIEW.SUCCESS'));
      },
      error: () => {
        this.snackbarService.showError(this.translate.instant('CAMPAIGNS.REVIEW.ERROR'));
      }
    });
  }
}
