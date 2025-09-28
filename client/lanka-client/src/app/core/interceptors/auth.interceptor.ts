import { HttpHandlerFn, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth/auth.service';
import {
  HttpEvent,
  HttpRequest,
  HttpErrorResponse,
} from '@angular/common/http';
import {
  catchError,
  Observable,
  switchMap,
  throwError,
  BehaviorSubject,
  filter,
  take,
} from 'rxjs';
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
): Observable<HttpEvent<any>> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (isAuthRequest(req.url)) {
    console.log('Skipping auth interceptor for request:', req.url);
    return next(req);
  }

  const token = authService.getToken();
  let authReq = req;

  if (token) {
    console.log('Adding Authorization header to request:', req.url);
    authReq = addAuthHeader(req, token);
  } else {
    console.warn('No valid token available for request:', req.url);
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isAuthRequest(req.url)) {
        console.log('401 error, attempting token refresh for:', req.url);
        return handle401Error(req, next, authService);
      }
      return throwError(() => error);
    })
  );
};

let isRefreshing = false;
let refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

function handle401Error(
  request: HttpRequest<any>,
  next: HttpHandlerFn,
  authService: AuthService
): Observable<HttpEvent<any>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    console.log('Starting token refresh process');
    return authService.refreshToken().pipe(
      switchMap((tokenResponse: any) => {
        isRefreshing = false;
        refreshTokenSubject.next(tokenResponse.accessToken);
        console.log('Token refreshed successfully, retrying request');

        // Retry the original request with the new token
        return next(addAuthHeader(request, tokenResponse.accessToken));
      }),
      catchError((error) => {
        console.error('Token refresh failed:', error);
        isRefreshing = false;
        refreshTokenSubject.next(null);
        authService.logout();
        return throwError(() => error);
      })
    );
  } else {
    // Wait for the refresh to complete, then retry with the new token
    console.log('Waiting for ongoing token refresh');
    return refreshTokenSubject.pipe(
      filter((token) => token !== null),
      take(1),
      switchMap((token) => {
        console.log('Using refreshed token for queued request');
        return next(addAuthHeader(request, token));
      })
    );
  }
}

function addAuthHeader(
  request: HttpRequest<any>,
  token: string
): HttpRequest<any> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });
}

function isAuthRequest(url: string): boolean {
  console.log('Checking if request is for auth endpoint:', url);
  return (
    url.includes('/users/login') ||
    url.includes('/users/register') ||
    url.includes('/users/refresh-token') ||
    url.includes('/auth/') // Cover any other auth endpoints
  );
}
