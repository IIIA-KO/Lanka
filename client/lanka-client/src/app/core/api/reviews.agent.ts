import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { IReview, ICreateReviewRequest, IEditReviewRequest } from '../models/campaigns';

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class ReviewsAgent {
  constructor(private http: HttpClient) {}

  private handleError(error: any) {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }

  createReview(request: ICreateReviewRequest): Observable<string> {
    return this.http
      .post<string>(`${BASE_URL}/reviews`, request)
      .pipe(catchError(this.handleError));
  }

  getBloggerReviews(): Observable<IReview[]> {
    return this.http
      .get<IReview[]>(`${BASE_URL}/reviews/blogger`)
      .pipe(catchError(this.handleError));
  }

  editReview(request: IEditReviewRequest): Observable<IReview> {
    return this.http
      .put<IReview>(`${BASE_URL}/reviews/${request.reviewId}`, request)
      .pipe(catchError(this.handleError));
  }

  deleteReview(id: string): Observable<void> {
    return this.http
      .delete<void>(`${BASE_URL}/reviews/${id}`)
      .pipe(catchError(this.handleError));
  }
}
