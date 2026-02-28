import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { catchError, map, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { FriendlyErrorService } from '../services/friendly-error.service';

export interface ISearchRequest {
  q?: string;
  page?: number;
  size?: number;
  itemTypes?: string;
  onlyActive?: boolean;
  priceMin?: number;
  priceMax?: number;
  followersMin?: number;
  followersMax?: number;
  engagementRateMin?: number;
  engagementRateMax?: number;
  category?: string;
  audienceCountry?: string;
  audienceGender?: string;
  audienceAgeGroup?: string;
  createdAfter?: string;
  createdBefore?: string;
  excludeItemId?: string;
}

export interface ISearchHighlight {
  fieldName: string;
  fragments: string[];
}

export interface ISearchResult {
  itemId: string;
  type: string;
  title: string;
  content: string;
  relevanceScore: number;
  highlights: ISearchHighlight[];
  metadata: Record<string, unknown>;
}

export interface ISearchResponse {
  results: ISearchResult[];
  totalCount: number;
  page: number;
  size: number;
  searchDuration: string;
  facets: Record<string, { value: string; count: number }[]>;
}

export interface ISimilarSearchRequest {
  sourceItemId: string;
  sourceType: string;
  limit?: number;
}

export interface ISearchSuggestionsRequest {
  query: string;
  itemType?: number;
  limit?: number;
}

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class SearchAgent {
  private readonly http = inject(HttpClient);
  private readonly friendlyErrorService = inject(FriendlyErrorService);

  public searchDocuments(request: ISearchRequest): Observable<ISearchResponse> {
    let params = new HttpParams();

    if (request.q) params = params.set('q', request.q);
    if (request.page) params = params.set('page', request.page.toString());
    if (request.size) params = params.set('size', request.size.toString());
    if (request.itemTypes) params = params.set('itemTypes', request.itemTypes);
    if (request.onlyActive !== undefined) {
      params = params.set('onlyActive', String(request.onlyActive));
    }
    if (request.priceMin !== undefined) {
      params = params.set('priceMin', request.priceMin.toString());
    }
    if (request.priceMax !== undefined) {
      params = params.set('priceMax', request.priceMax.toString());
    }
    if (request.followersMin !== undefined) {
      params = params.set('followersMin', request.followersMin.toString());
    }
    if (request.followersMax !== undefined) {
      params = params.set('followersMax', request.followersMax.toString());
    }
    if (request.engagementRateMin !== undefined) {
      params = params.set('engagementRateMin', request.engagementRateMin.toString());
    }
    if (request.engagementRateMax !== undefined) {
      params = params.set('engagementRateMax', request.engagementRateMax.toString());
    }
    if (request.category) {
      params = params.set('category', request.category);
    }
    if (request.audienceCountry) {
      params = params.set('audienceCountry', request.audienceCountry);
    }
    if (request.audienceGender) {
      params = params.set('audienceGender', request.audienceGender);
    }
    if (request.audienceAgeGroup) {
      params = params.set('audienceAgeGroup', request.audienceAgeGroup);
    }
    if (request.createdAfter) {
      params = params.set('createdAfter', request.createdAfter);
    }
    if (request.createdBefore) {
      params = params.set('createdBefore', request.createdBefore);
    }
    if (request.excludeItemId) {
      params = params.set('excludeItemId', request.excludeItemId);
    }

    return this.http
      .get<ISearchResponse>(`${BASE_URL}/search`, { params })
      .pipe(catchError(this.handleError));
  }

  public getSearchSuggestions(request: ISearchSuggestionsRequest): Observable<string[]> {
    let params = new HttpParams();

    params = params.set('q', request.query);
    if (request.itemType !== undefined) params = params.set('itemType', request.itemType.toString());
    if (request.limit) params = params.set('limit', request.limit.toString());

    return this.http
      .get<string[]>(`${BASE_URL}/search/suggestions`, { params })
      .pipe(catchError(this.handleError));
  }

  public searchSimilar(request: ISimilarSearchRequest): Observable<ISearchResult[]> {
    let params = new HttpParams();

    params = params.set('sourceType', request.sourceType);
    if (request.limit) params = params.set('size', request.limit.toString());

    return this.http
      .get<ISearchResponse>(`${BASE_URL}/search/similar/${request.sourceItemId}`, { params })
      .pipe(
        map(response => response.results),
        catchError(this.handleError)
      );
  }

  private readonly handleError = (error: unknown): Observable<never> => {
    const friendlyError = this.friendlyErrorService.toFriendlyError(error, {
      notFoundMessage: 'We could not find any items that match your filters.',
      networkMessage: 'We cannot reach the search service right now. Check your connection and try again.',
      fallbackMessage: 'Search is unavailable at the moment. Please try again later.'
    });

    return throwError(() => friendlyError);
  };
}
