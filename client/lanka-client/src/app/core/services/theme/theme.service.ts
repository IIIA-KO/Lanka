import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly STORAGE_KEY = 'lanka-theme-preference';
  private readonly DARK_CLASS = 'lanka-dark';

  // Read-only signal so components can react to theme changes
  public readonly isDark = signal<boolean>(false);

  constructor() {
    this.initTheme();
  }

  public toggleTheme(): void {
    const isCurrentlyDark = this.isDark();
    this.setTheme(!isCurrentlyDark);
  }

  public setTheme(isDark: boolean): void {
    if (isDark) {
      document.documentElement.classList.add(this.DARK_CLASS);
    } else {
      document.documentElement.classList.remove(this.DARK_CLASS);
    }

    localStorage.setItem(this.STORAGE_KEY, isDark ? 'dark' : 'light');
    this.isDark.set(isDark);
  }

  private initTheme(): void {
    const storedPreference = localStorage.getItem(this.STORAGE_KEY);

    if (storedPreference === 'dark') {
      this.setTheme(true);
    } else if (storedPreference === 'light') {
      this.setTheme(false);
    } else {
      // No strict preference found, fallback to system preference
      const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.setTheme(prefersDark);
    }

    // Optionally set up an OS-level listener if we want it to dynamic-switch when they haven't explicitly chosen
    if (window.matchMedia) {
      window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', event => {
        // Only auto-switch if the user hasn't explicitly set a preference
        if (!localStorage.getItem(this.STORAGE_KEY)) {
           this.setTheme(event.matches);
        }
      });
    }
  }
}
