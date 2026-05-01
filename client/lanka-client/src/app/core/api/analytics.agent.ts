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

    if (locationType != null) {
      params = params.set('locationType', locationType.toString());
    }

    return this.http
      .get<ILocationDistribution>(`${BASE_URL}/analytics/audience/location-distribution`, { params })
      .pipe(catchError(this.handleError));
  }

  public getReachDistribution(period: StatisticsPeriod): Observable<IReachDistribution> {
    const params = new HttpParams().set('period', period.toString());

    return this.http
      .get<IReachDistribution>(`${BASE_URL}/analytics/audience/reach-distribution`, { params })
      .pipe(catchError(this.handleError));
  }

  // Public blogger audience analytics (by userId)
  public getBloggerAgeDistribution(userId: string): Observable<IAgeDistribution> {
    return this.http
      .get<IAgeDistribution>(`${BASE_URL}/analytics/audience/${userId}/age-distribution`)
      .pipe(catchError(this.handleError));
  }

  public getBloggerGenderDistribution(userId: string): Observable<IGenderDistribution> {
    return this.http
      .get<IGenderDistribution>(`${BASE_URL}/analytics/audience/${userId}/gender-distribution`)
      .pipe(catchError(this.handleError));
  }

  public getBloggerLocationDistribution(userId: string, locationType?: LocationType): Observable<ILocationDistribution> {
    let params = new HttpParams();
    if (locationType != null) {
      params = params.set('locationType', locationType.toString());
    }
    return this.http
      .get<ILocationDistribution>(`${BASE_URL}/analytics/audience/${userId}/location-distribution`, { params })
      .pipe(catchError(this.handleError));
  }

  public getBloggerReachDistribution(userId: string, period: StatisticsPeriod): Observable<IReachDistribution> {
    const params = new HttpParams().set('period', period.toString());
    return this.http
      .get<IReachDistribution>(`${BASE_URL}/analytics/audience/${userId}/reach-distribution`, { params })
      .pipe(catchError(this.handleError));
  }

  // Public blogger statistics (by userId)
  public getBloggerOverviewStatistics(userId: string, period: StatisticsPeriod): Observable<IOverviewStatisticsResponse> {
    const params = new HttpParams().set('period', period.toString());
    return this.http
      .get<IOverviewStatisticsResponse>(`${BASE_URL}/analytics/statistics/${userId}/overview`, { params })
      .pipe(catchError(this.handleError));
  }

  public getBloggerEngagementStatistics(userId: string, period: StatisticsPeriod): Observable<IEngagementStatisticsResponse> {
    const params = new HttpParams().set('period', period.toString());
    return this.http
      .get<IEngagementStatisticsResponse>(`${BASE_URL}/analytics/statistics/${userId}/engagement`, { params })
      .pipe(catchError(this.handleError));
  }

  public getBloggerInteractionStatistics(userId: string, period: StatisticsPeriod): Observable<IInteractionStatisticsResponse> {
    const params = new HttpParams().set('period', period.toString());
    return this.http
      .get<IInteractionStatisticsResponse>(`${BASE_URL}/analytics/statistics/${userId}/interaction`, { params })
      .pipe(catchError(this.handleError));
  }

  // Public blogger posts (by userId)
  public getBloggerPosts(userId: string, limit = 6): Observable<IPostsResponse> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http
      .get<IPostsResponse>(`${BASE_URL}/analytics/posts/${userId}`, { params })
      .pipe(catchError(this.handleError));
  }

  // Posts Analytics
  public getPosts(queryParams?: IPostsQueryParams): Observable<IPostsResponse> {
    let params = new HttpParams();

    if (queryParams) {
      if (queryParams.limit) {
        params = params.set('limit', queryParams.limit.toString());
      }

      if (queryParams.after) {
        params = params.set('cursorType', 'after');
        params = params.set('cursor', queryParams.after);
      } else if (queryParams.before) {
        params = params.set('cursorType', 'before');
        params = params.set('cursor', queryParams.before);
      }

      if (queryParams.since) {
        params = params.set('since', queryParams.since);
      }

      if (queryParams.until) {
        params = params.set('until', queryParams.until);
      }
    }

    return this.http
      .get<IPostsResponse>(`${BASE_URL}/analytics/posts`, { params })
      .pipe(catchError(this.handleError));
  }

  // Statistics Analytics
  public getOverviewStatistics(period: StatisticsPeriod): Observable<IOverviewStatisticsResponse> {
    const params = new HttpParams().set('period', period.toString());

    return this.http
      .get<IOverviewStatisticsResponse>(`${BASE_URL}/analytics/overview-statistics`, { params })
      .pipe(catchError(this.handleError));
  }

  public getEngagementStatistics(period: StatisticsPeriod): Observable<IEngagementStatisticsResponse> {
    const params = new HttpParams().set('period', period.toString());

    return this.http
      .get<IEngagementStatisticsResponse>(`${BASE_URL}/analytics/engagement-statistics`, { params })
      .pipe(catchError(this.handleError));
  }

  public getInteractionStatistics(period: StatisticsPeriod): Observable<IInteractionStatisticsResponse> {
    const params = new HttpParams().set('period', period.toString());

    return this.http
      .get<IInteractionStatisticsResponse>(`${BASE_URL}/analytics/interaction-statistics`, { params })
      .pipe(catchError(this.handleError));
  }

  public getTableStatistics(period: StatisticsPeriod): Observable<IMetricsStatisticsResponse> {
    const params = new HttpParams().set('period', period.toString());

    return this.http
      .get<IMetricsStatisticsResponse>(`${BASE_URL}/analytics/table-statistics`, { params })
      .pipe(catchError(this.handleError));
  }

  private handleError(error: { error?: { message?: string }; message?: string }): Observable<never> {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }
}
