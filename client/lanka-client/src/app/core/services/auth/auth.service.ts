import { Injectable } from '@angular/core';
import { ITokenResponse } from '../../models/auth';
import { BehaviorSubject, Observable, Subscription, timer } from 'rxjs';
import { TokenRefreshService } from '../refresh/refresh.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'auth';
  private refreshTimerSub?: Subscription;

  private _isAuthenticated$ = new BehaviorSubject<boolean>(this.checkAuth());
  public readonly isAuthenticated$ = this._isAuthenticated$.asObservable();


  constructor(private tokenRefreshService: TokenRefreshService) {}

  login(authData: ITokenResponse) {
    const expiresAt = Date.now() + authData.expiresIn * 1000;

    sessionStorage.setItem(
      this.storageKey,
      JSON.stringify({ ...authData, expiresAt })
    );

    this.scheduleRefresh(authData.expiresIn);

    this._isAuthenticated$.next(true);
  }

  private scheduleRefresh(expiresInSeconds: number) {
    this.refreshTimerSub?.unsubscribe();

    const refreshDelay = (expiresInSeconds - 30) * 1000;
    this.refreshTimerSub = timer(refreshDelay).subscribe(() => {
      this.tokenRefreshService.refreshToken().subscribe({
        next: () => {
          const data = this.getAuthData();
          if (data) {
            this.scheduleRefresh(data.expiresIn);
          }
        },
        error: () => {
          this.clear();
        },
      });
    });
  }

  clear() {
    sessionStorage.removeItem(this.storageKey);
    this._isAuthenticated$.next(true);
    this.refreshTimerSub?.unsubscribe();
  }

  private checkAuth(): boolean {
    const data = this.getAuthData();
    return data ? Date.now() < data.expiresAt : false;
  }

  private getAuthData(): any {
    const json = sessionStorage.getItem(this.storageKey);
    return json ? JSON.parse(json) : null;
  }

  getToken(): string | null {
    const data = this.getAuthData();
    return data?.accessToken ?? null;
  }

  isTokenExpired(): boolean {
    const data = this.getAuthData();
    if (!data) {
      return true;
    }

    if (Date.now() > data.expiresAt) {
      return true;
    }

    return false;
  }

  isAuthenticated(): boolean {
    return !this.isTokenExpired();
  }

  getRefreshToken(): string | null {
    const data = this.getAuthData();
    return data?.refreshToken ?? null;
  }
}
