import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { catchError, map, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import {
  ICampaign,
  ICampaignReport,
  ICreateCampaignRequest,
  ICreateReviewRequest,
  IMarkCampaignAsDoneRequest,
  IPendCampaignRequest,
} from '../models/campaigns';

const BASE_URL = environment.apiUrl;

interface ICampaignApiResponse {
  id: string;
  status: string;
  name: string;
  description: string;
  offerId: string;
  clientId: string;
  creatorId: string;
  priceAmount: number;
  priceCurrency: string;
  scheduledOnUtc: string;
  pendedOnUtc: string;
  confirmedOnUtc?: string;
  rejectedOnUtc?: string;
  cancelledOnUtc?: string;
  doneOnUtc?: string;
  completedOnUtc?: string;
  creatorFirstName?: string;
  creatorLastName?: string;
  clientFirstName?: string;
  clientLastName?: string;
}

function mapApiResponseToCampaign(r: ICampaignApiResponse): ICampaign {
  return {
    ...r,
    price: { amount: r.priceAmount, currency: r.priceCurrency },
    expectedCompletionDate: r.scheduledOnUtc,
    deliverables: [],
    createdAt: r.pendedOnUtc,
    updatedAt: r.scheduledOnUtc,
  } as ICampaign;
}

@Injectable({
  providedIn: 'root',
})
export class CampaignsAgent {
  private readonly http = inject(HttpClient);

  public createCampaign(request: ICreateCampaignRequest): Observable<string> {
    return this.http
      .post<string>(`${BASE_URL}/campaigns`, request)
      .pipe(catchError(this.handleError));
  }

  public getCampaign(id: string): Observable<ICampaign> {
    return this.http
      .get<ICampaignApiResponse>(`${BASE_URL}/campaigns/${id}`)
      .pipe(
        map(mapApiResponseToCampaign),
        catchError(this.handleError)
      );
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

  public markCampaignAsDone(id: string, report: IMarkCampaignAsDoneRequest): Observable<void> {
    return this.http
      .post<void>(`${BASE_URL}/campaigns/${id}/mark-as-done`, report)
      .pipe(catchError(this.handleError));
  }

  public getCampaignReport(id: string): Observable<ICampaignReport> {
    return this.http
      .get<ICampaignReport>(`${BASE_URL}/campaigns/${id}/report`)
      .pipe(catchError(this.handleError));
  }

  public updateCampaignReport(id: string, report: IMarkCampaignAsDoneRequest): Observable<void> {
    return this.http
      .put<void>(`${BASE_URL}/campaigns/${id}/report`, report)
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

  public proposeCampaign(request: IPendCampaignRequest): Observable<string> {
    return this.http
      .post<string>(`${BASE_URL}/campaigns`, request)
      .pipe(catchError(this.handleError));
  }

  public getCampaigns(): Observable<ICampaign[]> {
    return this.http
      .get<ICampaign[]>(`${BASE_URL}/campaigns`)
      .pipe(catchError(this.handleError));
  }

  public getBloggerCampaigns(bloggerId: string, startDate?: string, endDate?: string): Observable<ICampaign[]> {
    let params = new HttpParams();
    if (startDate) { params = params.set('startDate', startDate); }
    if (endDate) { params = params.set('endDate', endDate); }

    return this.http
      .get<ICampaignApiResponse[]>(`${BASE_URL}/campaigns/bloggers/${bloggerId}`, { params })
      .pipe(
        map(items => items.map(mapApiResponseToCampaign)),
        catchError(this.handleError)
      );
  }

  public getCampaignsByStatus(status: string): Observable<ICampaign[]> {
    return this.http
      .get<ICampaign[]>(`${BASE_URL}/campaigns?status=${status}`)
      .pipe(catchError(this.handleError));
  }

  public createReview(request: ICreateReviewRequest): Observable<string> {
    return this.http
      .post<string>(`${BASE_URL}/reviews`, request)
      .pipe(catchError(this.handleError));
  }

  private handleError(error: { error?: { message?: string }; message?: string }): Observable<never> {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }
}
