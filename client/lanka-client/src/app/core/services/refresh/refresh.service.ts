import { Injectable, Injector } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { AgentService } from '../../api/agent';
import { throwError, catchError, Subject, Observable, tap, map } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TokenRefreshService {
  private refreshInProgress = false;
  private refreshSubject = new Subject<void>();

  constructor(private injector: Injector, private agent: AgentService) {}

  private get auth(): AuthService {
    return this.injector.get(AuthService);
  }

  refreshToken(): Observable<void> {
    if (this.refreshInProgress) {
      return this.refreshSubject.asObservable();
    }
    this.refreshInProgress = true;

    const refreshToken = this.auth.getRefreshToken();
    if (!refreshToken) {
      this.auth.clearTokens();
      return throwError(() => new Error('No refresh token'));
    }

    const refreshTokenRequest = {
      refreshToken,
    };

    return this.agent.Users.refreshToken(refreshTokenRequest).pipe(
      tap((res) => {
        this.auth.login(res);
        this.refreshInProgress = false;
        this.refreshSubject.next();
        this.refreshSubject.complete();
        this.refreshSubject = new Subject<void>();
      }),
      catchError((error) => {
        this.auth.clearTokens();
        this.refreshInProgress = false;
        return throwError(() => error);
      }),
      map(() => void 0)
    );
  }
}
