import { Component, inject } from '@angular/core';
import { RouterOutlet, NavigationEnd, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { AuthService } from '../../services/auth/auth.service';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { FooterComponent } from '../../components/footer/footer.component';
import { HeaderComponent } from '../../components/header/header.component';
import { combineLatest, filter, map, startWith } from 'rxjs';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    CommonModule,
    NavbarComponent,
    LoadingComponent,
    FooterComponent,
    HeaderComponent
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.css',
})
export class LayoutComponent {
  public readonly isAuthenticated$;
  public readonly showNavigation$;

  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly isOnLinkingFlow$;

  constructor() {
    this.isAuthenticated$ = this.auth.isAuthenticated$;
    this.isOnLinkingFlow$ = this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects),
      startWith(this.router.url),
      map((url) => this.isLinkingRoute(url))
    );

    this.showNavigation$ = combineLatest([
      this.isAuthenticated$,
      this.isOnLinkingFlow$,
    ]).pipe(map(([isAuthenticated, onLinkingFlow]) => isAuthenticated && !onLinkingFlow));
  }

  private isLinkingRoute(url: string): boolean {
    return url.startsWith('/link-instagram');
  }
}
