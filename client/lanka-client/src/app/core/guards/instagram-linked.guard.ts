import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AgentService } from '../api/agent';
import { AuthService } from '../services/auth/auth.service';
import { catchError, map, of } from 'rxjs';

export const instagramLinkedGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const api = inject(AgentService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/auth/login'], {
      queryParams: { returnUrl: state.url },
    });
    return false;
  }

  return api.Bloggers.getProfile().pipe(
    map((profile) => {
      console.log('Instagram account is linked:', profile.instagramUsername);
      if (profile && profile.instagramUsername) {
        return true;
      } else {
        router.navigate(['/link-instagram'], {
          queryParams: { returnUrl: state.url },
        });
        return false;
      }
    }),
    catchError(() => {
      router.navigate(['/link-instagram'], {
        queryParams: { returnUrl: state.url },
      });
      return of(false);
    })
  );
};
