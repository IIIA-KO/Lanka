import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AgentService } from '../api/agent';
import { AuthService } from '../services/auth/auth.service';
import { InstagramStatusService } from '../services/instagram-status.service';
import { catchError, map, of } from 'rxjs';

export const instagramLinkedGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const api = inject(AgentService);
  const router = inject(Router);
  const instagramStatusService = inject(InstagramStatusService);

  if (!authService.isAuthenticated()) {
    router.navigate(['/auth/login'], {
      queryParams: { returnUrl: state.url },
    });
    return false;
  }

  // Check in-memory cache first to avoid redundant HTTP calls
  const cachedStatus = instagramStatusService.currentLinkingStatus;
  if (cachedStatus?.status === 'completed') {
    return true;
  }

  return api.Users.getLinkInstagramStatus().pipe(
    map((statusResponse) => {
      // Update the in-memory cache so subsequent guard checks don't need HTTP calls
      instagramStatusService.setManualStatus(
        'linking',
        statusResponse?.status === 'completed' ? 'completed' : 'not_found'
      );

      if (statusResponse?.status === 'completed') {
        return true;
      }

      router.navigate(['/link-instagram'], {
        queryParams: { returnUrl: state.url },
      });
      return false;
    }),
    catchError(() => {
      // On transient errors (429, 5xx), allow navigation rather than blocking.
      // The guard's purpose is to check "is Instagram linked?" — if we can't determine
      // that due to a server error, it's better to let the user through.
      return of(true);
    })
  );
};
