import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private loadingCount = 0;
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  public readonly isLoading$ = this._isLoading$.asObservable();

  show() {
    this.loadingCount++;
    if (this.loadingCount === 1) {
      this._isLoading$.next(true);
    }
  }

  hide() {
    if (this.loadingCount > 0) {
      this.loadingCount--;
      if (this.loadingCount === 0) {
        this._isLoading$.next(false);
      }
    }
  }

  reset() {
    this.loadingCount = 0;
    this._isLoading$.next(false);
  }
} 