import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { DatePipe, SlicePipe, TitleCasePipe, DecimalPipe, CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Subject, forkJoin, takeUntil, catchError, of, finalize } from 'rxjs';
import { BloggersAgent } from '../../../core/api/bloggers.agent';
import { PactsAgent } from '../../../core/api/pacts.agent';
import { ReviewsAgent } from '../../../core/api/reviews.agent';
import { IPact, IReview, IOffer, IAveragePrice } from '../../../core/models/campaigns';
import { OffersAgent } from '../../../core/api/offers.agent';
import { MarkdownService } from '../../../core/services/markdown.service';

import {
  expandGenderLabel, expandReachLabel, formatMetricName,
  GENDER_COLOR_MAPPING, REACH_COLOR_MAPPING,
  formatMetric,
  MetricStatus,
  evaluateEngagementRate,
  evaluateReachRate,
  evaluateErReach,
  evaluateCpe,
  getMetricStatusColor,
  getMetricStatusIcon,
  getMetricStatusLabel
} from '../../../core/utils/analytics.utils';
import { AnalyticsAgent } from '../../../core/api/analytics.agent';
import { IBloggerProfile } from '../../../core/models/blogger';
import { LocationType, StatisticsPeriod } from '../../../core/models/analytics/analytics.audience';
import { IEngagementStatisticsResponse, IInteractionStatisticsResponse, IOverviewStatisticsResponse } from '../../../core/models/analytics/analytics.statistics';
import { IInstagramPost } from '../../../core/models/analytics/analytics.posts';
import { ButtonModule } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { SelectButtonModule } from 'primeng/selectbutton';
import { FormsModule } from '@angular/forms';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AccordionModule } from 'primeng/accordion';
import { AnalyticsChartComponent } from '../../analytics/chart/chart.component';
import { ProposeCampaignComponent } from './propose-campaign/propose-campaign.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';
import { ProfileCalendarComponent } from '../components/profile-calendar/profile-calendar.component';

@Component({
  selector: 'app-public-profile',
  standalone: true,
  imports: [
    DatePipe,
    SlicePipe,
    TitleCasePipe,
    DecimalPipe,
    CommonModule,
    RouterModule,
    FormsModule,
    ButtonModule,
    TabsModule,
    SelectButtonModule,
    ProgressSpinnerModule,
    AccordionModule,
    AnalyticsChartComponent,
    ProposeCampaignComponent,
    TranslateModule,
    TooltipModule,
    TagModule,
    ProfileCalendarComponent
  ],
  templateUrl: './public-profile.component.html',
  styleUrl: './public-profile.component.css'
})
export class PublicProfileComponent implements OnInit, OnDestroy {
  public blogger: IBloggerProfile | null = null;
  public pact: IPact | null = null;
  public offers: IOffer[] = [];
  public reviews: IReview[] = [];
  public loading = true;
  public loadingOffers = false;
  public loadingReviews = false;
  public recentPosts: IInstagramPost[] = [];
  public loadingPosts = false;
  public error: string | null = null;

  // Propose Campaign Dialog State
  public showProposeDialog = false;
  public selectedOfferId: string | null = null;
  public selectedOfferName: string | null = null;

  // Tabs state
  public activeTab: string = '0';

  // Statistics sub-tab
  public statsSubTab = 'audience';
  public statsSubTabs = [
    { label: 'Audience', value: 'audience' },
    { label: 'Engagement', value: 'engagement' }
  ];

  // Location mode toggle
  public locationMode = 'country';
  public locationModes = [
    { label: 'Countries', value: 'country' },
    { label: 'Cities', value: 'city' }
  ];

  // Statistics period toggle
  public statsPeriod: StatisticsPeriod = StatisticsPeriod.week;
  public statsPeriodOptions = [
    { label: '1d', value: StatisticsPeriod.day },
    { label: '7d', value: StatisticsPeriod.week },
    { label: '21d', value: StatisticsPeriod.day21 }
  ];

  public readonly genderColorMapping = GENDER_COLOR_MAPPING;
  public readonly reachColorMapping = REACH_COLOR_MAPPING;

  // Analytics Data — Audience Distributions
  public age: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public gender: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public locationCountry: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public locationCity: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public reach: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public loadingAnalytics = false;

