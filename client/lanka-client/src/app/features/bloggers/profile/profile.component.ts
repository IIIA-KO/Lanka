import { IBloggerProfile } from '../../../core/models/blogger';
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule, DatePipe } from '@angular/common';
import { Subject, forkJoin, takeUntil, catchError, of, finalize } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { AgentService } from '../../../core/api/agent';
import { ReviewsAgent } from '../../../core/api/reviews.agent';
import { IReview, IPact, IAveragePrice } from '../../../core/models/campaigns';
import {
  instagramClientId,
  instagramRenewRedirectUri,
  instagramScope,
  instagramResponseType,
  instagramConfigId,
} from '../../../core/constants/instagram.constants';
import { LocationType, StatisticsPeriod } from '../../../core/models/analytics/analytics.audience';
import {
  IEngagementStatisticsResponse,
  IInteractionStatisticsResponse,
  IOverviewStatisticsResponse
} from '../../../core/models/analytics/analytics.statistics';
import { AuthService } from '../../../core/services/auth/auth.service';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';
import { AnalyticsAgent } from '../../../core/api/analytics.agent';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TabsModule } from 'primeng/tabs';
import { DividerModule } from 'primeng/divider';
import { SelectButtonModule } from 'primeng/selectbutton';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { PactsAgent } from '../../../core/api/pacts.agent';
import { OffersAgent } from '../../../core/api/offers.agent';

import {
  expandGenderLabel,
  expandReachLabel,
  formatMetricName,
  GENDER_COLOR_MAPPING,
  REACH_COLOR_MAPPING,
  formatMetric as sharedFormatMetric,
  MetricStatus,
  evaluateEngagementRate,
  evaluateReachRate,
  evaluateErReach,
  evaluateCpe,
  getMetricStatusColor,
  getMetricStatusIcon,
  getMetricStatusLabel
} from '../../../core/utils/analytics.utils';
import { MarkdownService } from '../../../core/services/markdown.service';
import { DeliverableTemplatesService } from '../../../core/services/deliverable-templates.service';
import { IInstagramPost } from '../../../core/models/analytics/analytics.posts';
import { AnalyticsChartComponent } from '../../analytics/chart/chart.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ProfileCalendarComponent } from '../components/profile-calendar/profile-calendar.component';

@Component({
  imports: [
    RouterModule,
    CommonModule,
    FormsModule,
    DatePipe,
    AnalyticsChartComponent,
    ProfileCalendarComponent,
    ButtonModule,
    CardModule,
    TagModule,
    TabsModule,
    DividerModule,
    SelectButtonModule,
    ProgressSpinnerModule,
    TranslateModule,
    TooltipModule
  ],
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  standalone: true,
  styleUrl: './profile.component.css',
})
export class ProfileComponent implements OnInit, OnDestroy {
  public profile!: IBloggerProfile;
  public age: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public gender: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public locationCountry: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public locationCity: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public reach: { labels: string[]; values: number[] } = { labels: [], values: [] };

  // Derived top audience values (computed from analytics)
  public topCountry: string | null = null;
  public topCountryPct: number | null = null;
  public topAgeGroup: string | null = null;
  public topGender: string | null = null;
  public topGenderPct: number | null = null;
  public showDeleteConfirmation = false;
  public isDeletingAccount = false;
  public instagramAccountLinked = false;
  public pact: IPact | null = null;
  public loadingPact = false;
  public recentPosts: IInstagramPost[] = [];
  public loadingPosts = false;

  // Statistics
  public engagement: IEngagementStatisticsResponse | null = null;
  public interaction: IInteractionStatisticsResponse | null = null;
  public overview: IOverviewStatisticsResponse | null = null;
  public loadingStatistics = false;
  public loadingAnalytics = false;

  // Reviews
  public reviews: IReview[] = [];
  public loadingReviews = false;

  // Location toggle
  public locationMode = 'country';
  public locationModes: { label: string; value: string }[] = [];

  // Statistics period toggle
  public statsPeriod: StatisticsPeriod = StatisticsPeriod.week;
  public statsPeriodOptions: { label: string; value: StatisticsPeriod }[] = [];

  public readonly templatesService = inject(DeliverableTemplatesService);
  public averagePrices: IAveragePrice[] = [];

