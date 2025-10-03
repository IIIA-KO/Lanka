import { inject, Injectable, NgZone } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root',
})
export class SnackbarService {
  private snackBar: MatSnackBar = inject(MatSnackBar);
  private zone: NgZone = inject(NgZone);

  public showSuccess(message: string): void {
    this.zone.run(() => {
      this.snackBar.open(message, 'Close', {
        duration: 5000,
        panelClass: ['snack-success'],
      });
    });
  }

  public showError(message: string): void {
    this.zone.run(() => {
      this.snackBar.open(message, 'Close', {
        duration: 5000,
        panelClass: ['snack-error'],
      });
    });
  }
}
