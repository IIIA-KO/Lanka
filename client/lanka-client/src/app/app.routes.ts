import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { CampaignsComponent } from './features/campaigns/campaigns.component';
import { SearchComponent } from './features/search/search.component';
import { ProfileComponent } from './features//profile/profile.component';
import { LoginComponent } from './features/users/login/login.component';
import { RegisterComponent } from './features/users/register/register.component';

import { authGuard } from './core/guards/auth.guard';
import { AuthLayoutComponent } from './core/layouts/auth.layout.component';
import { PrivacyPolicyComponent } from './pages/privacy-policy/privacy-policy.component';
import { ServerErrorComponent } from './shared/components/server-error/server-error.component';
import { unauthGuard } from './core/guards/unauth.guard';
import { LayoutComponent } from './core/components/layout/layout.component';
import { LogoutComponent } from './features/users/logout/logout.component';

export const routes: Routes = [
  { path: 'privacy-policy', component: PrivacyPolicyComponent },
  {
    path: 'auth',
    component: AuthLayoutComponent,
    children: [
      { path: 'login', component: LoginComponent, canActivate: [unauthGuard] },
      {
        path: 'register',
        component: RegisterComponent,
        canActivate: [unauthGuard],
      },
      { path: '', redirectTo: 'login', pathMatch: 'full' },
    ],
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'logout', component: LogoutComponent, canActivate: [authGuard] },
      {
        path: 'dashboard',
        component: DashboardComponent,
        canActivate: [authGuard],
      },
      {
        path: 'campaigns',
        component: CampaignsComponent,
        canActivate: [authGuard],
      },
      { path: 'search', component: SearchComponent, canActivate: [authGuard] },
      {
        path: 'profile',
        component: ProfileComponent,
        canActivate: [authGuard],
      },
      { path: 'server-error', component: ServerErrorComponent },
      { path: '**', redirectTo: 'dashboard' },
    ],
  }
];
