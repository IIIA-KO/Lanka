import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { IOffer, ICreateOfferRequest, IEditOfferRequest } from '../models/campaigns';

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class OffersAgent {
  constructor(private http: HttpClient) {}

  private handleError(error: any) {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }

  createOffer(request: ICreateOfferRequest): Observable<string> {
    return this.http
      .post<string>(`${BASE_URL}/offers`, request)
      .pipe(catchError(this.handleError));
  }

  getOffer(id: string): Observable<IOffer> {
    return this.http
      .get<IOffer>(`${BASE_URL}/offers/${id}`)
      .pipe(catchError(this.handleError));
  }

  editOffer(request: IEditOfferRequest): Observable<IOffer> {
    return this.http
      .put<IOffer>(`${BASE_URL}/offers/${request.offerId}`, request)
      .pipe(catchError(this.handleError));
  }

  deleteOffer(id: string): Observable<void> {
    return this.http
      .delete<void>(`${BASE_URL}/offers/${id}`)
      .pipe(catchError(this.handleError));
  }

  getBloggerAverageOfferPrices(): Observable<any> {
    return this.http
      .get<any>(`${BASE_URL}/offers/blogger/average-prices`)
      .pipe(catchError(this.handleError));
  }
}
