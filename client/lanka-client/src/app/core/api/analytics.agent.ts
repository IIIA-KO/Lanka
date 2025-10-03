import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { IAgeDistribution, IGenderDistribution, ILocationDistribution, IReachDistribution, LocationType, StatisticsPeriod } from '../models/analytics/analytics.audience';
import { IPostsResponse, IPostsQueryParams } from '../models/analytics/analytics.posts';
import { IOverviewStatisticsResponse, IEngagementStatisticsResponse, IInteractionStatisticsResponse, IMetricsStatisticsResponse } from '../models/analytics/analytics.statistics';

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class AnalyticsAgent {
  private readonly http = inject(HttpClient);

  public getAgeDistribution(): Observable<IAgeDistribution> {
    return this.http
      .get<IAgeDistribution>(`${BASE_URL}/analytics/audience/age-distribution`)
      .pipe(catchError(this.handleError));
  }

  public getGenderDistribution(): Observable<IGenderDistribution> {
    return this.http
      .get<IGenderDistribution>(`${BASE_URL}/analytics/audience/gender-distribution`)
      .pipe(catchError(this.handleError));
  }

  public getLocationDistribution(locationType?: LocationType): Observable<ILocationDistribution> {
    let params = new HttpParams();

    if (locationType) {
      params = params.set('locationType', locationType);
    }

    return this.http
      .get<ILocationDistribution>(`${BASE_URL}/analytics/audience/location-distribution`, { params })
      .pipe(catchError(this.handleError));
  }

  public getReachDistribution(period: StatisticsPeriod): Observable<IReachDistribution> {
    let params = new HttpParams();

    if (period) {
      params = params.set('period', period);
    }

    return this.http
      .get<IReachDistribution>(`${BASE_URL}/analytics/audience/reach-distribution`, { params })
      .pipe(catchError(this.handleError));
  }

  // Posts Analytics
  public getPosts(queryParams?: IPostsQueryParams): Observable<IPostsResponse> {
    let params = new HttpParams();

    if (queryParams) {
      if (queryParams.limit) params = params.set('limit', queryParams.limit.toString());
      if (queryParams.after) params = params.set('after', queryParams.after);
      if (queryParams.before) params = params.set('before', queryParams.before);
      if (queryParams.since) params = params.set('since', queryParams.since);
      if (queryParams.until) params = params.set('until', queryParams.until);
    }

    return this.http
      .get<IPostsResponse>(`${BASE_URL}/analytics/posts`, { params })
      .pipe(catchError(this.handleError));
  }

  // Statistics Analytics
  public getOverviewStatistics(period: StatisticsPeriod): Observable<IOverviewStatisticsResponse> {
    let params = new HttpParams();
    if (period) params = params.set('period', period);

    return this.http
      .get<IOverviewStatisticsResponse>(`${BASE_URL}/analytics/statistics/overview`, { params })
      .pipe(catchError(this.handleError));
  }

  public getEngagementStatistics(period: StatisticsPeriod): Observable<IEngagementStatisticsResponse> {
    let params = new HttpParams();
    if (period) params = params.set('period', period);

    return this.http
      .get<IEngagementStatisticsResponse>(`${BASE_URL}/analytics/statistics/engagement`, { params })
      .pipe(catchError(this.handleError));
  }

  public getInteractionStatistics(period: StatisticsPeriod): Observable<IInteractionStatisticsResponse> {
    let params = new HttpParams();
    if (period) params = params.set('period', period);

    return this.http
      .get<IInteractionStatisticsResponse>(`${BASE_URL}/analytics/statistics/interaction`, { params })
      .pipe(catchError(this.handleError));
  }

  public getTableStatistics(period: StatisticsPeriod): Observable<IMetricsStatisticsResponse> {
    let params = new HttpParams();
    if (period) params = params.set('period', period);

    return this.http
      .get<IMetricsStatisticsResponse>(`${BASE_URL}/analytics/statistics/table`, { params })
      .pipe(catchError(this.handleError));
  }

  private handleError(error: { error?: { message?: string }; message?: string }): Observable<never> {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }
}
