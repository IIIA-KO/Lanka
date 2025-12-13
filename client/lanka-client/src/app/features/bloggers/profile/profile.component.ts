import { IBloggerProfile } from '../../../core/models/blogger';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AgentService } from '../../../core/api/agent';
import {
  instagramClientId,
  instagramRenewRedirectUri,
  instagramScope,
  instagramResponseType,
  instagramConfigId,
} from '../../../core/constants/instagram.constants';
import { LocationType } from '../../../core/models/analytics/analytics.audience';
import { AuthService } from '../../../core/services/auth/auth.service';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { PactsAgent } from '../../../core/api/pacts.agent';
import { IPact } from '../../../core/models/campaigns';
import { MarkdownService } from '../../../core/services/markdown.service';
import { DeliverableTemplatesService } from '../../../core/services/deliverable-templates.service';
import { IInstagramPost } from '../../../core/models/analytics/analytics.posts';
import { AnalyticsChartComponent } from '../../analytics/chart/chart.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  imports: [
    RouterModule,
    CommonModule,
    AnalyticsChartComponent,
    ButtonModule,
    CardModule,
    TagModule,
    DividerModule,
    TranslateModule
  ],
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  standalone: true,
  styleUrl: './profile.component.css',
})
export class ProfileComponent implements OnInit {
  public profile!: IBloggerProfile;
  public age: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public gender: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public location: { labels: string[]; values: number[] } = {
    labels: [],
    values: [],
  };
  public showDeleteConfirmation = false;
  public isDeletingAccount = false;
  public instagramAccountLinked = false;
  public pact: IPact | null = null;
  public loadingPact = false;
  public recentPosts: IInstagramPost[] = [];
  public loadingPosts = false;

  public readonly templatesService = inject(DeliverableTemplatesService);

  public priceRange = '—';
  public engagementRate = '—'; // Mock for now

  public readonly genderColorMapping: Record<string, string> = {
    'Female': '#ea5284ff', // Pink/Red
    'Male': '#2196F3',   // Blue
    'Unidentified': '#9E9E9E', // Grey
    'Unknown': '#9E9E9E',
    'F': '#ea5284ff',
    'M': '#2196F3',
    'U': '#9E9E9E'
  };

  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(AgentService);
  private readonly pactsApi = inject(PactsAgent);
  private readonly auth = inject(AuthService);
  private readonly snackbar = inject(SnackbarService);
  private readonly markdownService = inject(MarkdownService);
  private readonly translate = inject(TranslateService);

  public ngOnInit(): void {
    this.profile = this.route.snapshot.data['profile'] ?? null;
    this.instagramAccountLinked = Boolean(this.profile?.instagramUsername);
    this.refreshInstagramLinkedState();

    if (this.profile) {
      this.loadAnalytics();
      this.loadPact();
      this.loadRecentPosts();
    }
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
    this.api.Users.deleteAccount().subscribe({
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
    if (value == null) {
      return '—';
    }

    if (value >= 1_000_000) {
      return `${(value / 1_000_000).toFixed(1)}M`;
    }

    if (value >= 1_000) {
      return `${(value / 1_000).toFixed(1)}K`;
    }

    return value.toLocaleString();
  }

  public canRenewAccess(): boolean {
    return this.isInstagramLinked();
  }

  public getInsightValue(post: IInstagramPost, insightName: string): number {
    const insight = post.insights?.find(i => i.name === insightName);
    return insight?.values?.[0]?.value || 0;
  }

  public formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  private loadAnalytics(): void {
    this.api.Analytics.getAgeDistribution().subscribe({
      next: (data: { agePercentages: { ageGroup: string; percentage: number }[] }) => {
        this.age.labels = data.agePercentages.map((p) => p.ageGroup);
        this.age.values = data.agePercentages.map((p) => p.percentage);
      },
      error: (error: unknown) => console.error('[ProfileComponent] Error loading age distribution:', error),
    });

    this.api.Analytics.getGenderDistribution().subscribe({
      next: (data: { genderPercentages: { gender: string; percentage: number }[] }) => {
        this.gender.labels = data.genderPercentages.map((p) => p.gender);
        this.gender.values = data.genderPercentages.map((p) => p.percentage);
      },
      error: (error: unknown) =>
        console.error('[ProfileComponent] Error loading gender distribution:', error),
    });

    this.api.Analytics.getLocationDistribution(LocationType.country).subscribe({
      next: (data: { locationPercentages: { location: string; percentage: number }[] }) => {
        this.location.labels = data.locationPercentages.map((p) => p.location);
        this.location.values = data.locationPercentages.map(
          (p) => p.percentage
        );
      },
      error: (error: unknown) =>
        console.error('[ProfileComponent] Error loading location distribution:', error),
    });
  }

  private refreshInstagramLinkedState(): void {
    if (!this.profile?.id) {
      this.instagramAccountLinked = false;
      return;
    }

    this.api.Users.getLinkInstagramStatus().subscribe({
      next: (status) => {
        this.instagramAccountLinked = status?.status === 'completed';
      },
      error: (error: unknown) => {
        console.error('[ProfileComponent] Error checking Instagram link state:', error);
        this.instagramAccountLinked = false;
      },
    });
  }
  private loadPact(): void {
    if (!this.profile?.id) return;

    this.loadingPact = true;
    this.pactsApi.getPactByBloggerId(this.profile.id).subscribe({
      next: (pact) => {
        this.pact = pact;
        this.calculatePriceRange();
      },
      error: (error) => {
        console.error('[ProfileComponent] No pact found or error loading pact:', error);
      },
      complete: () => {
        this.loadingPact = false;
      }
    });
  }

  private calculatePriceRange(): void {
    if (!this.pact?.offers || this.pact.offers.length === 0) {
      this.priceRange = '—';
      return;
    }

    const prices = this.pact.offers.map(o => o.priceAmount);
    const min = Math.min(...prices);
    const max = Math.max(...prices);
    const currency = this.pact.offers[0].priceCurrency || 'USD';

    if (min === max) {
      this.priceRange = `${currency} ${min}`;
    } else {
      this.priceRange = `${currency} ${min} - ${max}`;
    }
  }

  private loadRecentPosts(): void {
    if (!this.instagramAccountLinked) {
      return;
    }

    this.loadingPosts = true;
    this.api.Analytics.getPosts({ limit: 6 }).subscribe({
      next: (response) => {
        this.recentPosts = response.posts || [];
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
