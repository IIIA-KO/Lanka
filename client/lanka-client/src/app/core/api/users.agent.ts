import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
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
import { IInstagramStatusResponse } from '../models/instagram';
import { FriendlyErrorService } from '../services/friendly-error.service';

const BASE_URL = environment.apiUrl;

@Injectable({
  providedIn: 'root',
})
export class UsersAgent {
  private readonly http = inject(HttpClient);
  private readonly friendlyErrorService = inject(FriendlyErrorService);

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

  public linkInstagram(code: string): Observable<HttpResponse<unknown>> {
    return this.http
      .post(`${BASE_URL}/users/link-instagram`, { code }, { observe: 'response' })
      .pipe(catchError(this.handleError));
  }

  public renewInstagramAccess(code: string): Observable<HttpResponse<unknown>> {
    return this.http
      .post(`${BASE_URL}/users/renew-instagram-access`, { code }, { observe: 'response' })
      .pipe(catchError(this.handleError));
  }

  public getLinkInstagramStatus(): Observable<IInstagramStatusResponse> {
    return this.http
      .get<IInstagramStatusResponse>(`${BASE_URL}/users/link-instagram/status`)
      .pipe(catchError(this.handleError));
  }

  public getRenewInstagramStatus(): Observable<IInstagramStatusResponse> {
    return this.http
      .get<IInstagramStatusResponse>(
        `${BASE_URL}/users/renew-instagram-access/status`
      )
      .pipe(catchError(this.handleError));
  }

  public deleteAccount(): Observable<void> {
    return this.http
      .delete<void>(`${BASE_URL}/users`)
      .pipe(catchError(this.handleError));
  }

  private readonly handleError = (error: unknown): Observable<never> => {
    const friendlyError = this.friendlyErrorService.toFriendlyError(error, {
      badRequestMessage: 'We could not process your request. Please check the entered data.',
      unauthorizedMessage: 'Your session has expired. Please sign in again.',
      notFoundMessage: 'We could not find the requested information.',
      networkMessage: 'Connection lost. Please check your internet connection.',
      fallbackMessage: 'Something went wrong. Please try again in a moment.'
    });

    return throwError(() => friendlyError);
  };
}
