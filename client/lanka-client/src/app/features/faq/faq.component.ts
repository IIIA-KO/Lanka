import { CommonModule, Location } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { NavbarComponent } from '../../core/components/navbar/navbar.component';
import { HeaderComponent } from '../../core/components/header/header.component';
import { FooterComponent } from '../../core/components/footer/footer.component';
import { AuthService } from '../../core/services/auth/auth.service';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageSwitcherComponent } from '../../shared/components/language-switcher/language-switcher.component';

@Component({
  selector: 'app-faq',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    NavbarComponent,
    HeaderComponent,
    FooterComponent,
    TranslateModule,
    LanguageSwitcherComponent
  ],
  templateUrl: './faq.component.html',
  styleUrl: './faq.component.css',
})
export class FaqComponent {
  public readonly isAuthenticated$ = inject(AuthService).isAuthenticated$;

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
