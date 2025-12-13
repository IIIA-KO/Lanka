import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { FriendlyErrorService } from '../services/friendly-error.service';

export interface ISearchRequest {
  q: string;
  page?: number;
  size?: number;
  sort?: string;
  filters?: Record<string, unknown>;
  itemTypes?: string;
  onlyActive?: boolean;
}

export interface ISearchResult {
  id: string;
  title: string;
  content: string;
  type: string;
  score: number;
  highlights?: string[];
  metadata?: Record<string, unknown>;
}

export interface ISearchResponse {
  results: ISearchResult[];
  total: number;
  page: number;
  size: number;
  took: number;
  suggestions?: string[];
}

export interface ISimilarSearchRequest {
  sourceItemId: string;
  sourceType: string;
  limit?: number;
}

export interface ISearchSuggestionsRequest {
  query: string;
  limit?: number;
}

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class SearchAgent {
  private readonly http = inject(HttpClient);
  private readonly friendlyErrorService = inject(FriendlyErrorService);

  /**
   * Search for documents using Elasticsearch
   */
  public searchDocuments(request: ISearchRequest): Observable<ISearchResponse> {
    let params = new HttpParams();
    
    params = params.set('q', request.q);
    if (request.page) params = params.set('page', request.page.toString());
    if (request.size) params = params.set('size', request.size.toString());
    if (request.sort) params = params.set('sort', request.sort);
    if (request.itemTypes) params = params.set('itemTypes', request.itemTypes);
    if (request.onlyActive !== undefined) {
      params = params.set('onlyActive', String(request.onlyActive));
    }
    
    // Add filters as query parameters
    if (request.filters) {
      Object.entries(request.filters).forEach(([key, value]) => {
        if (value !== null && value !== undefined) {
          params = params.set(`filters.${key}`, value.toString());
        }
      });
    }

    return this.http
      .get<ISearchResponse>(`${BASE_URL}/searchable-documents`, { params })
      .pipe(catchError(this.handleError));
  }

  /**
   * Get search suggestions based on query
   */
  public getSearchSuggestions(request: ISearchSuggestionsRequest): Observable<string[]> {
    let params = new HttpParams();
    
    params = params.set('query', request.query);
    if (request.limit) params = params.set('limit', request.limit.toString());

    return this.http
      .get<string[]>(`${BASE_URL}/searchable-documents/suggestions`, { params })
      .pipe(catchError(this.handleError));
  }

  /**
   * Find similar documents using More Like This (MLT) query
   */
  public searchSimilar(request: ISimilarSearchRequest): Observable<ISearchResult[]> {
    let params = new HttpParams();
    
    params = params.set('sourceType', request.sourceType);
    if (request.limit) params = params.set('limit', request.limit.toString());

    return this.http
      .get<ISearchResult[]>(`${BASE_URL}/searchable-documents/similar/${request.sourceItemId}`, { params })
      .pipe(catchError(this.handleError));
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
