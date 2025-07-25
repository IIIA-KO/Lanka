import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import {
  ILoginRequest,
  IRegisterRequest,
  IRefreshTokenRequest,
  ITokenResponse,
  IRegisterResponse,
} from '../models/auth';
import { environment } from '../../../environments/environment.development';

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class UsersAgent {
  constructor(private http: HttpClient) {}

  private handleError(error: any) {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }

  login(loginRequest: ILoginRequest): Observable<ITokenResponse> {
    return this.http
      .post<ITokenResponse>(`${BASE_URL}/users/login`, loginRequest)
      .pipe(catchError(this.handleError));
  }

  register(registerRequest: IRegisterRequest): Observable<IRegisterResponse> {
    return this.http
      .post<IRegisterResponse>(`${BASE_URL}/users/register`, registerRequest)
      .pipe(catchError(this.handleError));
  }

  refreshToken(
    refreshTokenRequest: IRefreshTokenRequest
  ): Observable<ITokenResponse> {
    return this.http
      .post<ITokenResponse>(
        `${BASE_URL}/users/refresh-token`,
        refreshTokenRequest
      )
      .pipe(catchError(this.handleError));
  }

  linkInstagram(code: string): Observable<any> {
    return this.http
      .post(`${BASE_URL}/users/link-instagram`, { code })
      .pipe(catchError(this.handleError));
  }

  renewInstagramAccess(code: string): Observable<any> {
    return this.http
      .post(`${BASE_URL}/users/renew-instagram-access`, { code })
      .pipe(catchError(this.handleError));
  }
}
