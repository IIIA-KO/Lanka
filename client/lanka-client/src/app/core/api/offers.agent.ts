import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { IOffer, ICreateOfferRequest, IEditOfferRequest } from '../models/campaigns';

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class OffersAgent {
  private readonly http = inject(HttpClient);

  public createOffer(request: ICreateOfferRequest): Observable<string> {
    return this.http
      .post<string>(`${BASE_URL}/offers`, request)
      .pipe(catchError(this.handleError));
  }

  public getOffer(id: string): Observable<IOffer> {
    return this.http
      .get<IOffer>(`${BASE_URL}/offers/${id}`)
      .pipe(catchError(this.handleError));
  }

  public editOffer(request: IEditOfferRequest): Observable<IOffer> {
    return this.http
      .put<IOffer>(`${BASE_URL}/offers/${request.offerId}`, request)
      .pipe(catchError(this.handleError));
  }

  public deleteOffer(id: string): Observable<void> {
    return this.http
      .delete<void>(`${BASE_URL}/offers/${id}`)
      .pipe(catchError(this.handleError));
  }

  public getBloggerAverageOfferPrices(): Observable<Record<string, number>> {
    return this.http
      .get<Record<string, number>>(`${BASE_URL}/offers/blogger/average-prices`)
      .pipe(catchError(this.handleError));
  }

  private handleError(error: { error?: { message?: string }; message?: string }): Observable<never> {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }
}
