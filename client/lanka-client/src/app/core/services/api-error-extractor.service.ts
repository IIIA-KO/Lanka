import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FriendlyHttpError } from './friendly-error.service';

@Injectable({
  providedIn: 'root'
})
export class ApiErrorExtractorService {

  public extractErrorMessage(error: unknown): string {
    let httpError: HttpErrorResponse | null = null;

    if (error instanceof FriendlyHttpError && error.originalError instanceof HttpErrorResponse) {
      httpError = error.originalError;
    } else if (error instanceof HttpErrorResponse) {
      httpError = error;
    }

    // If it's not an HttpErrorResponse (or wrapped one), handle basic JS errors
    if (!httpError) {
        if (error instanceof Error) return error.message;
        if (typeof error === 'string') return error;
        return 'ERRORS.UNEXPECTED';
    }

    // Now handle the HttpErrorResponse body
    const responseBody = httpError.error;

    if (!responseBody) {
      // Fallback based on status code
      return this.getFallbackMessage(httpError.status);
    }

    // 1. String body
    if (typeof responseBody === 'string') {
      return responseBody;
    }

    // 2. Object body
    const errObj = responseBody as {
      errors?: Record<string, string[] | string> | unknown[];
      detail?: string;
      title?: string;
      message?: string;
    };

    // Handle 'errors' property (Identity / ValidationProblemDetails)
    if (errObj.errors) {
      // Case A: Array of errors (e.g. Identity)
      if (Array.isArray(errObj.errors)) {
        return errObj.errors.map((e: unknown) => {
          if (typeof e === 'string') return e;
          const eObj = e as { description?: string; message?: string };
          return eObj.description || eObj.message || JSON.stringify(e);
        }).join('\n');
      }

      // Case B: Map of errors (ValidationProblemDetails)
      if (typeof errObj.errors === 'object') {
        const messages: string[] = [];
        const errorsRecord = errObj.errors as Record<string, string[] | string>;
        
        Object.keys(errorsRecord).forEach(key => {
          const value = errorsRecord[key];
          if (Array.isArray(value)) {
            messages.push(...value);
          } else if (typeof value === 'string') {
            messages.push(value);
          }
        });

        if (messages.length > 0) return messages.join('\n');
      }
    }

    // Handle direct array body (e.g. ["Error 1", "Error 2"])
    if (Array.isArray(responseBody)) {
       return (responseBody as unknown[]).map((e: unknown) => {
          if (typeof e === 'string') return e;
          const eObj = e as { description?: string; message?: string };
          return eObj.description || eObj.message || JSON.stringify(e);
       }).join('\n');
    }

    // Handle standard properties
    if (errObj.detail) return errObj.detail;
    if (errObj.title) return errObj.title;
    if (errObj.message) return errObj.message;

    // Fallback
    return this.getFallbackMessage(httpError.status);
  }

  private getFallbackMessage(status: number): string {
    switch (status) {
      case 400: return 'REGISTER.ERRORS.VALIDATION_FAILED';
      case 401: return 'LOGIN.ERRORS.INVALID_CREDENTIALS';
      case 403: return 'ERRORS.FORBIDDEN';
      case 404: return 'ERRORS.NOT_FOUND';
      case 409: return 'REGISTER.ERRORS.EMAIL_EXISTS';
      case 500: return 'ERRORS.SERVER';
      default: return 'REGISTER.ERRORS.GENERIC';
    }
  }
}
