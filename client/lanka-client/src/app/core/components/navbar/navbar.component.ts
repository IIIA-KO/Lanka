import { Component, DestroyRef, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PanelMenuModule } from 'primeng/panelmenu';
import { TranslateService } from '@ngx-translate/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

interface MenuItem {
  title: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'app-navbar',
  imports: [CommonModule, PanelMenuModule, RouterModule],
  standalone: true,
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent {
  public open = false;
  public menu: MenuItem[] = [];

  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    this.buildMenu();
    this.translate.onLangChange
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.buildMenu());
  }

  public toggleSidebar(): void {
    this.open = !this.open;
  }

  private buildMenu(): void {
    this.menu = [
      { title: this.translate.instant('NAV.PROFILE'), icon: '/icons/profile-icon.svg', path: '/profile' },
      { title: this.translate.instant('NAV.SEARCH'), icon: '/icons/search-icon.svg', path: '/search' },
      { title: this.translate.instant('NAV.CAMPAIGNS'), icon: '/icons/calendar-icon.svg', path: '/campaigns' },
      { title: this.translate.instant('NAV.PACT'), icon: '/icons/pact-icon.svg', path: '/pact' },
      { title: this.translate.instant('NAV.SETTINGS'), icon: '/icons/settings-icon.svg', path: '/settings/profile' },
    ];
  }
}
