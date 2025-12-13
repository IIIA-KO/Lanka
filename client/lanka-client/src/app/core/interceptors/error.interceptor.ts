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

/**
 * Gets user-friendly error message based on HTTP status
 */
function getUserFriendlyErrorMessage(err: HttpErrorResponse): string {
  switch (err.status) {
    case 400:
      return 'Invalid request. Please check your input and try again.';
    case 401:
      return 'Please log in to continue.';
    case 403:
      return 'You don\'t have permission to perform this action.';
    case 404:
      return 'The requested resource was not found.';
    case 405:
      return 'This action is not allowed. Please contact support if this persists.';
    case 409:
      return 'A conflict occurred. Please refresh the page and try again.';
    case 422:
      return 'The provided data is invalid. Please check your input.';
    case 429:
      return 'Too many requests. Please wait a moment and try again.';
    case 500:
      return 'Server error occurred. Please try again later.';
    case 502:
      return 'Service temporarily unavailable. Please try again later.';
    case 503:
      return 'Service is currently under maintenance. Please try again later.';
    case 504:
      return 'Request timeout. Please try again.';
    default:
      if (err.status >= 500) {
        return 'Server is temporarily unavailable. Please try again later.';
      } else if (err.status === 0) {
        return 'No internet connection. Please check your connection and try again.';
      } else {
        return 'Something went wrong. Please try again.';
      }
  }
}

export const errorInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> => {
  const snackbar = inject(SnackbarService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      // Only show snackbars for server errors (5xx) and network errors
      if (err.status >= 500) {
        const errorMessage = getUserFriendlyErrorMessage(err);
        snackbar.showError(errorMessage);
      } else if (err.status === 0) {
        // Network error
        snackbar.showError('No internet connection. Please check your connection and try again.');
      }
      // All other errors (4xx) are handled by individual components

      return throwError(() => err);
    })
  );
};
