import { inject, Injectable, NgZone } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root',
})
export class SnackbarService {
  private snackBar: MatSnackBar = inject(MatSnackBar);
  private zone: NgZone = inject(NgZone);

  showSuccess(message: string, title?: string) {
    this.zone.run(() => {
      this.snackBar.open(message, 'Close', {
        duration: 5000,
        panelClass: ['snack-success'],
      });
    });
  }

  showError(message: string, title?: string) {
    this.zone.run(() => {
      this.snackBar.open(message, 'Close', {
        duration: 5000,
        panelClass: ['snack-error'],
      });
    });
  }
}
