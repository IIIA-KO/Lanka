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

/**
 * Determines if error display should be skipped for certain endpoints
 */
function shouldSkipErrorDisplay(url: string, status: number): boolean {
  // Skip 404 errors for pact endpoint - components handle this themselves
  if (url.includes('/pact') && status === 404) {
    return true;
  }
  
  // Skip 404 errors for optional profile data
  if (url.includes('/profile') && status === 404) {
    return true;
  }
  
  return false;
}

/**
 * Determines if a resource is optional (404 should not show error)
 */
function isOptionalResource(url: string): boolean {
  const optionalEndpoints = ['/pact', '/profile', '/avatar'];
  return optionalEndpoints.some(endpoint => url.includes(endpoint));
}

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
  req: HttpRequest<any>,
  next: HttpHandlerFn
): Observable<HttpEvent<any>> => {
  const router = inject(Router);
  const snackbar = inject(SnackbarService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      // Don't show errors for certain endpoints that handle their own error display
      if (shouldSkipErrorDisplay(req.url, err.status)) {
        return throwError(() => err);
      }

      const errorMessage = getUserFriendlyErrorMessage(err);
      
      if (err.status === 400) {
        const errors = err.error?.errors;

        if (Array.isArray(errors)) {
          const descriptions = errors.map(e => e.description).filter(Boolean);
          const message = descriptions.join('\n');
          snackbar.showError(message || errorMessage);
        } else {
          snackbar.showError(err.error?.title || errorMessage);
        }
      } else if (err.status === 401) {
        snackbar.showError(err.error?.title || errorMessage);
      } else if (err.status === 403) {
        snackbar.showError(errorMessage);
      } else if (err.status === 404) {
        // Only show 404 errors for non-optional resources
        if (!isOptionalResource(req.url)) {
          snackbar.showError(err.error?.detail || errorMessage);
        }
      } else if (err.status === 409) {
        snackbar.showError(err.error?.detail || errorMessage);
      } else if (err.status === 422) {
        snackbar.showError(err.error?.detail || errorMessage);
      } else if (err.status >= 500) {
        snackbar.showError(errorMessage);
        // Optionally navigate to error page for severe errors
        // const navigationExtras: NavigationExtras = {
        //   state: { error: err.error },
        // };
        // router.navigateByUrl('/server-error', navigationExtras);
      } else if (err.status === 0) {
        // Network error
        snackbar.showError('No internet connection. Please check your connection and try again.');
      } else {
        snackbar.showError(errorMessage);
      }

      return throwError(() => err);
    })
  );
};
