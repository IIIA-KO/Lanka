import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { ICampaign } from '../models/campaigns';

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class CampaignsAgent {
  constructor(private http: HttpClient) {}

  private handleError(error: any) {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }

  getCampaign(id: string): Observable<ICampaign> {
    return this.http
      .get<ICampaign>(`${BASE_URL}/campaigns/${id}`)
      .pipe(catchError(this.handleError));
  }

  confirmCampaign(id: string): Observable<void> {
    return this.http
      .patch<void>(`${BASE_URL}/campaigns/${id}/confirm`, {})
      .pipe(catchError(this.handleError));
  }

  rejectCampaign(id: string): Observable<void> {
    return this.http
      .patch<void>(`${BASE_URL}/campaigns/${id}/reject`, {})
      .pipe(catchError(this.handleError));
  }

  markCampaignAsDone(id: string): Observable<void> {
    return this.http
      .patch<void>(`${BASE_URL}/campaigns/${id}/done`, {})
      .pipe(catchError(this.handleError));
  }

  completeCampaign(id: string): Observable<void> {
    return this.http
      .patch<void>(`${BASE_URL}/campaigns/${id}/complete`, {})
      .pipe(catchError(this.handleError));
  }

  cancelCampaign(id: string): Observable<void> {
    return this.http
      .patch<void>(`${BASE_URL}/campaigns/${id}/cancel`, {})
      .pipe(catchError(this.handleError));
  }

  pendCampaign(id: string): Observable<void> {
    return this.http
      .patch<void>(`${BASE_URL}/campaigns/${id}/pend`, {})
      .pipe(catchError(this.handleError));
  }
}
