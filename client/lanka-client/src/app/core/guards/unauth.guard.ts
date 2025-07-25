import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth/auth.service';

export const unauthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    console.log(
      'UNAUTH GUARD: User is authenticated, redirecting to pact'
    );
    router.navigate(['/pact']);
    return false;
  }

  if (authService.getRefreshToken()) {
    console.log('UNAUTH GUARD: User has refresh token, checking validity...');

    authService.refreshToken().subscribe({
      next: () => {
        console.log('UNAUTH GUARD: Token refreshed, redirecting to pact');
        router.navigate(['/pact']);
      },
      error: () => {
        console.log(
          'UNAUTH GUARD: Token refresh failed, allowing access to auth pages'
        );
      },
    });
  }

  console.log('UNAUTH GUARD: User is not authenticated, allowing access');
  return true;
};
