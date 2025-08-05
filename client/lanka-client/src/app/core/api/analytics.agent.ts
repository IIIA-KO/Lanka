import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { IAgeDistribution, IGenderDistribution, ILocationDistribution, IReachDistribution, LocationType, StatisticsPeriod } from '../models/analytics/analytics.audience';

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class AnalyticsAgent {
  constructor(private http: HttpClient) {}

  private handleError(error: any) {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }

  getAgeDistribution() : Observable<IAgeDistribution> {
    return this.http
      .get<IAgeDistribution>(`${BASE_URL}/analytics/audience/age-distribution`)
      .pipe(catchError(this.handleError));
  }

  getGenderDistribution() : Observable<IGenderDistribution> {
    return this.http
      .get<IGenderDistribution>(`${BASE_URL}/analytics/audience/gender-distribution`)
      .pipe(catchError(this.handleError));
  }

  getLocationDistribution(locationType?: LocationType) : Observable<ILocationDistribution> {
    let params = new HttpParams();

    if (locationType) {
      params = params.set('locationType', locationType);
    }

    return this.http
      .get<ILocationDistribution>(`${BASE_URL}/analytics/audience/location-distribution`, { params })
      .pipe(catchError(this.handleError));
  }

  getReachDistribution(period: StatisticsPeriod) : Observable<IReachDistribution> {
    let params = new HttpParams();

    if (period) {
      params = params.set('period', period);
    }

    return this.http
      .get<IReachDistribution>(`${BASE_URL}/analytics/audience/reach-distribution`, { params })
      .pipe(catchError(this.handleError));
  }
}
