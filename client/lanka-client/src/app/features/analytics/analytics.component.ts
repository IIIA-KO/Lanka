import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { catchError, of, forkJoin } from 'rxjs';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TabsModule } from 'primeng/tabs';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';

import { AnalyticsAgent } from '../../core/api/analytics.agent';
import { StatisticsPeriod } from '../../core/models/analytics/analytics.audience';
import { 
  IPostsResponse, 
  IInstagramPost 
} from '../../core/models/analytics/analytics.posts';
import { 
  IOverviewStatisticsResponse,
  IEngagementStatisticsResponse,
  IInteractionStatisticsResponse,
  IMetricsStatisticsResponse
} from '../../core/models/analytics/analytics.statistics';
import { SnackbarService } from '../../core/services/snackbar/snackbar.service';

@Component({
  standalone: true,
  selector: 'app-analytics',
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    CardModule,
    TabsModule,
    ProgressSpinnerModule,
    MessageModule,
    SelectModule,
    TagModule
  ],
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.css']
})
export class AnalyticsComponent implements OnInit {
  public loading = false;
  public error: string | null = null;
  
  // Period selection
  public selectedPeriod: StatisticsPeriod = StatisticsPeriod.week;
  public periodOptions = [
    { label: 'Daily', value: StatisticsPeriod.day },
    { label: 'Weekly', value: StatisticsPeriod.week },
    { label: '21 Days', value: StatisticsPeriod.day21 }
  ];

  // Data
  public posts: IInstagramPost[] = [];
  public overviewStats: IOverviewStatisticsResponse | null = null;
  public engagementStats: IEngagementStatisticsResponse | null = null;
  public interactionStats: IInteractionStatisticsResponse | null = null;
  public tableStats: IMetricsStatisticsResponse | null = null;

  // No chart dependencies needed

  private readonly analyticsAgent = inject(AnalyticsAgent);
  private readonly snackbarService = inject(SnackbarService);

  public ngOnInit(): void {
    this.loadAnalyticsData();
  }

  public onPeriodChange(): void {
    this.loadAnalyticsData();
  }

  public getPostInsightValue(post: IInstagramPost, insightName: string): number {
    const insight = post.insights?.find(i => i.name === insightName);
    return insight?.values?.[0]?.value || 0;
  }

  public formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    } else {
      return num.toString();
    }
  }

  public refreshData(): void {
    this.loadAnalyticsData();
  }

  private loadAnalyticsData(): void {
    this.loading = true;
    this.error = null;

    // Load all analytics data in parallel
    forkJoin({
      posts: this.analyticsAgent.getPosts({ limit: 20 }).pipe(
        catchError(error => {
          console.error('[AnalyticsComponent] Error loading posts:', error);
          return of({ posts: [], paging: { cursors: {} } } as IPostsResponse);
        })
      ),
      overview: this.analyticsAgent.getOverviewStatistics(this.selectedPeriod).pipe(
        catchError(error => {
          console.error('[AnalyticsComponent] Error loading overview stats:', error);
          return of(null);
        })
      ),
      engagement: this.analyticsAgent.getEngagementStatistics(this.selectedPeriod).pipe(
        catchError(error => {
          console.error('[AnalyticsComponent] Error loading engagement stats:', error);
          return of(null);
        })
      ),
      interaction: this.analyticsAgent.getInteractionStatistics(this.selectedPeriod).pipe(
        catchError(error => {
          console.error('[AnalyticsComponent] Error loading interaction stats:', error);
          return of(null);
        })
      ),
      table: this.analyticsAgent.getTableStatistics(this.selectedPeriod).pipe(
        catchError(error => {
          console.error('[AnalyticsComponent] Error loading table stats:', error);
          return of(null);
        })
      )
    }).subscribe({
      next: (results) => {
        this.posts = results.posts.posts;
        this.overviewStats = results.overview;
        this.engagementStats = results.engagement;
        this.interactionStats = results.interaction;
        this.tableStats = results.table;
      },
      error: (error) => {
        this.error = 'Failed to load analytics data';
        this.snackbarService.showError('Error loading analytics: ' + error.message);
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
} 