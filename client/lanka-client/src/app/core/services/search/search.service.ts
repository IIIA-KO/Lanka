import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, Subscription, debounceTime, distinctUntilChanged, map, of, switchMap } from 'rxjs';
import { SearchAgent, ISearchRequest, ISearchResponse, ISearchResult, ISimilarSearchRequest, ISearchSuggestionsRequest } from '../../api/search.agent';

export interface IBloggerFilters {
  category?: string;
  followersMin?: number;
  followersMax?: number;
  engagementRateMin?: number;
  engagementRateMax?: number;
  audienceCountry?: string;
  audienceGender?: string;
  audienceAgeGroup?: string;
}

export interface ISearchState {
  query: string;
  page: number;
  size: number;
  filters: IBloggerFilters;
  results: ISearchResponse | null;
  loading: boolean;
  error: string | null;
}

@Injectable({
  providedIn: 'root',
})
export class SearchService {
  public readonly searchState$;
  public readonly suggestions$;

  private readonly searchAgent = inject(SearchAgent);
  private readonly searchStateSubject = new BehaviorSubject<ISearchState>({
    query: '',
    page: 1,
    size: 20,
    filters: {},
    results: null,
    loading: false,
    error: null,
  });

  private searchSubscription: Subscription | null = null;
  private excludeItemId?: string;

  constructor() {
    this.searchState$ = this.searchStateSubject.asObservable();

    this.suggestions$ = this.searchState$.pipe(
      map(state => state.query),
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => {
        if (!query.trim() || query.length < 2) {
          return of([]);
        }
        return this.searchAgent.getSearchSuggestions({ query, limit: 10 });
      })
    );
  }

  public search(query: string): void {
    this.updateState({ query, page: 1 });
    this.executeSearch();
  }

  public setPage(page: number): void {
    this.updateState({ page });
    this.executeSearch();
  }

  public setPageSize(size: number): void {
    this.updateState({ size, page: 1 });
    this.executeSearch();
  }

  public setFilters(filters: IBloggerFilters): void {
    this.updateState({ filters, page: 1 });
    this.executeSearch();
  }

  public setExcludeId(id: string): void {
    this.excludeItemId = id;
  }

  public browse(): void {
    this.executeSearch();
  }

  public clearSearch(): void {
    this.searchSubscription?.unsubscribe();
    this.updateState({
      query: '',
      page: 1,
      filters: {},
      results: null,
      loading: false,
      error: null,
    });
  }

  public searchSimilar(sourceItemId: string, sourceType: string, limit = 5): Observable<ISearchResult[]> {
    const request: ISimilarSearchRequest = {
      sourceItemId,
      sourceType,
      limit,
    };

    return this.searchAgent.searchSimilar(request);
  }

  public getSearchSuggestions(query: string, limit = 10): Observable<string[]> {
    const request: ISearchSuggestionsRequest = {
      query,
      limit,
    };

    return this.searchAgent.getSearchSuggestions(request);
  }

  private executeSearch(): void {
    const state = this.searchStateSubject.value;

    this.updateState({ loading: true, error: null });
    this.searchSubscription?.unsubscribe();

    const request: ISearchRequest = {
      q: state.query.trim() || undefined,
      page: state.page,
      size: state.size,
      itemTypes: '1', // Blogger type only
      onlyActive: true,
      followersMin: state.filters.followersMin,
      followersMax: state.filters.followersMax,
      engagementRateMin: state.filters.engagementRateMin,
      engagementRateMax: state.filters.engagementRateMax,
      category: state.filters.category,
      audienceCountry: state.filters.audienceCountry,
      audienceGender: state.filters.audienceGender,
      audienceAgeGroup: state.filters.audienceAgeGroup,
      excludeItemId: this.excludeItemId,
    };

    this.searchSubscription = this.searchAgent.searchDocuments(request).subscribe({
      next: (response) => {
        this.updateState({ results: response, loading: false });
      },
      error: (error) => {
        this.updateState({ loading: false, error: error?.message || 'Search failed' });
      },
    });
  }

  private updateState(updates: Partial<ISearchState>): void {
    const currentState = this.searchStateSubject.value;
    this.searchStateSubject.next({ ...currentState, ...updates });
  }
}
