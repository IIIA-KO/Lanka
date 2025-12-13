import { Component, inject } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { NavbarComponent } from '../../core/components/navbar/navbar.component';
import { HeaderComponent } from '../../core/components/header/header.component';
import { FooterComponent } from '../../core/components/footer/footer.component';
import { AuthService } from '../../core/services/auth/auth.service';
import { Router } from '@angular/router';
import { LanguageSwitcherComponent } from '../../shared/components/language-switcher/language-switcher.component';

@Component({
  selector: 'app-privacy-policy',
  imports: [
    CommonModule,
    TranslateModule,
    ButtonModule,
    NavbarComponent,
    HeaderComponent,
    FooterComponent,
    LanguageSwitcherComponent
  ],
  templateUrl: './privacy-policy.component.html',
  styleUrl: './privacy-policy.component.css',
})
export class PrivacyPolicyComponent {
  public readonly isAuthenticated$ = inject(AuthService).isAuthenticated$;

  private readonly translate = inject(TranslateService);
  private readonly location = inject(Location);
  private readonly router = inject(Router);

  public onBack(): void {
    if (window.history.length > 1) {
      this.location.back();
    } else {
      this.router.navigate(['/']);
    }
  }
}
