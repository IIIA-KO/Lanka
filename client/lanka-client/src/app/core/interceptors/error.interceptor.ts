import { inject } from '@angular/core';
import {
  HttpErrorResponse,
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpEvent,
} from '@angular/common/http';
import { throwError, Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { SnackbarService } from '../services/snackbar/snackbar.service';
import { Router, NavigationExtras } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
): Observable<HttpEvent<any>> => {
  const router = inject(Router);
  const snackbar = inject(SnackbarService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 400) {
        const errors = err.error?.errors;

        if (Array.isArray(errors)) {
          const descriptions = errors.map(e => e.description).filter(Boolean);
          const message = descriptions.join('\n');
          snackbar.showError(message || 'Validation error');
        } else {
          snackbar.showError(err.error?.title || 'Bad request');
        }
      }

      if (err.status === 401) {
        snackbar.showError(err.error?.title || 'Unauthorized');
      }

      if (err.status === 403) {
        snackbar.showError('Forbidden');
      }

      if (err.status === 404) {
        snackbar.showError(err.error.detail || 'Not found');
      }

      if (err.status >= 500) {
        const navigationExtras: NavigationExtras = {
          state: { error: err.error },
        };
        router.navigateByUrl('/server-error', navigationExtras);
      }

      return throwError(() => err);
    })
  );
};
