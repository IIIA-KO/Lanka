import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

// PrimeNG Modules
import { InputTextModule } from 'primeng/inputtext';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { SliderModule } from 'primeng/slider';
import { RadioButtonModule } from 'primeng/radiobutton';
import { PaginatorModule } from 'primeng/paginator';
import { MessageModule } from 'primeng/message';
import { DividerModule } from 'primeng/divider';

import { SearchService, ISearchState, ISearchFilters } from '../../../core/services/search/search.service';
import { SearchAgent, ISearchResult } from '../../../core/api/search.agent';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    InputTextModule,
    AutoCompleteModule,
    CardModule,
    TagModule,
    ButtonModule,
    SkeletonModule,
    SliderModule,
    RadioButtonModule,
    PaginatorModule,
    MessageModule,
    DividerModule,
    TranslateModule
  ],
  templateUrl: './search.component.html',
  styleUrl: './search.component.css'
})
export class SearchComponent implements OnInit, OnDestroy {
  public searchState: ISearchState | null = null;
  public searchQuery = '';
  public suggestions: string[] = [];
  public filteredSuggestions: string[] = [];
  public loading = false;
  public error: string | null = null;

  // Instagram-specific filters
  public followerRange: number[] = [0, 10000000]; // 0 to 10M
  public maxFollowers = 10000000;
  public engagementRate = 'any';
  public engagementOptions: { label: string; value: string }[] = [];

  private readonly searchService = inject(SearchService);
  private readonly searchAgent = inject(SearchAgent);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();
  private readonly translate = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);

  public ngOnInit(): void {
    this.buildEngagementOptions();
    this.translate.onLangChange
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.buildEngagementOptions());

    this.subscribeToSearchState();
    this.subscribeToSuggestions();
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public onSearchQueryChange(event: { query?: string } | string): void {
    const query = typeof event === 'object' ? event.query : event;
    this.searchQuery = typeof query === 'string' ? query : '';
    
    if (this.searchQuery.length >= 2) {
      this.filteredSuggestions = this.suggestions.filter(s => 
        s.toLowerCase().includes(this.searchQuery.toLowerCase())
      );
    } else {
      this.filteredSuggestions = [];
    }
  }

  public onSuggestionSelect(event: { value?: string } | string): void {
    const value = typeof event === 'object' ? event.value : event;
    this.searchQuery = typeof value === 'string' ? value : '';
    this.executeSearch();
  }

  public onSearchSubmit(): void {
    this.executeSearch();
  }

  public onFollowerRangeChange(): void {
    this.applyFilters();
  }

  public onEngagementRateChange(): void {
    this.applyFilters();
  }

  public onPageChange(event: { page?: number }): void {
    if (event.page !== undefined) {
      this.searchService.setPage(event.page + 1);
    }
  }

  public clearSearch(): void {
    this.searchQuery = '';
    this.followerRange = [0, 10000000];
    this.engagementRate = 'any';
    this.searchService.clearSearch();
  }

  public navigateToResult(result: ISearchResult): void {
    // Always navigate to public blogger profile
    this.router.navigate(['/bloggers', result.id]);
  }

  public formatFollowerCount(count: number): string {
    if (count >= 1000000) {
      return `${(count / 1000000).toFixed(1)}M`;
    } else if (count >= 1000) {
      return `${(count / 1000).toFixed(1)}K`;
    }
    return count.toString();
  }

  public formatFollowerRange(): string {
    const min = this.formatFollowerCount(this.followerRange[0]);
    const max = this.followerRange[1] >= this.maxFollowers 
      ? '10M+' 
      : this.formatFollowerCount(this.followerRange[1]);
    return `${min} - ${max}`;
  }

  public getFollowerCount(result: ISearchResult): number {
    return (result.metadata?.['followerCount'] as number) || 0;
  }

  public getPostCount(result: ISearchResult): number {
    return (result.metadata?.['postCount'] as number) || 0;
  }

  public getEngagementRate(result: ISearchResult): number {
    return (result.metadata?.['engagementRate'] as number) || 0;
  }

  public getLocation(result: ISearchResult): string {
    return (result.metadata?.['location'] as string) || '';
  }

  public getRating(result: ISearchResult): number {
    return (result.metadata?.['rating'] as number) || 0;
  }

  public getUsername(result: ISearchResult): string {
    return (result.metadata?.['username'] as string) || '';
  }

  public getAvatarUrl(result: ISearchResult): string {
    return (result.metadata?.['avatarUrl'] as string) || '/icons/no-profile.svg';
  }

  private executeSearch(): void {
    if (this.searchQuery.trim()) {
      this.searchService.search(this.searchQuery.trim());
    }
  }

  private applyFilters(): void {
    const filters: ISearchFilters = {
      type: 'blogger' // Always search for bloggers only
    };

    // Add follower range filter
    if (this.followerRange[0] > 0 || this.followerRange[1] < this.maxFollowers) {
      filters.minFollowers = this.followerRange[0];
      filters.maxFollowers = this.followerRange[1];
    }

    // Add engagement rate filter
    if (this.engagementRate !== 'any') {
      filters.engagementRate = this.engagementRate;
    }

    this.searchService.setFilters(filters);
  }

  private subscribeToSearchState(): void {
    this.searchService.searchState$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.searchState = state;
        this.loading = state.loading;
        this.error = state.error;
      });
  }

  private subscribeToSuggestions(): void {
    this.searchService.suggestions$
      .pipe(takeUntil(this.destroy$))
      .subscribe(suggestions => {
        this.suggestions = suggestions;
      });
  }

  private buildEngagementOptions(): void {
    this.engagementOptions = [
      { label: this.translate.instant('SEARCH.ENGAGEMENT_OPTIONS.ANY'), value: 'any' },
      { label: this.translate.instant('SEARCH.ENGAGEMENT_OPTIONS.LOW'), value: 'low' },
      { label: this.translate.instant('SEARCH.ENGAGEMENT_OPTIONS.MEDIUM'), value: 'medium' },
      { label: this.translate.instant('SEARCH.ENGAGEMENT_OPTIONS.HIGH'), value: 'high' }
    ];
  }
}
