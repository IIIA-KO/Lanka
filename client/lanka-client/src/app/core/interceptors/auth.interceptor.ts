import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth/auth.service';
import {
  BehaviorSubject,
  Observable,
  catchError,
  filter,
  switchMap,
  take,
  throwError,
} from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> => {
  const authService = inject(AuthService);

  if (isAuthRequest(req.url)) {
    console.warn('[AuthInterceptor] Skipping auth interceptor for request:', req.url);
    return next(req);
  }

  const token = authService.getToken();
  let authReq = req;

  if (token) {
    console.warn('[AuthInterceptor] Adding Authorization header to request:', req.url);
    authReq = addAuthHeader(req, token);
  } else {
    console.warn('[AuthInterceptor] No valid token available for request:', req.url);
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isAuthRequest(req.url)) {
        console.warn('[AuthInterceptor] 401 error, attempting token refresh for:', req.url);
        return handle401Error(req, next, authService);
      }
      return throwError(() => error);
    })
  );
};

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

function handle401Error(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService
): Observable<HttpEvent<unknown>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    console.warn('[AuthInterceptor] Starting token refresh process');
    return authService.refreshToken().pipe(
      switchMap((tokenResponse: { accessToken: string }) => {
        isRefreshing = false;
        refreshTokenSubject.next(tokenResponse.accessToken);
        console.warn('[AuthInterceptor] Token refreshed successfully, retrying request');

        // Retry the original request with the new token
        return next(addAuthHeader(request, tokenResponse.accessToken));
      }),
      catchError((error) => {
        console.error('[AuthInterceptor] Token refresh failed:', error);
        isRefreshing = false;
        refreshTokenSubject.next(null);
        authService.logout();
        return throwError(() => error);
      })
    );
  } else {
    // Wait for the refresh to complete, then retry with the new token
    console.warn('[AuthInterceptor] Waiting for ongoing token refresh');
    return refreshTokenSubject.pipe(
      filter((token) => token !== null),
      take(1),
      switchMap((token) => {
        console.warn('[AuthInterceptor] Using refreshed token for queued request');
        return next(addAuthHeader(request, token));
      })
    );
  }
}

function addAuthHeader(
  request: HttpRequest<unknown>,
  token: string
): HttpRequest<unknown> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });
}

function isAuthRequest(url: string): boolean {
  console.warn('[AuthInterceptor] Checking if request is for auth endpoint:', url);
  return (
    url.includes('/users/login') ||
    url.includes('/users/register') ||
    url.includes('/users/refresh-token') ||
    url.includes('/auth/') // Cover any other auth endpoints
  );
}
