import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';

import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';

import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';

import { provideTranslate } from './core/providers/translate.provider';

import { definePreset } from '@primeng/themes';

const MyPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#f0f9f9',
      100: '#dcf2f2',
      200: '#bfe5e5',
      300: '#94d2d2',
      400: '#4ab8b8',
      500: '#2d9d9d',
      600: '#247f7f',
      700: '#206666',
      800: '#1e5252',
      900: '#1c4444',
      950: '#0f2929'
    }
  }
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withInterceptors([loadingInterceptor, authInterceptor, errorInterceptor])
    ),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: MyPreset,
        options: {
          darkModeSelector: false,
        },
      },
    }),
    provideTranslate(),
  ],
};
