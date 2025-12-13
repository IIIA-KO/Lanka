import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-language-switcher',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './language-switcher.component.html',
  styleUrl: './language-switcher.component.css',
})
export class LanguageSwitcherComponent {
  public readonly languages: readonly string[];
  public readonly currentLanguage$;

  private readonly languageService = inject(LanguageService);

  constructor() {
    this.languages = this.languageService.getSupportedLanguages();
    this.currentLanguage$ = this.languageService.language$;
  }

  public setLanguage(lang: string): void {
    this.languageService.setLanguage(lang as 'en' | 'uk');
  }
}
