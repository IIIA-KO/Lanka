import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';
import { SearchAgent, ISearchRequest, ISearchResponse, ISimilarSearchRequest, ISearchSuggestionsRequest } from '../../api/search.agent';

export interface ISearchFilters {
  type?: string;
  dateRange?: {
    from: Date;
    to: Date;
  };
  priceRange?: {
    min: number;
    max: number;
  };
  rating?: number;
  // Blogger-specific filters
  minFollowers?: number;
  maxFollowers?: number;
  engagementRate?: string; // 'low' | 'medium' | 'high'
  location?: string;
  niches?: string[];
}

export interface ISearchState {
  query: string;
  page: number;
  size: number;
  sort: string;
  filters: ISearchFilters;
  results: ISearchResponse | null;
  loading: boolean;
  error: string | null;
}

@Injectable({
  providedIn: 'root',
})
export class SearchService {
  public readonly searchState$;
  public readonly searchResults$;
  public readonly suggestions$;

  private readonly searchAgent = inject(SearchAgent);
  private readonly searchStateSubject = new BehaviorSubject<ISearchState>({
    query: '',
    page: 1,
    size: 10,
    sort: 'relevance',
    filters: {},
    results: null,
    loading: false,
    error: null,
  });

  constructor() {
    this.searchState$ = this.searchStateSubject.asObservable();

    this.searchResults$ = this.searchState$.pipe(
      switchMap(state => {
        if (!state.query.trim()) {
          return of(null);
        }
        return this.performSearch(state);
      })
    );

    this.suggestions$ = this.searchState$.pipe(
      debounceTime(300),
      distinctUntilChanged((prev, curr) => prev.query === curr.query),
      switchMap(state => {
        if (!state.query.trim() || state.query.length < 2) {
          return of([]);
        }
        return this.getSuggestions(state.query);
      })
    );
  }

  public search(query: string): void {
    this.updateSearchState({ query, page: 1 });
  }

  public setPage(page: number): void {
    this.updateSearchState({ page });
  }

  public setPageSize(size: number): void {
    this.updateSearchState({ size, page: 1 });
  }

  public setSort(sort: string): void {
    this.updateSearchState({ sort, page: 1 });
  }

  public setFilters(filters: ISearchFilters): void {
    this.updateSearchState({ filters, page: 1 });
  }

  public clearSearch(): void {
    this.updateSearchState({
      query: '',
      page: 1,
      results: null,
      error: null,
    });
  }

  public searchSimilar(sourceItemId: string, sourceType: string, limit = 5): Observable<unknown[]> {
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

  private performSearch(state: ISearchState): Observable<ISearchResponse | null> {
    if (!state.query.trim()) {
      return of(null);
    }

    this.updateSearchState({ loading: true, error: null });

    const request: ISearchRequest = {
      q: state.query,
      page: state.page,
      size: state.size,
      sort: state.sort,
      filters: this.buildFilters(state.filters),
    };

    return this.searchAgent.searchDocuments(request);
  }

  private getSuggestions(query: string): Observable<string[]> {
    return this.searchAgent.getSearchSuggestions({ query, limit: 10 });
  }

  private buildFilters(filters: ISearchFilters): Record<string, unknown> {
    const result: Record<string, unknown> = {};

    if (filters.type) {
      result['type'] = filters.type;
    }

    if (filters.dateRange) {
      result['dateFrom'] = filters.dateRange.from.toISOString();
      result['dateTo'] = filters.dateRange.to.toISOString();
    }

    if (filters.priceRange) {
      result['priceMin'] = filters.priceRange.min;
      result['priceMax'] = filters.priceRange.max;
    }

    if (filters.rating !== undefined) {
      result['rating'] = filters.rating;
    }

    // Blogger-specific filters
    if (filters.minFollowers !== undefined) {
      result['minFollowers'] = filters.minFollowers;
    }

    if (filters.maxFollowers !== undefined) {
      result['maxFollowers'] = filters.maxFollowers;
    }

    if (filters.engagementRate) {
      result['engagementRate'] = filters.engagementRate;
    }

    if (filters.location) {
      result['location'] = filters.location;
    }

    if (filters.niches && filters.niches.length > 0) {
      result['niches'] = filters.niches.join(',');
    }

    return result;
  }

  private updateSearchState(updates: Partial<ISearchState>): void {
    const currentState = this.searchStateSubject.value;
    this.searchStateSubject.next({ ...currentState, ...updates });
  }
}
