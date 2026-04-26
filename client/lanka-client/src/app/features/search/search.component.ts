import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, take, takeUntil } from 'rxjs';

import { InputTextModule } from 'primeng/inputtext';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';

import { SearchService, ISearchState, IBloggerFilters } from '../../core/services/search/search.service';
import { ISearchResult } from '../../core/api/search.agent';
import { BloggersAgent } from '../../core/api/bloggers.agent';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ThemeService } from '../../core/services/theme/theme.service';

interface SelectOption {
  label: string;
  value: string;
}

// Deterministic category color palette — pastel bg + dark text pairs
const CATEGORY_COLORS: { bg: string; color: string; darkBg: string; darkColor: string }[] = [
  { bg: '#dbeafe', color: '#1d4ed8', darkBg: 'rgba(29, 78, 216, 0.2)', darkColor: '#93c5fd' }, // blue
  { bg: '#dcfce7', color: '#166534', darkBg: 'rgba(22, 101, 52, 0.2)', darkColor: '#86efac' }, // green
  { bg: '#ede9fe', color: '#6d28d9', darkBg: 'rgba(109, 40, 217, 0.2)', darkColor: '#c4b5fd' }, // purple
  { bg: '#ffedd5', color: '#9a3412', darkBg: 'rgba(154, 52, 18, 0.2)', darkColor: '#fdba74' }, // orange
  { bg: '#fce7f3', color: '#9d174d', darkBg: 'rgba(157, 23, 77, 0.2)', darkColor: '#f9a8d4' }, // pink
  { bg: '#fef9c3', color: '#854d0e', darkBg: 'rgba(133, 77, 14, 0.2)', darkColor: '#fde047' }, // yellow
  { bg: '#ccfbf1', color: '#0f766e', darkBg: 'rgba(15, 118, 110, 0.2)', darkColor: '#5eead4' }, // teal
  { bg: '#fee2e2', color: '#991b1b', darkBg: 'rgba(153, 27, 27, 0.2)', darkColor: '#fca5a5' }, // red
  { bg: '#e0e7ff', color: '#3730a3', darkBg: 'rgba(55, 48, 163, 0.2)', darkColor: '#a5b4fc' }, // indigo
];

// Deterministic avatar color palette (teal-ish to match the theme)
const AVATAR_COLORS = [
  '#2d9d9d', '#247f7f', '#206666', '#1e5252', '#1c4444',
  '#3b82f6', '#6366f1', '#8b5cf6', '#a855f7', '#ec4899',
  '#ef4444', '#f97316', '#eab308', '#22c55e', '#14b8a6',
];

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [
    FormsModule,
    InputTextModule,
    AutoCompleteModule,
    ButtonModule,
    SkeletonModule,
    SelectModule,
    TableModule,
    TooltipModule,
    TranslateModule,
  ],
  templateUrl: './search.component.html',
  styleUrl: './search.component.css',
})
export class SearchComponent implements OnInit, OnDestroy {
  public searchState: ISearchState | null = null;
  public searchQuery = '';
  public suggestions: string[] = [];
  public filteredSuggestions: string[] = [];
  public loading = false;
  public error: string | null = null;
  public currentBloggerId: string | null = null;
  public similarResults: ISearchResult[] = [];
  public loadingSimilar = false;

  // Filters
  public selectedCategory: string | null = null;
  public followersMin: number | null = null;
  public followersMax: number | null = null;
  public engagementRateMin: number | null = null;
  public engagementRateMax: number | null = null;
  public selectedCountry: string | null = null;
  public selectedGender: string | null = null;
  public selectedAgeGroup: string | null = null;

  public categoryOptions: SelectOption[] = [];
  public genderOptions: SelectOption[] = [];
  public ageGroupOptions: SelectOption[] = [];
  public countryOptions: SelectOption[] = [];

  public readonly followerRanges: SelectOption[] = [
    { label: 'All', value: '' },
    { label: '1K \u2013 10K', value: '1000-10000' },
    { label: '10K \u2013 50K', value: '10000-50000' },
    { label: '50K \u2013 100K', value: '50000-100000' },
    { label: '100K \u2013 500K', value: '100000-500000' },
    { label: '500K+', value: '500000-' },
  ];
  public selectedFollowerRange: string | null = null;

