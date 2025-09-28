import { Routes } from '@angular/router';
import { PactComponent } from './features/bloggers/pact/pact.component';
import { CampaignsComponent } from './features/bloggers/campaigns/campaigns.component';
import { SearchComponent } from './features/bloggers/search/search.component';
import { ProfileComponent } from './features/bloggers/profile/profile.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { EditProfileComponent } from './features/bloggers/profile/edit-profile/edit-profile.component';

import { AuthLayoutComponent } from './core/layouts/auth.layout.component';
import { PrivacyPolicyComponent } from './pages/privacy-policy/privacy-policy.component';
import { ServerErrorComponent } from './shared/components/server-error/server-error.component';
import { LayoutComponent } from './core/components/layout/layout.component';
import { LogoutComponent } from './features/auth/logout/logout.component';
import { ProfileResolver } from './core/api/profile.resolver';
import { LinkInstagramComponent } from './pages/link-instagram/link-instagram.component';
import { LinkInstagramCallbackComponent } from './pages/link-instagram/link-instagram-callback/link-instagram-callback.component';
import { RenewInstagramCallbackComponent } from './pages/link-instagram/renew-instagram-callback/renew-instagram-callback.component';

import { authGuard } from './core/guards/auth.guard';
import { unauthGuard } from './core/guards/unauth.guard';
import { instagramLinkedGuard } from './core/guards/instagram-linked.guard';

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
      { path: '', redirectTo: 'pact', pathMatch: 'full' },
      { path: 'logout', component: LogoutComponent, canActivate: [authGuard] },
      {
        path: 'link-instagram',
        component: LinkInstagramComponent,
        canActivate: [authGuard],
      },
      {
        path: 'link-instagram/callback',
        component: LinkInstagramCallbackComponent,
        canActivate: [authGuard],
      },
      {
        path: 'renew-instagram/callback',
        component: RenewInstagramCallbackComponent,
        canActivate: [authGuard],
      },
      {
        path: 'pact',
        component: PactComponent,
        canActivate: [instagramLinkedGuard],
      },
      {
        path: 'campaigns',
        component: CampaignsComponent,
        canActivate: [instagramLinkedGuard],
      },
      { path: 'search', component: SearchComponent, canActivate: [instagramLinkedGuard] },
      {
        path: 'profile',
        component: ProfileComponent,
        canActivate: [instagramLinkedGuard],
        resolve: { profile: ProfileResolver },
      },
      {
        path: 'profile/edit',
        component: EditProfileComponent,
        canActivate: [instagramLinkedGuard],
        resolve: { profile: ProfileResolver },
      },
      { path: 'server-error', component: ServerErrorComponent },
      {
        path: 'offers',
        loadChildren: () =>
          import('./features/bloggers/offers/offers.module').then(
            (m) => m.OffersModule
          ),
        canActivate: [instagramLinkedGuard],
      },
      {
        path: 'reviews',
        loadChildren: () =>
          import('./features/bloggers/reviews/reviews.module').then(
            (m) => m.ReviewsModule
          ),
        canActivate: [instagramLinkedGuard],
      },
      {
        path: 'analytics',
        loadChildren: () =>
          import('./features/analytics/analytics.module').then(
            (m) => m.AnalyticsModule
          ),
        canActivate: [instagramLinkedGuard],
      },
      { path: '**', redirectTo: 'pact' },
    ],
  },
];
