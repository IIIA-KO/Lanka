import { Injectable, inject } from '@angular/core';
import { ITokenResponse, IRefreshTokenRequest } from '../../models/auth';
import { BehaviorSubject, Observable, Subscription, throwError, timer } from 'rxjs';
import { UsersAgent } from '../../api/users.agent';
import { Router } from '@angular/router';
import { SignalRService } from '../signalr.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly TOKEN_EXPIRY_KEY = 'token_expiry';

  private readonly MIN_BUFFER_TIME = 1 * 60 * 1000;
  private readonly MAX_BUFFER_TIME = 2 * 60 * 1000;
  private readonly BUFFER_RATIO = 1 / 3;

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(
    this.hasValidToken()
  );

  private refreshTimer: Subscription | null = null;

  private readonly usersAgent = inject(UsersAgent);
  private readonly router = inject(Router);
  private readonly signalRService = inject(SignalRService);

  constructor() {
    if (this.isAuthenticated()) {
      this.startTokenRefreshTimer();
      this.startSignalRConnection();
    }
  }

  public get isAuthenticated$(): Observable<boolean> {
    return this.isAuthenticatedSubject.asObservable();
  }

  public login(tokenResponse: ITokenResponse): void {
    this.storeTokens(tokenResponse);
    this.isAuthenticatedSubject.next(true);
    this.startTokenRefreshTimer();
    this.startSignalRConnection();
  }

  public logout(): void {
    this.clearTokens();
    this.stopTokenRefreshTimer();
    this.stopSignalRConnection();
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/auth/login']);
  }

  public isAuthenticated(): boolean {
    return this.hasValidToken();
  }

  public getToken(): string | null {
    const token = localStorage.getItem(this.ACCESS_TOKEN_KEY);
    if (!token) {
      return null;
    }

    // Check both our stored expiry and the actual JWT expiry
    if (this.isTokenExpired() || this.isJwtTokenExpired(token)) {
      console.warn('[AuthService] Token expired, clearing tokens');
      this.clearTokens();
      return null;
    }

    return token;
  }

  public getUserIdFromToken(token?: string): string | null {
    const tokenToUse = token || this.getToken();
    if (!tokenToUse) {
      return null;
    }

    try {
      const payload = this.decodeToken(tokenToUse);
      return payload?.sub || null;
    } catch {
      console.error('[AuthService] Error extracting user ID from token');
      return null;
    }
  }

  public getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  public isTokenExpired(): boolean {
    const expiry = localStorage.getItem(this.TOKEN_EXPIRY_KEY);
    if (!expiry) return true;

    const expiryTime = parseInt(expiry, 10);
    const currentTime = Date.now();

    const bufferTime = this.MAX_BUFFER_TIME;
    return currentTime + bufferTime >= expiryTime;
  }

  public refreshToken(): Observable<ITokenResponse> {
    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      this.logout();
      return throwError(() => new Error('No refresh token available'));
    }

    const refreshRequest: IRefreshTokenRequest = {
      refreshToken: refreshToken,
    };

    return new Observable((observer) => {
      this.usersAgent.refreshToken(refreshRequest).subscribe({
        next: (tokenResponse: ITokenResponse) => {
          this.storeTokens(tokenResponse);
          this.isAuthenticatedSubject.next(true);
          this.startTokenRefreshTimer();
          observer.next(tokenResponse);
          observer.complete();
        },
        error: (error) => {
          console.error('[AuthService] Token refresh failed:', error);
          this.logout();
          observer.error(error);
        },
      });
    });
  }

  public clearTokens(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.TOKEN_EXPIRY_KEY);
  }

  private isJwtTokenExpired(token: string): boolean {
    try {
      const payload = this.decodeToken(token);
      if (!payload?.exp) return true;
      
      // Add a small buffer (30 seconds) to prevent race conditions
      const bufferTime = 30 * 1000;
      return Date.now() + bufferTime >= payload.exp * 1000;
    } catch {
      console.error('[AuthService] Error checking JWT expiration');
      return true;
    }
  }

  private decodeToken(token: string): { sub?: string; exp?: number } | null {
    if (!token) {
      return null;
    }

    const parts = token.split('.');
    if (parts.length !== 3) {
      throw new Error('Invalid JWT token format');
    }

    try {
      const payload = parts[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded);
    } catch {
      throw new Error('Failed to decode JWT token');
    }
  }

  private hasValidToken(): boolean {
    const token = localStorage.getItem(this.ACCESS_TOKEN_KEY);
    return token !== null && !this.isTokenExpired();
  }

  private storeTokens(tokenResponse: ITokenResponse): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, tokenResponse.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, tokenResponse.refreshToken);

    // Calculate and store expiry time
    const expiryTime = Date.now() + tokenResponse.expiresIn * 1000;
    localStorage.setItem(this.TOKEN_EXPIRY_KEY, expiryTime.toString());
  }

  private calculateBufferTime(remainingTime: number): number {
    const oneThirdTime = remainingTime * this.BUFFER_RATIO;

    return Math.max(
      this.MIN_BUFFER_TIME,
      Math.min(oneThirdTime, this.MAX_BUFFER_TIME)
    );
  }

  private startTokenRefreshTimer(): void {
    this.stopTokenRefreshTimer();

    const expiry = localStorage.getItem(this.TOKEN_EXPIRY_KEY);
    if (!expiry) return;

    const expiryTime = parseInt(expiry, 10);
    const currentTime = Date.now();
    const remainingTime = expiryTime - currentTime;

    if (remainingTime <= 0) {
      console.warn('[AuthService] Token has already expired, refreshing immediately');
      this.refreshToken().subscribe();
      return;
    }

    const bufferTime = this.calculateBufferTime(remainingTime);

    const refreshTime = remainingTime - bufferTime;

    if (refreshTime > 0) {
      console.warn(
        `[AuthService] Token will be refreshed in ${Math.round(
          refreshTime / 1000 / 60
        )} minutes`
      );

      this.refreshTimer = timer(refreshTime).subscribe(() => {
        console.warn('[AuthService] Automatically refreshing token...');
        this.refreshToken().subscribe({
          next: () => console.warn('[AuthService] Token refreshed successfully'),
          error: (error) => {
            console.error('[AuthService] Automatic token refresh failed:', error);
            this.logout();
          },
        });
      });
    } else {
      console.warn('[AuthService] Token is about to expire, refreshing now...');
      this.refreshToken().subscribe();
    }
  }

  private stopTokenRefreshTimer(): void {
    if (this.refreshTimer) {
      this.refreshTimer.unsubscribe();
      this.refreshTimer = null;
    }
  }

  private async startSignalRConnection(): Promise<void> {
    try {
      const token = this.getToken();
      if (token) {
        await this.signalRService.startConnection(token);
        console.warn('[AuthService] SignalR connection started');
      }
    } catch (error) {
      console.error('[AuthService] Failed to start SignalR connection:', error);
    }
  }

  private async stopSignalRConnection(): Promise<void> {
    try {
      await this.signalRService.stopConnection();
      console.warn('[AuthService] SignalR connection stopped');
    } catch (error) {
      console.error('[AuthService] Failed to stop SignalR connection:', error);
    }
  }
}
