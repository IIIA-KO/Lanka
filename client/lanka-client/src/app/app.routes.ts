import { Routes } from '@angular/router';
import { PactComponent } from './features/bloggers/pact/pact.component';
import { CampaignsComponent } from './features/bloggers/campaigns/campaigns.component';
import { SearchComponent } from './features/bloggers/search/search.component';
import { ProfileComponent } from './features/bloggers/profile/profile.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';

import { AuthLayoutComponent } from './core/layouts/auth-layout/auth-layout.component';
import { PrivacyPolicyComponent } from './features/privacy-policy/privacy-policy.component';
import { FaqComponent } from './features/faq/faq.component';
import { ServerErrorComponent } from './shared/components/server-error/server-error.component';
import { LayoutComponent } from './core/layouts/main-layout/layout.component';
import { LogoutComponent } from './features/auth/logout/logout.component';
import { ProfileResolver } from './core/api/profile.resolver';
import { LinkInstagramComponent } from './features/link-instagram/link-instagram.component';
import { LinkInstagramCallbackComponent } from './features/link-instagram/link-instagram-callback/link-instagram-callback.component';
import { RenewInstagramCallbackComponent } from './features/link-instagram/renew-instagram-callback/renew-instagram-callback.component';

import { authGuard } from './core/guards/auth.guard';
import { unauthGuard } from './core/guards/unauth.guard';
import { instagramLinkedGuard } from './core/guards/instagram-linked.guard';

export const routes: Routes = [
  { path: 'privacy-policy', component: PrivacyPolicyComponent, data: { titleKey: 'PAGE_TITLES.PRIVACY_POLICY' } },
  { path: 'faq', component: FaqComponent, data: { titleKey: 'PAGE_TITLES.FAQ' } },
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
      { path: '', redirectTo: 'profile', pathMatch: 'full' },
      { path: 'logout', component: LogoutComponent, canActivate: [authGuard] },
      {
        path: 'link-instagram',
        component: LinkInstagramComponent,
        canActivate: [authGuard],
        data: { titleKey: 'PAGE_TITLES.LINK_INSTAGRAM' }
      },
      {
        path: 'link-instagram/callback',
        component: LinkInstagramCallbackComponent,
        canActivate: [authGuard],
        data: { titleKey: 'PAGE_TITLES.LINK_INSTAGRAM' }
      },
      {
        path: 'renew-instagram/callback',
        component: RenewInstagramCallbackComponent,
        canActivate: [authGuard],
        data: { titleKey: 'PAGE_TITLES.LINK_INSTAGRAM' }
      },
      {
        path: 'pact',
        component: PactComponent,
        canActivate: [authGuard, instagramLinkedGuard],
        data: { titleKey: 'PAGE_TITLES.PACT' }
      },
      {
        path: 'campaigns',
        component: CampaignsComponent,
        canActivate: [authGuard, instagramLinkedGuard],
        data: { titleKey: 'PAGE_TITLES.CAMPAIGNS' }
      },
      {
        path: 'campaigns/:id',
        loadComponent: () =>
          import('./features/bloggers/campaigns/campaign-detail/campaign-detail.component').then(
            (m) => m.CampaignDetailComponent
          ),
        canActivate: [authGuard, instagramLinkedGuard],
        data: { titleKey: 'PAGE_TITLES.CAMPAIGN_DETAILS' }
      },
      {
        path: 'campaigns/create',
        loadComponent: () =>
          import('./features/brands/campaigns/create-campaign/create-campaign.component').then(
            (m) => m.CreateCampaignComponent
          ),
        canActivate: [authGuard, instagramLinkedGuard],
        data: { titleKey: 'PAGE_TITLES.CREATE_CAMPAIGN' }
      },
      {
        path: 'search',
        component: SearchComponent,
        canActivate: [authGuard, instagramLinkedGuard],
        data: { titleKey: 'PAGE_TITLES.SEARCH' }
      },
      {
        path: 'profile',
        component: ProfileComponent,
        canActivate: [authGuard, instagramLinkedGuard],
        resolve: { profile: ProfileResolver },
        data: { titleKey: 'PAGE_TITLES.PROFILE' }
      },
      {
        path: 'profile/edit',
        redirectTo: 'settings/profile',
        pathMatch: 'full' 
      },
      {
        path: 'settings/profile',
        loadComponent: () => import('./features/settings/profile-settings/profile-settings.component').then(m => m.ProfileSettingsComponent),
        canActivate: [authGuard, instagramLinkedGuard],
        resolve: { profile: ProfileResolver },
        data: { titleKey: 'PAGE_TITLES.SETTINGS' }
      },
      {
        path: 'calendar',
        loadComponent: () => import('./features/calendar/calendar.component').then(m => m.CalendarComponent),
        canActivate: [authGuard],
        data: { titleKey: 'PAGE_TITLES.CALENDAR' }
      },
      {
        path: 'bloggers/:id',
        loadComponent: () => import('./features/bloggers/public-profile/public-profile.component').then(m => m.PublicProfileComponent),
        canActivate: [authGuard],
        data: { titleKey: 'PAGE_TITLES.PUBLIC_PROFILE' }
      },
      { path: 'server-error', component: ServerErrorComponent },
      {
        path: 'offers',
        loadChildren: () =>
          import('./features/bloggers/offers/offers.module').then(
            (m) => m.OffersModule
          ),
        canActivate: [authGuard, instagramLinkedGuard],
        data: { titleKey: 'PAGE_TITLES.OFFERS' }
      },
      {
        path: 'reviews',
        loadChildren: () =>
          import('./features/bloggers/reviews/reviews.module').then(
            (m) => m.ReviewsModule
          ),
        canActivate: [authGuard, instagramLinkedGuard],
        data: { titleKey: 'PAGE_TITLES.REVIEWS' }
      },
      {
        path: 'analytics',
        loadChildren: () =>
          import('./features/analytics/analytics.module').then(
            (m) => m.AnalyticsModule
          ),
        canActivate: [authGuard, instagramLinkedGuard],
        data: { titleKey: 'PAGE_TITLES.ANALYTICS' }
      },
      { path: '**', redirectTo: 'profile' },
    ],
  },
];
