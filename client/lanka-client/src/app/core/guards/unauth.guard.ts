import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth/auth.service';

export const unauthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isTokenExpired()) {
    console.log('UNAUTH GUARD: User is not authenticated');
    return true;
  } else {
    router.navigate(['/dashboard']);
    return false;
  }
};