  public readonly genderColorMapping = GENDER_COLOR_MAPPING;
  public readonly reachColorMapping = REACH_COLOR_MAPPING;

  // Statistics sub-tab
  public statsSubTab = 'audience';
  public statsSubTabs: { label: string; value: string }[] = [];

  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(AgentService);
  private readonly analyticsApi = inject(AnalyticsAgent);
  private readonly pactsApi = inject(PactsAgent);
  private readonly offersApi = inject(OffersAgent);
  private readonly reviewsApi = inject(ReviewsAgent);
  private readonly auth = inject(AuthService);
  private readonly snackbar = inject(SnackbarService);
  private readonly markdownService = inject(MarkdownService);
  private readonly translate = inject(TranslateService);
  private readonly destroy$ = new Subject<void>();

  public get activeLocation(): { labels: string[]; values: number[] } {
    return this.locationMode === 'city' ? this.locationCity : this.locationCountry;
  }

  public get averageRating(): number {
    if (this.reviews.length === 0) return 0;
    const sum = this.reviews.reduce((acc, r) => acc + r.rating, 0);
    return Math.round((sum / this.reviews.length) * 10) / 10;
  }

  public activeTab = '0';

  private initTranslatedOptions(): void {
    this.locationModes = [
      { label: this.translate.instant('PROFILE.LOCATION.COUNTRIES'), value: 'country' },
      { label: this.translate.instant('PROFILE.LOCATION.CITIES'), value: 'city' }
    ];
    this.statsPeriodOptions = [
      { label: this.translate.instant('PROFILE.PERIOD.ONE_DAY'), value: StatisticsPeriod.day },
      { label: this.translate.instant('PROFILE.PERIOD.SEVEN_DAYS'), value: StatisticsPeriod.week },
      { label: this.translate.instant('PROFILE.PERIOD.TWENTY_ONE_DAYS'), value: StatisticsPeriod.day21 }
    ];
    this.statsSubTabs = [
      { label: this.translate.instant('PROFILE.TABS.AUDIENCE'), value: 'audience' },
      { label: this.translate.instant('PROFILE.TABS.ENGAGEMENT'), value: 'engagement' }
    ];
  }

  public ngOnInit(): void {
    this.initTranslatedOptions();
    this.translate.onLangChange.pipe(takeUntil(this.destroy$)).subscribe(() => this.initTranslatedOptions());

    this.profile = this.route.snapshot.data['profile'] ?? null;
    this.instagramAccountLinked = Boolean(this.profile?.instagramUsername);

    if (this.profile) {
      // Batch 1: lightweight data + statistics (5 requests)
      this.loadPact();
      this.loadReviews();
      this.loadStatistics(this.statsPeriod, () => {
        // Batch 2: after statistics complete, load distributions + posts (6 requests)
        this.loadAnalyticsAndDistributions();
        if (this.instagramAccountLinked) {
          this.loadRecentPosts();
        }
      });

      if (!this.instagramAccountLinked) {
        this.refreshInstagramLinkedState();
      }
    }
  }

