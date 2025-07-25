import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { IAgeRatio, IGenderRatio, ILocationRatio, IReachRatio } from '../models/analytics/analytics.audience';

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

  getAudienceAgeRatio() : Observable<IAgeRatio> {
    return this.http
      .get<IAgeRatio>(`${BASE_URL}/analytics/audience/age-ratio`)
      .pipe(catchError(this.handleError));
  }

  getAudienceGenderRatio() : Observable<IGenderRatio> {
    return this.http
      .get<IGenderRatio>(`${BASE_URL}/analytics/audience/gender-ratio`)
      .pipe(catchError(this.handleError));
  }

  getAudienceLocationRatio() : Observable<ILocationRatio> {
    return this.http
      .get<ILocationRatio>(`${BASE_URL}/analytics/audience/location-ratio`)
      .pipe(catchError(this.handleError));
  }

  getAudienceReachRatio() : Observable<IReachRatio> {
    return this.http
      .get<IReachRatio>(`${BASE_URL}/analytics/audience/reach-ratio`)
      .pipe(catchError(this.handleError));
  }
}
