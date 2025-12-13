import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

export interface FriendlyErrorOptions {
  fallbackMessage?: string;
  notFoundMessage?: string;
  badRequestMessage?: string;
  unauthorizedMessage?: string;
  forbiddenMessage?: string;
  conflictMessage?: string;
  validationMessage?: string;
  networkMessage?: string;
  serverErrorMessage?: string;
  rateLimitMessage?: string;
}

export class FriendlyHttpError extends Error {
  constructor(
    message: string,
    public readonly status?: number,
    public readonly originalError?: unknown
  ) {
    super(message);
    this.name = 'FriendlyHttpError';
  }

  public get isClientError(): boolean {
    return typeof this.status === 'number' && this.status >= 400 && this.status < 500;
  }

  public get isServerError(): boolean {
    return typeof this.status === 'number' && this.status >= 500;
  }
}

@Injectable({ providedIn: 'root' })
export class FriendlyErrorService {
  public toFriendlyError(
    error: unknown,
    options?: FriendlyErrorOptions
  ): FriendlyHttpError {
    if (error instanceof FriendlyHttpError) {
      return error;
    }

    if (error instanceof HttpErrorResponse) {
      const status = error.status;
      const override = this.resolveOverride(status, options);
      const backendMessage = this.extractBackendMessage(error);
      const message = override ?? backendMessage ?? this.defaultMessage(status, options);

      return new FriendlyHttpError(message, status, error);
    }

    const fallback = options?.fallbackMessage ?? 'Something went wrong. Please try again.';
    return new FriendlyHttpError(fallback, undefined, error);
  }

  private resolveOverride(
    status: number,
    options?: FriendlyErrorOptions
  ): string | undefined {
    switch (status) {
      case 0:
        return options?.networkMessage;
      case 400:
        return options?.badRequestMessage ?? options?.validationMessage;
      case 401:
        return options?.unauthorizedMessage;
      case 403:
        return options?.forbiddenMessage;
      case 404:
        return options?.notFoundMessage;
      case 409:
        return options?.conflictMessage;
      case 422:
        return options?.validationMessage;
      case 429:
        return options?.rateLimitMessage;
      default:
        if (status >= 500) {
          return options?.serverErrorMessage;
        }
        return undefined;
    }
  }

  private extractBackendMessage(error: HttpErrorResponse): string | undefined {
    const payload = error.error;

    if (!payload || typeof payload !== 'object') {
      return undefined;
    }

    const candidateKeys = ['message', 'detail', 'title', 'error'];
    for (const key of candidateKeys) {
      const value = (payload as Record<string, unknown>)[key];
      if (typeof value === 'string' && value.trim().length > 0) {
        return value;
      }
    }

    return undefined;
  }

  private defaultMessage(status: number, options?: FriendlyErrorOptions): string {
    if (status === 0) {
      return (
        options?.networkMessage ??
        'Unable to reach the server. Check your connection and try again.'
      );
    }

    if (status >= 500) {
      return (
        options?.serverErrorMessage ??
        'Something broke on our side. Please try again later.'
      );
    }

    switch (status) {
      case 400:
      case 422:
        return (
          options?.badRequestMessage ??
          options?.validationMessage ??
          'Some of the provided data is invalid. Please review it and try again.'
        );
      case 401:
        return options?.unauthorizedMessage ?? 'You need to sign in to continue.';
      case 403:
        return options?.forbiddenMessage ?? 'You don\'t have permission to perform this action.';
      case 404:
        return options?.notFoundMessage ?? 'We could not find what you were looking for.';
      case 409:
        return options?.conflictMessage ?? 'This action conflicts with the current data state.';
      case 429:
        return options?.rateLimitMessage ?? 'Too many requests. Please wait and try again.';
      default:
        return options?.fallbackMessage ?? 'Something went wrong. Please try again.';
    }
  }
}

