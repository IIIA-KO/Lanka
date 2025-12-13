import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter, map, mergeMap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { AvatarModule } from 'primeng/avatar';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { AuthService } from '../../services/auth/auth.service';
import { AgentService } from '../../api/agent';
import { IBloggerProfile } from '../../models/blogger';

import { InputTextModule } from 'primeng/inputtext';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageSwitcherComponent } from '../../../shared/components/language-switcher/language-switcher.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    AvatarModule,
    MenuModule,
    InputTextModule,
    TranslateModule,
    LanguageSwitcherComponent
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit {
  public pageTitle$: Observable<string>;
  public profile$: Observable<IBloggerProfile>;
  public userMenuItems: MenuItem[] = [];

  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private authService = inject(AuthService);
  private api = inject(AgentService);
  private readonly translate = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    this.pageTitle$ = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      map(() => this.activatedRoute),
      map(route => {
        while (route.firstChild) route = route.firstChild;
        return route;
      }),
      mergeMap(route => route.data),
      map(data => data['titleKey'] || data['title'] || 'COMMON.APP_NAME'),
      mergeMap((key) => this.translate.stream(key))
    );

    this.profile$ = this.api.Bloggers.getProfile();
  }

  public ngOnInit() {
    this.buildMenuItems();
    this.translate.onLangChange
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.buildMenuItems());
  }

  public buildUserSubtitle(profile?: IBloggerProfile | null): string {
    if (!profile) {
      return this.translate.instant('COMMON.BLOGGER');
    }

    if (profile.instagramUsername) {
      return `@${profile.instagramUsername}`;
    }

    if (profile.category) {
      return profile.category;
    }

    return this.translate.instant('COMMON.BLOGGER');
  }

  private buildMenuItems(): void {
    this.userMenuItems = [
      {
        label: this.translate.instant('NAV.PROFILE'),
        icon: 'pi pi-user',
        routerLink: '/profile'
      },
      {
        label: this.translate.instant('NAV.SETTINGS'),
        icon: 'pi pi-cog',
        routerLink: '/settings/profile'
      },
      {
        separator: true
      },
      {
        label: this.translate.instant('NAV.LOGOUT'),
        icon: 'pi pi-sign-out',
        command: () => this.authService.logout()
      }
    ];
  }
}
