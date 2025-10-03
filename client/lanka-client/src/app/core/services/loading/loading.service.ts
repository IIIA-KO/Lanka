import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  public readonly isLoading$ = this._isLoading$.asObservable();
  private loadingCount = 0;
  private _isLoading$ = new BehaviorSubject<boolean>(false);

  public show(): void {
    this.loadingCount++;
    if (this.loadingCount === 1) {
      this._isLoading$.next(true);
    }
  }

  public hide(): void {
    if (this.loadingCount > 0) {
      this.loadingCount--;
      if (this.loadingCount === 0) {
        this._isLoading$.next(false);
      }
    }
  }

  public reset(): void {
    this.loadingCount = 0;
    this._isLoading$.next(false);
  }
} 