  public onTabChange(tabValue: string | number): void {
    this.activeTab = String(tabValue);
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public getParsedPactContent(content: string): string {
    return this.markdownService.parse(content);
  }

  public async handleShareProfile(): Promise<void> {
    if (!this.profile?.id) {
      return;
    }

    const url = `${window.location.origin}/bloggers/${this.profile.id}`;
    const name = `${this.profile.firstName ?? ''} ${this.profile.lastName ?? ''}`.trim();
    const displayName = name || this.translate.instant('PROFILE.SHARE.DEFAULT_NAME');
    const shareTitle = this.translate.instant('PROFILE.SHARE.TITLE');
    const shareText = this.translate.instant('PROFILE.SHARE.MESSAGE', { name: displayName });

    if (navigator.share) {
      try {
        await navigator.share({ title: shareTitle, text: shareText, url });
        this.snackbar.showSuccess(this.translate.instant('PROFILE.SHARE.SHARED'));
        return;
      } catch {
        // Ignore user cancellation and fall through to clipboard
      }
    }

    if (navigator.clipboard?.writeText) {
      try {
        await navigator.clipboard.writeText(url);
        this.snackbar.showSuccess(this.translate.instant('PROFILE.SHARE.COPIED'));
        return;
      } catch {
        // Fall through to error snackbar
      }
    }

    this.snackbar.showError(this.translate.instant('PROFILE.SHARE.UNAVAILABLE'));
  }

  public handleRenewInstagramAccess(): void {
    const authUrl = `https://www.facebook.com/v20.0/dialog/oauth?client_id=${instagramClientId}&redirect_uri=${instagramRenewRedirectUri}&scope=${instagramScope}&response_type=${instagramResponseType}&config_id=${instagramConfigId}`;
    window.location.href = authUrl;
  }

  public toggleDeleteConfirmation(): void {
    this.showDeleteConfirmation = !this.showDeleteConfirmation;
  }

  public cancelAccountDeletion(): void {
    this.showDeleteConfirmation = false;
    this.isDeletingAccount = false;
  }

  public confirmAccountDeletion(): void {
    if (this.isDeletingAccount) {
      return;
    }

    this.isDeletingAccount = true;
    this.api.Users.deleteAccount()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackbar.showSuccess('PROFILE.DELETE_SUCCESS');
          this.auth.logout();
        },
        error: (error) => {
          console.error('[ProfileComponent] Failed to delete account:', error);
          this.snackbar.showError(
            error?.message || 'PROFILE.DELETE_ERROR'
          );
          this.isDeletingAccount = false;
          this.showDeleteConfirmation = false;
        },
      });
  }

  public isInstagramLinked(): boolean {
    return this.instagramAccountLinked;
  }

  public formatMetric(value?: number | null): string {
    return sharedFormatMetric(value);
  }

  public formatPercent(value?: number | null): string {
    if (value == null) return '—';
    return value.toFixed(2) + '%';
  }

  public getRatingArray(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < rating);
  }

  public canRenewAccess(): boolean {
    return this.isInstagramLinked();
  }

  public getInsightValue(post: IInstagramPost, insightName: string): number {
    const insight = post.insights?.find(i => i.name === insightName);
    return insight?.values?.[0]?.value || 0;
  }

  public formatNumber(num: number): string {
    return sharedFormatMetric(num);
  }

  public formatMetricNameLabel(name: string): string {
    return formatMetricName(name);
  }

  public getErStatus(er: number | null | undefined): MetricStatus { return evaluateEngagementRate(er); }
  public getRrStatus(rr: number | null | undefined): MetricStatus { return evaluateReachRate(rr); }
  public getErReachStatus(erReach: number | null | undefined): MetricStatus { return evaluateErReach(erReach); }
  public getCpeStatus(cpe: number | null | undefined): MetricStatus { return evaluateCpe(cpe); }
  
  public getMetricStatusColor(status: MetricStatus): string { return getMetricStatusColor(status); }
  public getMetricStatusIcon(status: MetricStatus): string { return getMetricStatusIcon(status); }
  public getMetricStatusLabel(status: MetricStatus): string { return getMetricStatusLabel(status); }

  public onPeriodChange(): void {
    this.loadStatistics(this.statsPeriod);
  }

  private loadAnalyticsAndDistributions(): void {
    this.loadingAnalytics = true;
    forkJoin({
      age: this.analyticsApi.getAgeDistribution().pipe(catchError(() => of(null))),
      gender: this.analyticsApi.getGenderDistribution().pipe(catchError(() => of(null))),
      locationCountry: this.analyticsApi.getLocationDistribution(LocationType.country).pipe(catchError(() => of(null))),
      locationCity: this.analyticsApi.getLocationDistribution(LocationType.city).pipe(catchError(() => of(null))),
      reach: this.analyticsApi.getReachDistribution(StatisticsPeriod.week).pipe(catchError(() => of(null)))
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          if (result.age?.agePercentages) {
            this.age = {
              labels: result.age.agePercentages.map(a => a.ageGroup),
              values: result.age.agePercentages.map(a => a.percentage)
            };
          }
          if (result.gender?.genderPercentages) {
            this.gender = {
              labels: result.gender.genderPercentages.map(g => expandGenderLabel(g.gender)),
              values: result.gender.genderPercentages.map(g => g.percentage)
            };
          }
          if (result.locationCountry?.locationPercentages) {
            const sortedCountry = [...result.locationCountry.locationPercentages].sort((a, b) => b.percentage - a.percentage);
            const top5Country = sortedCountry.slice(0, 5);
            this.locationCountry = {
              labels: top5Country.map(l => l.location),
              values: top5Country.map(l => l.percentage)
            };
            if (sortedCountry.length > 0) {
              this.topCountry = sortedCountry[0].location;
              this.topCountryPct = sortedCountry[0].percentage;
            }
          }
          if (result.locationCity?.locationPercentages) {
            const sortedCity = [...result.locationCity.locationPercentages].sort((a, b) => b.percentage - a.percentage);
            const top5City = sortedCity.slice(0, 5);
            this.locationCity = {
              labels: top5City.map(l => l.location),
              values: top5City.map(l => l.percentage)
            };
          }
          if (result.age?.agePercentages) {
            const sortedAge = [...result.age.agePercentages].sort((a, b) => b.percentage - a.percentage);
            this.topAgeGroup = sortedAge.length > 0 ? sortedAge[0].ageGroup : null;
          }
          if (result.gender?.genderPercentages) {
            const sortedGender = [...result.gender.genderPercentages].sort((a, b) => b.percentage - a.percentage);
            if (sortedGender.length > 0) {
              this.topGender = expandGenderLabel(sortedGender[0].gender);
              this.topGenderPct = sortedGender[0].percentage;
            }
          }
          if (result.reach?.reachPercentages) {
            this.reach = {
              labels: result.reach.reachPercentages.map(r => expandReachLabel(r.followType)),
              values: result.reach.reachPercentages.map(r => r.percentage)
            };
          }
          this.loadingAnalytics = false;
        }
      });
  }

  private loadStatistics(period: StatisticsPeriod, onComplete?: () => void): void {
    this.loadingStatistics = true;
    forkJoin({
      engagement: this.analyticsApi.getEngagementStatistics(period).pipe(catchError(() => of(null))),
      interaction: this.analyticsApi.getInteractionStatistics(period).pipe(catchError(() => of(null))),
      overview: this.analyticsApi.getOverviewStatistics(period).pipe(catchError(() => of(null)))
    })
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.loadingStatistics = false;
          onComplete?.();
        })
      )
      .subscribe({
        next: (result) => {
          this.engagement = result.engagement;
          this.interaction = result.interaction;
          this.overview = result.overview;
        }
      });
  }

  private loadReviews(): void {
    if (!this.profile?.id) return;
    this.loadingReviews = true;
    this.reviewsApi.getBloggerReviews(this.profile.id)
      .pipe(takeUntil(this.destroy$), catchError(() => of([] as IReview[])))
      .subscribe({
        next: (reviews) => {
          this.reviews = reviews;
          this.loadingReviews = false;
        }
      });
  }

  private refreshInstagramLinkedState(): void {
    if (!this.profile?.id) {
      this.instagramAccountLinked = false;
      return;
    }

    this.api.Users.getLinkInstagramStatus()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (status) => {
          this.instagramAccountLinked = status?.status === 'completed';
          if (this.instagramAccountLinked) {
            this.loadRecentPosts();
          }
        },
        error: (error: unknown) => {
          console.error('[ProfileComponent] Error checking Instagram link state:', error);
        },
      });
  }

  private loadPact(): void {
    if (!this.profile?.id) return;

    this.loadingPact = true;
    this.pactsApi.getPactByBloggerId(this.profile.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (pact) => {
          this.pact = pact;
          this.loadAveragePrices();
        },
        error: (error) => {
          console.error('[ProfileComponent] No pact found or error loading pact:', error);
        },
        complete: () => {
          this.loadingPact = false;
        }
      });
  }

  private loadAveragePrices(): void {
    if (!this.profile?.id) return;
    this.offersApi.getBloggerAverageOfferPrices(this.profile.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (prices) => {
          this.averagePrices = prices;
        },
        error: () => {
          this.averagePrices = [];
        }
      });
  }

  private loadRecentPosts(): void {
    if (!this.instagramAccountLinked) {
      return;
    }

    this.loadingPosts = true;
    this.analyticsApi.getPosts({ limit: 6 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.recentPosts = response?.posts || [];
        },
        error: (error: unknown) => {
          console.error('[ProfileComponent] Error loading recent posts:', error);
          this.recentPosts = [];
        },
        complete: () => {
          this.loadingPosts = false;
        }
      });
  }
}
