import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth/auth.service';

export const unauthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    console.warn(
      '[UnauthGuard] User is authenticated, redirecting to pact'
    );
    router.navigate(['/pact']);
    return false;
  }

  if (authService.getRefreshToken()) {
    console.warn('[UnauthGuard] User has refresh token, checking validity...');

    authService.refreshToken().subscribe({
      next: () => {
        console.warn('[UnauthGuard] Token refreshed, redirecting to pact');
        router.navigate(['/pact']);
      },
      error: () => {
        console.warn(
          '[UnauthGuard] Token refresh failed, allowing access to auth pages'
        );
      },
    });
  }

  console.warn('[UnauthGuard] User is not authenticated, allowing access');
  return true;
};
