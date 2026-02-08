import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

const THEME_STORAGE_KEY = 'lanka-theme';
const DARK_MODE_CLASS = 'dark-mode';

type Theme = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  public readonly theme$: Observable<Theme>;

  private readonly themeSubject = new BehaviorSubject<Theme>('light');

  constructor() {
    this.theme$ = this.themeSubject.asObservable();
  }

  public init(): void {
    const stored = localStorage.getItem(THEME_STORAGE_KEY) as Theme | null;
    const initial: Theme = stored === 'dark' ? 'dark' : 'light';
    this.applyTheme(initial, false);
  }

  public setTheme(theme: Theme, persist = true): void {
    this.applyTheme(theme, persist);
  }

  public toggleTheme(): void {
    const next: Theme = this.themeSubject.value === 'light' ? 'dark' : 'light';
    this.applyTheme(next, true);
  }

  public getCurrentTheme(): Theme {
    return this.themeSubject.value;
  }

  private applyTheme(theme: Theme, persist: boolean): void {
    this.themeSubject.next(theme);

    if (theme === 'dark') {
      document.documentElement.classList.add(DARK_MODE_CLASS);
    } else {
      document.documentElement.classList.remove(DARK_MODE_CLASS);
    }

    if (persist) {
      localStorage.setItem(THEME_STORAGE_KEY, theme);
    }
  }
}
