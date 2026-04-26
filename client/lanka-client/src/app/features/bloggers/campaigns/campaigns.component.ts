import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
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


@Component({
  standalone: true,
  selector: 'app-campaigns',
  imports: [
    DatePipe,
    NgClass,
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

  public get weekdays(): string[] {
    const days = this.translate.instant('PROFILE.CALENDAR.WEEKDAYS');
    return Array.isArray(days) ? days : ['MO', 'TU', 'WE', 'TH', 'FR', 'SA', 'SU'];
  }

  public get currentMonthLabel(): string {
    return this.getMonthName(this.baseDate);
  }

  public get currentMonth(): MonthRow {
    const year = this.baseDate.getFullYear();
    const month = this.baseDate.getMonth();
    const monthCampaigns = this.filteredCampaigns.filter(c => {
      const d = new Date(c.scheduledOnUtc);
      return d.getFullYear() === year && d.getMonth() === month;
    });
    return {
      monthDate: this.baseDate,
      monthName: this.getMonthName(this.baseDate),
      year,
      days: this.buildCalendarDays(year, month, monthCampaigns),
      campaigns: monthCampaigns
    };
  }

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

  // ── List view filters + pagination ────────────────────────────────────────
  public nameFilter = '';
  public statusFilter: string[] = [];
  public listPage = 1;
  public readonly listPageSize = 10;

  public get totalListPages(): number {
    return Math.max(1, Math.ceil(this.filteredListCampaigns.length / this.listPageSize));
  }

  public get listPageNumbers(): (number | null)[] {
    const total = this.totalListPages;
    const current = this.listPage;
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
    const shown = new Set<number>([1, total]);
    for (let p = Math.max(1, current - 1); p <= Math.min(total, current + 1); p++) shown.add(p);
    const sorted = Array.from(shown).sort((a, b) => a - b);
    const result: (number | null)[] = [];
    for (let i = 0; i < sorted.length; i++) {
      if (i > 0 && sorted[i] - sorted[i - 1] > 1) result.push(null);
      result.push(sorted[i]);
    }
    return result;
  }

  public get pagedListCampaigns(): ICampaign[] {
    const start = (this.listPage - 1) * this.listPageSize;
    return this.filteredListCampaigns.slice(start, start + this.listPageSize);
  }

  public goToListPage(page: number | null): void {
    if (page === null) return;
    this.listPage = Math.min(Math.max(1, page), this.totalListPages);
  }
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

  // ── Selected day (calendar) ────────────────────────────────────────────────
  public selectedDate: Date | null = null;

  public get selectedDayCampaigns(): ICampaign[] {
    if (!this.selectedDate) return [];
    return this.filteredCampaigns.filter(c =>
      new Date(c.scheduledOnUtc).toDateString() === this.selectedDate!.toDateString()
    );
  }

  public get upcomingCampaign(): ICampaign | null {
    const now = new Date();
    const sorted = this.filteredCampaigns
      .filter(c =>
        new Date(c.scheduledOnUtc) >= now &&
        c.status !== CampaignStatus.Cancelled &&
        c.status !== CampaignStatus.Rejected
      )
      .sort((a, b) => new Date(a.scheduledOnUtc).getTime() - new Date(b.scheduledOnUtc).getTime());
    return sorted[0] ?? null;
  }

  public get detailCampaigns(): ICampaign[] {
    if (this.selectedDate && this.selectedDayCampaigns.length > 0) {
      return this.selectedDayCampaigns;
    }
    const upcoming = this.upcomingCampaign;
    return upcoming ? [upcoming] : [];
  }

  public onDayClick(day: CalendarDay): void {
    if (!day.isCurrentMonth || day.campaigns.length === 0) return;
    const toggled = this.selectedDate?.toDateString() === day.date.toDateString()
      ? null : day.date;
    this.selectedDate = toggled;
    this.syncUrlParams({ d: toggled ? toggled.toISOString() : null });
  }

  public getDayStatusClass(day: CalendarDay): string {
    if (day.campaigns.length === 0) return day.isToday ? 'cal-mini-num--today-empty' : 'cal-mini-num--plain';
    const hasAction = day.campaigns.some(c => this.getAvailableActions(c).length > 0);
    if (hasAction) return 'cal-mini-num--pending';
    const status = day.campaigns[0].status.toLowerCase();
    return `cal-mini-num--${status}`;
  }

  public isSelectedDay(day: CalendarDay): boolean {
    return !!this.selectedDate && day.date.toDateString() === this.selectedDate.toDateString();
  }

  public getPartnerName(campaign: ICampaign): string {
    if (campaign.creatorId?.toLowerCase() === this.currentUserId) {
      return [campaign.clientFirstName, campaign.clientLastName].filter(Boolean).join(' ');
    }
    return [campaign.creatorFirstName, campaign.creatorLastName].filter(Boolean).join(' ');
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
  private readonly route = inject(ActivatedRoute);
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
        monthName: this.getMonthName(monthDate),
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
    const params = this.route.snapshot.queryParams;

    // Restore view mode
    if (params['view'] === 'list' || params['view'] === 'calendar') {
      this.viewMode = params['view'] as ViewMode;
    }

    // Restore list filters
    if (params['name']) this.nameFilter = params['name'];
    if (params['status']) this.statusFilter = params['status'].split(',');

    // Restore selected date (calendar mode)
    if (params['d']) {
      const parsed = new Date(params['d']);
      if (!isNaN(parsed.getTime())) {
        this.selectedDate = parsed;
        this.baseDate = new Date(parsed.getFullYear(), parsed.getMonth(), 1);
      }
    }

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
    this.selectedDate = null;
    this.syncUrlParams({ d: null });
    this.loadCampaigns();
  }

  public nextMonth(): void {
    this.baseDate = new Date(this.baseDate.getFullYear(), this.baseDate.getMonth() + 1, 1);
    this.selectedDate = null;
    this.syncUrlParams({ d: null });
    this.loadCampaigns();
  }

  public goToToday(): void {
    this.baseDate = new Date();
    this.selectedDate = null;
    this.syncUrlParams({ d: null });
    this.loadCampaigns();
  }

  // ── View mode change ───────────────────────────────────────────────────────
  public onViewModeChange(): void {
    this.nameFilter = '';
    this.statusFilter = [];
    this.listPage = 1;
    this.syncUrlParams({ view: this.viewMode, d: null, name: null, status: null });
    this.loadCampaigns();
  }

  private syncUrlParams(params: Record<string, string | null>): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: params,
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
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
      endDate = new Date(this.baseDate.getFullYear(), this.baseDate.getMonth() + 1, 0, 23, 59, 59).toISOString();
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

  public rowAction(event: Event, action: string, campaignId: string): void {
    event.stopPropagation();
    this.onCampaignAction(action, campaignId);
  }

  public onNameFilterChange(value: string): void {
    this.listPage = 1;
    this.syncUrlParams({ name: value || null });
  }

  public onStatusFilterChange(values: string[]): void {
    this.listPage = 1;
    this.syncUrlParams({ status: values.length ? values.join(',') : null });
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
