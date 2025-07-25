import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { catchError, Observable, throwError } from "rxjs";
import { IBloggerProfile } from "../models/blogger";
import { environment } from "../../../environments/environment.development";

const BASE_URL = environment.apiUrl;

@Injectable({
    providedIn: 'root'
})
export class BloggersAgent {
    constructor(private http: HttpClient) {}

    private handleError(error: any) {
      const message = error.error?.message || error.message || "Unknown error";
      return throwError(() => new Error(message));
    }

    getProfile(): Observable<IBloggerProfile> {
      return this.http
        .get<IBloggerProfile>(`${BASE_URL}/bloggers/profile`)
        .pipe(catchError(this.handleError));
    }

    updateProfile(profileData: { firstName: string; lastName: string; birthDate: string; bio: string }): Observable<IBloggerProfile> {
      return this.http
        .put<IBloggerProfile>(`${BASE_URL}/bloggers`, profileData)
        .pipe(catchError(this.handleError));
    }

    uploadProfilePhoto(file: File): Observable<any> {
      const formData = new FormData();
      formData.append('photo', file);
      return this.http
        .post(`${BASE_URL}/bloggers/photos`, formData)
        .pipe(catchError(this.handleError));
    }

    deleteProfilePhoto(): Observable<any> {
      return this.http
        .delete(`${BASE_URL}/bloggers/photos`)
        .pipe(catchError(this.handleError));
    }
  }
