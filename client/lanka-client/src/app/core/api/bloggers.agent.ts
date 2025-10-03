import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { IBloggerProfile } from '../models/blogger';
import { environment } from '../../../environments/environment.development';

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root'
})
export class BloggersAgent {
  private readonly http = inject(HttpClient);

  public getProfile(): Observable<IBloggerProfile> {
      return this.http
        .get<IBloggerProfile>(`${BASE_URL}/bloggers/profile`)
        .pipe(catchError(this.handleError));
    }

  public updateProfile(profileData: { firstName: string; lastName: string; birthDate: string; bio: string }): Observable<IBloggerProfile> {
      return this.http
        .put<IBloggerProfile>(`${BASE_URL}/bloggers`, profileData)
        .pipe(catchError(this.handleError));
    }

  public uploadProfilePhoto(file: File): Observable<unknown> {
      const formData = new FormData();
      formData.append('photo', file);
      return this.http
        .post(`${BASE_URL}/bloggers/photos`, formData)
        .pipe(catchError(this.handleError));
    }

  public deleteProfilePhoto(): Observable<unknown> {
      return this.http
        .delete(`${BASE_URL}/bloggers/photos`)
        .pipe(catchError(this.handleError));
    }

  private handleError(error: { error?: { message?: string }; message?: string }): Observable<never> {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }
}