  // Analytics Data — Statistics
  public engagement: IEngagementStatisticsResponse | null = null;
  public interaction: IInteractionStatisticsResponse | null = null;
  public overview: IOverviewStatisticsResponse | null = null;
  public loadingStatistics = false;

  // Derived top audience values (computed from analytics)
  public topCountry: string | null = null;
  public topCountryPct: number | null = null;
  public topAgeGroup: string | null = null;
  public topGender: string | null = null;
  public topGenderPct: number | null = null;

  public averagePrices: IAveragePrice[] = [];

  private readonly route = inject(ActivatedRoute);
  private readonly bloggersApi = inject(BloggersAgent);
  private readonly pactsApi = inject(PactsAgent);
  private readonly reviewsApi = inject(ReviewsAgent);
  private readonly offersApi = inject(OffersAgent);
  private readonly analyticsApi = inject(AnalyticsAgent);
  private readonly markdownService = inject(MarkdownService);
  private readonly translate = inject(TranslateService);
  private readonly snackbar = inject(SnackbarService);
  private readonly destroy$ = new Subject<void>();

  public get averageRating(): number {
    if (this.reviews.length === 0) return 0;
    const sum = this.reviews.reduce((acc, r) => acc + r.rating, 0);
    return Math.round((sum / this.reviews.length) * 10) / 10;
  }

  public async handleShareProfile(): Promise<void> {
    if (!this.blogger?.id) {
      return;
    }

    const url = `${window.location.origin}/bloggers/${this.blogger.id}`;
    const name = `${this.blogger.firstName ?? ''} ${this.blogger.lastName ?? ''}`.trim();
    const displayName = name || this.translate.instant('PROFILE.SHARE.DEFAULT_NAME');
    const shareTitle = this.translate.instant('PROFILE.SHARE.TITLE');
    const shareText = this.translate.instant('PROFILE.SHARE.MESSAGE', { name: displayName });

    if (navigator.share) {
      try {
        await navigator.share({ title: shareTitle, text: shareText, url });
        this.snackbar.showSuccess(this.translate.instant('PROFILE.SHARE.SHARED'));
        return;
      } catch (e: any) {
        // Ignore user cancellation and fall through to clipboard
      }
    }

    if (navigator.clipboard?.writeText) {
      try {
        await navigator.clipboard.writeText(url);
        this.snackbar.showSuccess(this.translate.instant('PROFILE.SHARE.COPIED'));
        return;
      } catch (e: any) {
        // Fall through to error
      }
    }

    this.snackbar.showError(this.translate.instant('PROFILE.SHARE.UNAVAILABLE'));
  }

  public handleWorkWithMe(): void {
    // Switch to Campaign tab (value "3")
    this.activeTab = '3';

    // Scroll to the content container
    setTimeout(() => {
      const element = document.getElementById('profile-content');
      if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }, 100);
  }

  public get location(): { labels: string[]; values: number[] } {
    return this.locationMode === 'city' ? this.locationCity : this.locationCountry;
  }

  public ngOnInit(): void {
    const bloggerId = this.route.snapshot.paramMap.get('id');
    if (bloggerId) {
      this.loadBlogger(bloggerId);
    } else {
      this.error = 'Blogger ID not found';
      this.loading = false;
    }
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public formatMetric(value?: number | null): string {
    return formatMetric(value);
  }

  public formatPercent(value?: number | null): string {
    if (value == null) return '—';
    return value.toFixed(2) + '%';
  }

  public formatPrice(amount: number): string {
    return amount.toFixed(0);
  }

  public getPactContent(): string {
    if (!this.pact?.content) return '';
    return this.markdownService.parse(this.pact.content);
  }

  public getFormatIcon(format?: string): string {
    const map: Record<string, string> = {
      'Instagram Post': 'pi pi-image',
      'Instagram Reel': 'pi pi-video',
      'Instagram Story': 'pi pi-clock',
      'Instagram Carousel': 'pi pi-images',
      'TikTok Video': 'pi pi-play',
      'YouTube Video': 'pi pi-youtube',
      'YouTube Short': 'pi pi-play-circle',
      'Blog Post': 'pi pi-file',
      'Custom': 'pi pi-pencil'
    };
    return map[format ?? ''] ?? 'pi pi-briefcase';
  }

  public getRatingArray(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < rating);
  }

  public formatMetricNameLabel(name: string): string {
    return formatMetricName(name);
  }

  public onPeriodChange(): void {
    if (this.blogger?.id) {
      this.loadStatistics(this.blogger.id, this.statsPeriod);
    }
  }

