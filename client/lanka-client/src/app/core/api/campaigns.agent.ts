import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { ICampaign } from '../models/campaigns';

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class CampaignsAgent {
  private readonly http = inject(HttpClient);

  public getCampaign(id: string): Observable<ICampaign> {
    return this.http
      .get<ICampaign>(`${BASE_URL}/campaigns/${id}`)
      .pipe(catchError(this.handleError));
  }

  public confirmCampaign(id: string): Observable<void> {
    return this.http
      .patch<void>(`${BASE_URL}/campaigns/${id}/confirm`, {})
      .pipe(catchError(this.handleError));
  }

  public rejectCampaign(id: string): Observable<void> {
    return this.http
      .patch<void>(`${BASE_URL}/campaigns/${id}/reject`, {})
      .pipe(catchError(this.handleError));
  }

  public markCampaignAsDone(id: string): Observable<void> {
    return this.http
      .patch<void>(`${BASE_URL}/campaigns/${id}/done`, {})
      .pipe(catchError(this.handleError));
  }

  public completeCampaign(id: string): Observable<void> {
    return this.http
      .patch<void>(`${BASE_URL}/campaigns/${id}/complete`, {})
      .pipe(catchError(this.handleError));
  }

  public cancelCampaign(id: string): Observable<void> {
    return this.http
      .patch<void>(`${BASE_URL}/campaigns/${id}/cancel`, {})
      .pipe(catchError(this.handleError));
  }

  public pendCampaign(id: string): Observable<void> {
    return this.http
      .patch<void>(`${BASE_URL}/campaigns/${id}/pend`, {})
      .pipe(catchError(this.handleError));
  }

  private handleError(error: { error?: { message?: string }; message?: string }): Observable<never> {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }
}
