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
    authReq = addAuthHeader(req, token);
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && token && !isAuthRequest(req.url)) {
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

    return authService.refreshToken().pipe(
      switchMap((tokenResponse: any) => {
        isRefreshing = false;
        refreshTokenSubject.next(tokenResponse.accessToken);

        // Retry the original request with the new token
        return next(addAuthHeader(request, tokenResponse.accessToken));
      }),
      catchError((error) => {
        isRefreshing = false;
        authService.logout();
        return throwError(() => error);
      })
    );
  } else {
    // Wait for the refresh to complete, then retry with the new token
    return refreshTokenSubject.pipe(
      filter((token) => token !== null),
      take(1),
      switchMap((token) => next(addAuthHeader(request, token)))
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
    url.includes('/users/login')
    || url.includes('/users/register')
    || url.includes('/users/refresh-token')
  );
}
