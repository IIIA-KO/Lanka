import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LanguageService } from './core/services/language.service';
import { ThemeService } from './core/services/theme.service';
@Component({
  selector: 'app-root',
  imports: [RouterOutlet ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  public title = 'lanka-client';
  private readonly languageService = inject(LanguageService);
  private readonly themeService = inject(ThemeService);

  constructor() {
    this.languageService.init();
    this.themeService.init();
  }
}
