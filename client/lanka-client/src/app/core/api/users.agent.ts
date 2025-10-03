import { Injectable, inject } from '@angular/core';
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
  private readonly http = inject(HttpClient);

  public login(loginRequest: ILoginRequest): Observable<ITokenResponse> {
    return this.http
      .post<ITokenResponse>(`${BASE_URL}/users/login`, loginRequest)
      .pipe(catchError(this.handleError));
  }

  public register(registerRequest: IRegisterRequest): Observable<IRegisterResponse> {
    return this.http
      .post<IRegisterResponse>(`${BASE_URL}/users/register`, registerRequest)
      .pipe(catchError(this.handleError));
  }

  public refreshToken(
    refreshTokenRequest: IRefreshTokenRequest
  ): Observable<ITokenResponse> {
    return this.http
      .post<ITokenResponse>(
        `${BASE_URL}/users/refresh-token`,
        refreshTokenRequest
      )
      .pipe(catchError(this.handleError));
  }

  public linkInstagram(code: string): Observable<unknown> {
    return this.http
      .post(`${BASE_URL}/users/link-instagram`, { code }, { observe: 'response' })
      .pipe(catchError(this.handleError));
  }

  public renewInstagramAccess(code: string): Observable<unknown> {
    return this.http
      .post(`${BASE_URL}/users/renew-instagram-access`, { code }, { observe: 'response' })
      .pipe(catchError(this.handleError));
  }

  public getLinkInstagramStatus(): Observable<{ status: string }> {
    return this.http
      .get<{ status: string }>(`${BASE_URL}/users/link-instagram/status`)
      .pipe(catchError(this.handleError));
  }

  public getRenewInstagramStatus(): Observable<{ status: string }> {
    return this.http
      .get<{ status: string }>(`${BASE_URL}/users/renew-instagram-access/status`)
      .pipe(catchError(this.handleError));
  }

  public deleteAccount(): Observable<void> {
    return this.http
      .delete<void>(`${BASE_URL}/users`)
      .pipe(catchError(this.handleError));
  }

  private handleError(error: { error?: { message?: string }; message?: string }): Observable<never> {
    const message = error.error?.message || error.message || 'Unknown error';
    return throwError(() => new Error(message));
  }
}
