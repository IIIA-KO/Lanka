import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-theme-switcher',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      type="button"
      class="theme-toggle"
      (click)="toggle()"
      [attr.aria-label]="(theme$ | async) === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'">
      <i class="pi" [ngClass]="(theme$ | async) === 'dark' ? 'pi-sun' : 'pi-moon'"></i>
    </button>
  `,
  styles: `
    .theme-toggle {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 2.25rem;
      height: 2.25rem;
      border: 1px solid var(--app-border);
      background: var(--app-surface-alt);
      border-radius: 999px;
      cursor: pointer;
      transition: all 0.2s ease;
      color: var(--p-text-color);
      font-size: 1rem;
    }

    .theme-toggle:hover {
      background: var(--app-surface-hover);
    }
  `,
})
export class ThemeSwitcherComponent {
  public readonly theme$;

  private readonly themeService = inject(ThemeService);

  constructor() {
    this.theme$ = this.themeService.theme$;
  }

  public toggle(): void {
    this.themeService.toggleTheme();
  }
}
