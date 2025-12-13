import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  public readonly isLoading$;
  private readonly isLoadingSubject = new BehaviorSubject<boolean>(false);
  private loadingCount = 0;
  private showTimeout: ReturnType<typeof setTimeout> | null = null;
  private hideTimeout: ReturnType<typeof setTimeout> | null = null;
  private lastShowTimestamp: number | null = null;

  private readonly showDelayMs = 50;
  private readonly hideDelayMs = 0;
  private readonly minVisibleMs = 250;

  constructor() {
    this.isLoading$ = this.isLoadingSubject.asObservable();
  }

  public show(): void {
    this.loadingCount++;
    if (this.loadingCount === 1) {
      if (this.hideTimeout) {
        clearTimeout(this.hideTimeout);
        this.hideTimeout = null;
      }
      this.showTimeout = setTimeout(() => {
        if (this.loadingCount > 0) {
          this.lastShowTimestamp = Date.now();
          this.isLoadingSubject.next(true);
        }
        this.showTimeout = null;
      }, this.showDelayMs);
    }
  }

  public hide(): void {
    if (this.loadingCount > 0) {
      this.loadingCount--;
      if (this.loadingCount === 0) {
        if (this.showTimeout) {
          clearTimeout(this.showTimeout);
          this.showTimeout = null;
        }

        const hideNow = () => {
          this.lastShowTimestamp = null;
          this.isLoadingSubject.next(false);
        };

        if (this.lastShowTimestamp !== null) {
          const elapsed = Date.now() - this.lastShowTimestamp;
          const remaining = this.minVisibleMs - elapsed;
          if (remaining > 0) {
            this.hideTimeout = setTimeout(() => {
              hideNow();
              this.hideTimeout = null;
            }, remaining + this.hideDelayMs);
            return;
          }
        }

        this.hideTimeout = setTimeout(() => {
          hideNow();
          this.hideTimeout = null;
        }, this.hideDelayMs);
      }
    }
  }

  public reset(): void {
    this.loadingCount = 0;
    if (this.showTimeout) {
      clearTimeout(this.showTimeout);
      this.showTimeout = null;
    }
    if (this.hideTimeout) {
      clearTimeout(this.hideTimeout);
      this.hideTimeout = null;
    }
    this.lastShowTimestamp = null;
    this.isLoadingSubject.next(false);
  }
}