  public readonly engagementRateRanges: SelectOption[] = [
    { label: 'All', value: '' },
    { label: '< 1%', value: '-1' },
    { label: '1% \u2013 3%', value: '1-3' },
    { label: '3% \u2013 5%', value: '3-5' },
    { label: '5% \u2013 8%', value: '5-8' },
    { label: '8%+', value: '8-' },
  ];
  public selectedEngagementRange: string | null = null;

  private readonly searchService = inject(SearchService);
  private readonly bloggersAgent = inject(BloggersAgent);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroy$ = new Subject<void>();
  private readonly translate = inject(TranslateService);
  private readonly themeService = inject(ThemeService);

  public get hasActiveSearch(): boolean {
    return this.searchQuery.trim().length > 0 || this.activeFilterCount > 0;
  }

  public get activeFilterCount(): number {
    let count = 0;
    if (this.selectedCategory) count++;
    if (this.selectedFollowerRange) count++;
    if (this.selectedEngagementRange) count++;
    if (this.selectedGender) count++;
    if (this.selectedAgeGroup) count++;
    if (this.selectedCountry) count++;
    return count;
  }

  public get currentPage(): number {
    return this.searchState?.page ?? 1;
  }

  public get totalPages(): number {
    if (!this.searchState?.results) return 1;
    return Math.ceil(this.searchState.results.totalCount / this.searchState.results.size);
  }

  public get pageNumbers(): (number | null)[] {
    const total = this.totalPages;
    const current = this.currentPage;

    if (total <= 7) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }

    const shown = new Set<number>([1, total]);
    for (let p = Math.max(1, current - 1); p <= Math.min(total, current + 1); p++) {
      shown.add(p);
    }

