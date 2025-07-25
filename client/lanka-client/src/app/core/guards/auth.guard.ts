import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth/auth.service';
import { catchError, map, of } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  if (!authService.isAuthenticated() && authService.getRefreshToken()) {
    return authService.refreshToken().pipe(
      map(() => {
        return true;
      }),
      catchError(() => {
        router.navigate(['/auth/login'], {
          queryParams: { returnUrl: state.url },
        });
        return of(false);
      })
    );
  }

  router.navigate(['/auth/login'], {
    queryParams: { returnUrl: state.url },
  });
  return false;
};
