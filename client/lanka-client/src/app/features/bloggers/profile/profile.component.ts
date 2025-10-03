import { IBloggerProfile } from '../../../core/models/blogger';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AgentService } from '../../../core/api/agent';
import { instagramClientId, instagramRenewRedirectUri, instagramScope, instagramResponseType, instagramConfigId } from '../../../core/constants/instagram.constants';
import { AnalyticsChartComponent } from '../../analytics/chart/chart.component';
import { LocationType } from '../../../core/models/analytics/analytics.audience';

@Component({
  imports: [RouterModule, CommonModule, AnalyticsChartComponent],
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
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(AgentService);

  constructor() {
    // Empty constructor
  }

  public ngOnInit(): void {
    this.profile = this.route.snapshot.data['profile'] ?? null;

    if (this.profile) {
      this.loadAnalytics();
    }
  }

  public handleRenewInstagramAccess(): void {
    const authUrl = `https://www.facebook.com/v20.0/dialog/oauth?client_id=${instagramClientId}&redirect_uri=${instagramRenewRedirectUri}&scope=${instagramScope}&response_type=${instagramResponseType}&config_id=${instagramConfigId}`;
    window.location.href = authUrl;
  }

  private loadAnalytics(): void {
    this.api.Analytics.getAgeDistribution().subscribe({
      next: (data) => {
        this.age.labels = data.agePercentages.map((p) => p.ageGroup);
        this.age.values = data.agePercentages.map((p) => p.percentage);
      },
      error: (error) => console.error('[ProfileComponent] Error loading age distribution:', error),
    });

    this.api.Analytics.getGenderDistribution().subscribe({
      next: (data) => {
        this.gender.labels = data.genderPercentages.map((p) => p.gender);
        this.gender.values = data.genderPercentages.map((p) => p.percentage);
      },
      error: (error) =>
        console.error('[ProfileComponent] Error loading gender distribution:', error),
    });

    this.api.Analytics.getLocationDistribution(LocationType.country).subscribe({
      next: (data) => {
        this.location.labels = data.locationPercentages.map((p) => p.location);
        this.location.values = data.locationPercentages.map(
          (p) => p.percentage
        );
      },
      error: (error) =>
        console.error('[ProfileComponent] Error loading location distribution:', error),
    });
  }
}
