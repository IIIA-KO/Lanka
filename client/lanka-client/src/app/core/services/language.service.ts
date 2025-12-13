import { Injectable, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';

const LANGUAGE_STORAGE_KEY = 'lanka-lang';
const SUPPORTED_LANGUAGES = ['en', 'uk'] as const;
type SupportedLanguage = (typeof SUPPORTED_LANGUAGES)[number];

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  private readonly translate = inject(TranslateService);
  private readonly languageSubject = new BehaviorSubject<SupportedLanguage>('en');

  public readonly language$ = this.languageSubject.asObservable();

  public init(): void {
    const stored = localStorage.getItem(LANGUAGE_STORAGE_KEY) as SupportedLanguage | null;
    const browser = this.translate.getBrowserLang() as SupportedLanguage | undefined;

    const initial = this.normalizeLanguage(stored) ??
      this.normalizeLanguage(browser) ??
      'en';

    this.setLanguage(initial, false);
  }

  public setLanguage(lang: SupportedLanguage, persist = true): void {
    const normalized = this.normalizeLanguage(lang) ?? 'en';

    this.translate.setDefaultLang('en');
    this.translate.use(normalized);
    this.languageSubject.next(normalized);

    if (persist) {
      localStorage.setItem(LANGUAGE_STORAGE_KEY, normalized);
    }
  }

  public getCurrentLanguage(): SupportedLanguage {
    return this.languageSubject.value;
  }

  public getSupportedLanguages(): SupportedLanguage[] {
    return SUPPORTED_LANGUAGES.slice() as SupportedLanguage[];
  }

  private normalizeLanguage(lang?: string | null): SupportedLanguage | null {
    if (!lang) return null;
    return SUPPORTED_LANGUAGES.includes(lang as SupportedLanguage)
      ? (lang as SupportedLanguage)
      : null;
  }
}
