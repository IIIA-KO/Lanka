import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { BloggersAgent } from '../../../core/api/bloggers.agent';
import { OffersAgent } from '../../../core/api/offers.agent';
import { IBloggerProfile } from '../../../core/models/blogger';
import { IOffer, DeliverableFormat } from '../../../core/models/campaigns';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TabsModule } from 'primeng/tabs';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AnalyticsChartComponent } from '../../analytics/chart/chart.component';
import { AgentService } from '../../../core/api/agent';
import { ProposeCampaignComponent } from './propose-campaign/propose-campaign.component';

@Component({
  selector: 'app-public-profile',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ButtonModule,
    CardModule,
    TagModule,
    TabsModule,
    ProgressSpinnerModule,
    AnalyticsChartComponent,
    ProposeCampaignComponent
  ],
  templateUrl: './public-profile.component.html',
  styleUrl: './public-profile.component.css'
})
export class PublicProfileComponent implements OnInit {
  public blogger: IBloggerProfile | null = null;
  public offers: IOffer[] = [];
  public loading = true;
  public loadingOffers = false;
  public error: string | null = null;

  // Propose Campaign Dialog State
  public showProposeDialog = false;
  public selectedOfferId: string | null = null;
  public selectedOfferName: string | null = null;

  // Analytics Data
  public age: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public gender: { labels: string[]; values: number[] } = { labels: [], values: [] };
  public location: { labels: string[]; values: number[] } = { labels: [], values: [] };
  private readonly route = inject(ActivatedRoute);
  private readonly bloggersApi = inject(BloggersAgent);
  private readonly offersApi = inject(OffersAgent); // Assuming we might need this later or use a specific endpoint
  private readonly api = inject(AgentService); // For analytics
  public ngOnInit(): void {
    const bloggerId = this.route.snapshot.paramMap.get('id');
    if (bloggerId) {
      this.loadBlogger(bloggerId);
    } else {
      this.error = 'Blogger ID not found';
      this.loading = false;
    }
  }

  public formatMetric(value?: number | null): string {
    if (value == null) return '—';
    if (value >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}M`;
    if (value >= 1_000) return `${(value / 1_000).toFixed(1)}K`;
    return value.toLocaleString();
  }

  public openProposeDialog(offerId?: string, offerName?: string): void {
    this.selectedOfferId = offerId || null; // If no offer ID, it might be a general proposal (if supported)
    this.selectedOfferName = offerName || null;
    this.showProposeDialog = true;
  }

  private loadBlogger(id: string): void {
    this.loading = true;
    this.bloggersApi.getBlogger(id).subscribe({
      next: (data) => {
        this.blogger = data;
        this.loading = false;
        // After loading blogger, load their offers and analytics (if public/available)
        // Note: Real implementation might need specific endpoints for public offers/analytics
        // For MVP, we'll assume we can use existing endpoints or mock data if needed.
        // Since getOffers is currently for "my offers", we might need a getBloggerOffers(id) endpoint.
        // For now, we'll leave offers empty or mock them until that endpoint is confirmed/created.
        
        // Load Analytics (Mock or Real if endpoint supports by ID)
        // The current Analytics endpoints use UserContext, so they return "my" analytics.
        // We can't fetch another user's analytics with current endpoints.
        // We will mock analytics for the public view for now as per "No backend updates" rule.
        this.mockAnalytics();
      },
      error: (err) => {
        console.error('Failed to load blogger', err);
        this.error = 'Failed to load blogger profile';
        this.loading = false;
      }
    });
  }  private mockAnalytics(): void {
    this.age = {
      labels: ['13-17', '18-24', '25-34', '35-44', '45-54', '55+'],
      values: [5, 25, 45, 15, 8, 2]
    };
    this.gender = {
      labels: ['Male', 'Female', 'Other'],
      values: [40, 58, 2]
    };
    this.location = {
      labels: ['USA', 'UK', 'Canada', 'Germany', 'France'],
      values: [40, 20, 15, 10, 5]
    };
    
    // Mock Offers for Public View
      this.offers = [
      {
        id: 'mock-offer-1',
        name: 'Instagram Reel',
        priceAmount: 500,
        priceCurrency: 'USD',
        description: 'High quality 60s reel showcasing your product with voiceover.',
        format: DeliverableFormat.InstagramReel
      },
      {
        id: 'mock-offer-2',
        name: 'Instagram Story',
        priceAmount: 150,
        priceCurrency: 'USD',
        description: 'Set of 3 stories with link sticker and mention.',
        format: DeliverableFormat.InstagramStory
      }
    ];
  }
}
