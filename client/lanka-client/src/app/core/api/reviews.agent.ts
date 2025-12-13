import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';

export interface IReview {
  id: string;
  bloggerId: string;
  campaignId: string;
  rating: number;
  comment: string;
  createdAt: string;
  updatedAt: string;
}

export interface ICreateReviewRequest {
  bloggerId: string;
  campaignId: string;
  rating: number;
  comment: string;
}

export interface IEditReviewRequest {
  id: string;
  rating: number;
  comment: string;
}

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class ReviewsAgent {
  private readonly http = inject(HttpClient);

  public createReview(request: ICreateReviewRequest): Observable<string> {
    return this.http
      .post<string>(`${BASE_URL}/reviews`, request)
      .pipe(catchError(this.handleError));
  }

  public getBloggerReviews(bloggerId: string): Observable<IReview[]> {
    return this.http
      .get<IReview[]>(`${BASE_URL}/reviews/${bloggerId}`)
      .pipe(catchError(this.handleError));
  }

  public editReview(request: IEditReviewRequest): Observable<IReview> {
    return this.http
      .put<IReview>(`${BASE_URL}/reviews/${request.id}`, request)
      .pipe(catchError(this.handleError));
  }

  public deleteReview(id: string): Observable<void> {
    return this.http
      .delete<void>(`${BASE_URL}/reviews/${id}`)
      .pipe(catchError(this.handleError));
  }

  private handleError(error: { error?: { message?: string }; message?: string }): Observable<never> {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }
}