    const sorted = Array.from(shown).sort((a, b) => a - b);
    const result: (number | null)[] = [];
    for (let i = 0; i < sorted.length; i++) {
      if (i > 0 && sorted[i] - sorted[i - 1] > 1) result.push(null);
      result.push(sorted[i]);
    }
    return result;
  }

  public ngOnInit(): void {
    this.buildFilterOptions();
    this.translate.onLangChange
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.buildFilterOptions());

    this.subscribeToSearchState();
    this.subscribeToSuggestions();
    this.loadExcludeId();

    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        if (params['q'] && params['q'] !== this.searchQuery) {
          this.searchQuery = params['q'];
        }
        this.applyFilters();
      });
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public onSearchQueryChange(event: { query?: string } | string): void {
    const query = typeof event === 'object' ? event.query : event;
    this.searchQuery = typeof query === 'string' ? query : '';

    if (this.searchQuery.length >= 2) {
      this.searchService.getSearchSuggestions(this.searchQuery)
        .pipe(take(1), takeUntil(this.destroy$))
        .subscribe(suggestions => {
          this.filteredSuggestions = suggestions;
        });
    } else {
      this.filteredSuggestions = [];
    }
  }

  public onSuggestionSelect(event: { value?: string } | string): void {
    const value = typeof event === 'object' ? event.value : event;
    this.searchQuery = typeof value === 'string' ? value : '';
    this.applyFilters();
  }

  public onSearchKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.applyFilters();
    }
  }

  public onSearchSubmit(): void {
    this.applyFilters();
  }

  public onFollowerRangeChange(): void {
    if (this.selectedFollowerRange) {
      const parts = this.selectedFollowerRange.split('-');
      this.followersMin = parts[0] ? Number(parts[0]) : null;
      this.followersMax = parts[1] ? Number(parts[1]) : null;
    } else {
      this.followersMin = null;
      this.followersMax = null;
    }
    this.applyFilters();
  }

  public onEngagementRangeChange(): void {
    if (this.selectedEngagementRange) {
      const parts = this.selectedEngagementRange.split('-');
      this.engagementRateMin = parts[0] ? Number(parts[0]) : null;
      this.engagementRateMax = parts[1] ? Number(parts[1]) : null;
    } else {
      this.engagementRateMin = null;
      this.engagementRateMax = null;
    }
    this.applyFilters();
  }

  public onFilterChange(): void {
    this.applyFilters();
  }

  public goToPage(page: number): void {
    this.searchService.setPage(page);
  }

  public goToPreviousPage(): void {
    if (this.currentPage > 1) this.searchService.setPage(this.currentPage - 1);
  }

  public goToNextPage(): void {
    if (this.currentPage < this.totalPages) this.searchService.setPage(this.currentPage + 1);
  }

  public clearFilters(): void {
    this.selectedCategory = null;
    this.selectedFollowerRange = null;
    this.selectedEngagementRange = null;
    this.followersMin = null;
    this.followersMax = null;
    this.engagementRateMin = null;
    this.engagementRateMax = null;
    this.selectedCountry = null;
    this.selectedGender = null;
    this.selectedAgeGroup = null;
    this.applyFilters();
  }

  public clearSearch(): void {
    this.searchQuery = '';
    this.clearFilters();
  }

  public navigateToResult(result: ISearchResult): void {
    this.router.navigate(['/bloggers', result.itemId]);
  }

  public getFollowers(result: ISearchResult): string {
    const count = result.metadata?.['FollowersCount'] as number;
    if (!count) return '\u2014';
    if (count >= 1_000_000) return (count / 1_000_000).toFixed(1) + 'M';
    if (count >= 1_000) return (count / 1_000).toFixed(1) + 'K';
    return count.toString();
  }

  public getEngagementRate(result: ISearchResult): string {
    const rate = result.metadata?.['EngagementRate'] as number;
    if (!rate) return '\u2014';
    return rate.toFixed(2) + '%';
  }

  public getEngagementClass(result: ISearchResult): string {
    const rate = result.metadata?.['EngagementRate'] as number;
    if (!rate) return 'er-badge er-none';
    if (rate >= 3.5) return 'er-badge er-high';
    if (rate >= 1.5) return 'er-badge er-medium';
    return 'er-badge er-low';
  }

  public getCategory(result: ISearchResult): string {
    return (result.metadata?.['Category'] as string) || '\u2014';
  }

  public getCategoryStyle(category: string): { background: string; color: string } {
    let hash = 0;
    for (let i = 0; i < category.length; i++) {
      hash = category.charCodeAt(i) + ((hash << 5) - hash);
    }
    const { bg, color, darkBg, darkColor } = CATEGORY_COLORS[Math.abs(hash) % CATEGORY_COLORS.length];
    
    if (this.themeService.isDark()) {
      return { background: darkBg, color: darkColor };
    }
    return { background: bg, color };
  }

  public getTopCountry(result: ISearchResult): string {
    const country = result.metadata?.['AudienceTopCountry'] as string;
    const pct = result.metadata?.['AudienceTopCountryPercentage'] as number;
    if (!country) return '\u2014';
    return pct ? `${country} (${pct.toFixed(0)}%)` : country;
  }

  public getTopGender(result: ISearchResult): string {
    const gender = result.metadata?.['AudienceTopGender'] as string;
    const pct = result.metadata?.['AudienceTopGenderPercentage'] as number;
    if (!gender) return '\u2014';
    return pct ? `${gender} (${pct.toFixed(0)}%)` : gender;
  }

  public getTopAgeGroup(result: ISearchResult): string {
    return (result.metadata?.['AudienceTopAgeGroup'] as string) || '\u2014';
  }

  public getInstagramUsername(result: ISearchResult): string {
    return (result.metadata?.['InstagramUsername'] as string) || '';
  }

  public getMediaCount(result: ISearchResult): string {
    const count = result.metadata?.['MediaCount'] as number;
    if (!count) return '\u2014';
    return count.toLocaleString();
  }

  public getInitials(name: string): string {
    if (!name) return '?';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  public getAvatarColor(name: string): string {
    let hash = 0;
    for (let i = 0; i < (name?.length ?? 0); i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length];
  }

  private applyFilters(): void {
    const filters: IBloggerFilters = {};

    if (this.selectedCategory) filters.category = this.selectedCategory;
    if (this.followersMin !== null) filters.followersMin = this.followersMin;
    if (this.followersMax !== null) filters.followersMax = this.followersMax;
    if (this.engagementRateMin !== null) filters.engagementRateMin = this.engagementRateMin;
    if (this.engagementRateMax !== null) filters.engagementRateMax = this.engagementRateMax;
    if (this.selectedCountry) filters.audienceCountry = this.selectedCountry;
    if (this.selectedGender) filters.audienceGender = this.selectedGender;
    if (this.selectedAgeGroup) filters.audienceAgeGroup = this.selectedAgeGroup;
    // Always sync the query text to the service state
    this.searchService.search(this.searchQuery.trim());
    this.searchService.setFilters(filters);
  }

  private loadExcludeId(): void {
    this.bloggersAgent.getProfile()
      .pipe(take(1), takeUntil(this.destroy$))
      .subscribe({
        next: profile => {
          this.currentBloggerId = profile.id;
          this.searchService.setExcludeId(profile.id);
          this.applyFilters();
        },
        error: () => { /* search still works without exclusion */ }
      });
  }

  private subscribeToSearchState(): void {
    this.searchService.searchState$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.searchState = state;
        this.loading = state.loading;
        this.error = state.error;
        this.loadSimilarResults(state);
      });
  }

  private lastSimilarSourceId: string | null = null;

  private loadSimilarResults(state: ISearchState): void {
    const results = state.results?.results;
    if (!results || results.length === 0 || state.loading || !this.hasActiveSearch) {
      this.similarResults = [];
      this.lastSimilarSourceId = null;
      this.loadingSimilar = false;
      return;
    }

    // Use the top result as the source for "similar" query
    const topResult = results[0];
    if (topResult.itemId === this.lastSimilarSourceId) {
      return; // already loaded for this result
    }

    this.lastSimilarSourceId = topResult.itemId;
    this.loadingSimilar = true;
    // Request extra items to compensate for filtering out main results
    this.searchService.searchSimilar(topResult.itemId, topResult.type, 15)
      .pipe(take(1), takeUntil(this.destroy$))
      .subscribe({
        next: (similar) => {
          // Filter out items already in the main results and self, limit to 6
          const mainIds = new Set(results.map(r => r.itemId));
          if (this.currentBloggerId) mainIds.add(this.currentBloggerId);
          this.similarResults = similar.filter(s => !mainIds.has(s.itemId)).slice(0, 6);
          this.loadingSimilar = false;
        },
        error: () => {
          this.similarResults = [];
          this.loadingSimilar = false;
        }
      });
  }

  private subscribeToSuggestions(): void {
    this.searchService.suggestions$
      .pipe(takeUntil(this.destroy$))
      .subscribe(suggestions => {
        this.suggestions = suggestions;
      });
  }

  private buildFilterOptions(): void {
    const categories = [
      'Animals', 'Art', 'Automobiles', 'Clothing and Footwear', 'Comedy',
      'Cooking and Food', 'Cryptocurrency', 'DIY and Crafts', 'Education',
      'Entrepreneurship', 'Environment', 'Fashion and Style', 'Finance',
      'Fitness', 'Gaming', 'Health and Wellness', 'History', 'Home Decor',
      'Horticulture', 'Legal Advice', 'Literature', 'Marketing',
      'Mental Health', 'Movies and TV', 'Music', 'News', 'Parenting',
      'Personal Development', 'Photography', 'Politics', 'Real Estate',
      'Relationships', 'Religion and Spirituality', 'Science',
      'Self Improvement', 'Social Media', 'Sports', 'Technology', 'Travel',
    ];

    this.categoryOptions = [
      { label: this.translate.instant('SEARCH.FILTERS.ALL'), value: '' },
      ...categories.map(c => ({ label: c, value: c })),
    ];

    this.genderOptions = [
      { label: this.translate.instant('SEARCH.FILTERS.ALL'), value: '' },
      { label: 'Male', value: 'Male' },
      { label: 'Female', value: 'Female' },
    ];

    this.ageGroupOptions = [
      { label: this.translate.instant('SEARCH.FILTERS.ALL'), value: '' },
      { label: '13-17', value: '13-17' },
      { label: '18-24', value: '18-24' },
      { label: '25-34', value: '25-34' },
      { label: '35-44', value: '35-44' },
      { label: '45-54', value: '45-54' },
      { label: '55-64', value: '55-64' },
      { label: '65+', value: '65+' },
    ];

    this.countryOptions = [
      { label: this.translate.instant('SEARCH.FILTERS.ALL'), value: '' },
      { label: 'United States', value: 'US' },
      { label: 'United Kingdom', value: 'GB' },
      { label: 'Ukraine', value: 'UA' },
      { label: 'Germany', value: 'DE' },
      { label: 'France', value: 'FR' },
      { label: 'Brazil', value: 'BR' },
      { label: 'India', value: 'IN' },
      { label: 'Canada', value: 'CA' },
      { label: 'Australia', value: 'AU' },
      { label: 'Japan', value: 'JP' },
    ];
  }
}