  public getInsightValue(post: IInstagramPost, insightName: string): number {
    const insight = post.insights?.find(i => i.name === insightName);
    return insight?.values?.[0]?.value || 0;
  }

  public openProposeDialog(offerId?: string, offerName?: string): void {
    this.selectedOfferId = offerId || null;
    this.selectedOfferName = offerName || null;
    this.showProposeDialog = true;
  }

  public getErStatus(er: number | null | undefined): MetricStatus { return evaluateEngagementRate(er); }
  public getRrStatus(rr: number | null | undefined): MetricStatus { return evaluateReachRate(rr); }
  public getErReachStatus(erReach: number | null | undefined): MetricStatus { return evaluateErReach(erReach); }
  public getCpeStatus(cpe: number | null | undefined): MetricStatus { return evaluateCpe(cpe); }
  
  public getMetricStatusColor(status: MetricStatus): string { return getMetricStatusColor(status); }
  public getMetricStatusIcon(status: MetricStatus): string { return getMetricStatusIcon(status); }
  public getMetricStatusLabel(status: MetricStatus): string { return getMetricStatusLabel(status); }

  private loadBlogger(id: string): void {
    this.loading = true;
    this.bloggersApi.getBlogger(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.blogger = data;
          this.loading = false;
          this.loadRelatedData(id);
        },
        error: () => {
          this.error = 'Failed to load blogger profile';
          this.loading = false;
        }
      });
  }

  private loadRelatedData(bloggerId: string): void {
    // Phase 1: Lightweight data (pact, offers, reviews) — these hit Campaigns module, not Instagram
    this.loadingOffers = true;
    this.pactsApi.getPactByBloggerId(bloggerId)
      .pipe(takeUntil(this.destroy$), catchError(() => of(null)))
      .subscribe({
        next: (pact) => {
          this.pact = pact;
          this.offers = pact?.offers ?? [];
          this.loadingOffers = false;
        }
      });

    this.offersApi.getBloggerAverageOfferPrices(bloggerId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (prices) => {
          this.averagePrices = prices;
        },
        error: () => {
          this.averagePrices = [];
        }
      });

    this.loadingReviews = true;
    this.reviewsApi.getBloggerReviews(bloggerId)
      .pipe(takeUntil(this.destroy$), catchError(() => of([])))
      .subscribe({
        next: (reviews) => {
          this.reviews = reviews;
          this.loadingReviews = false;
        }
      });

    // Phase 2: Statistics first, then distributions + posts after completion
    this.loadStatistics(bloggerId, this.statsPeriod, () => {
      this.loadAnalyticsAndPosts(bloggerId);
    });
  }

  private loadAnalyticsAndPosts(bloggerId: string): void {
    this.loadingPosts = true;
    this.analyticsApi.getBloggerPosts(bloggerId, 6)
      .pipe(takeUntil(this.destroy$), catchError(() => of(null)))
      .subscribe({
        next: (response) => {
          this.recentPosts = response?.posts || [];
          this.loadingPosts = false;
        }
      });

    this.loadingAnalytics = true;
    forkJoin({
      age: this.analyticsApi.getBloggerAgeDistribution(bloggerId).pipe(catchError(() => of(null))),
      gender: this.analyticsApi.getBloggerGenderDistribution(bloggerId).pipe(catchError(() => of(null))),
      locationCountry: this.analyticsApi.getBloggerLocationDistribution(bloggerId, LocationType.country).pipe(catchError(() => of(null))),
      locationCity: this.analyticsApi.getBloggerLocationDistribution(bloggerId, LocationType.city).pipe(catchError(() => of(null))),
      reach: this.analyticsApi.getBloggerReachDistribution(bloggerId, StatisticsPeriod.week).pipe(catchError(() => of(null)))
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

  private loadStatistics(bloggerId: string, period: StatisticsPeriod, onComplete?: () => void): void {
    this.loadingStatistics = true;
    forkJoin({
      engagement: this.analyticsApi.getBloggerEngagementStatistics(bloggerId, period).pipe(catchError(() => of(null))),
      interaction: this.analyticsApi.getBloggerInteractionStatistics(bloggerId, period).pipe(catchError(() => of(null))),
      overview: this.analyticsApi.getBloggerOverviewStatistics(bloggerId, period).pipe(catchError(() => of(null)))
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
}
