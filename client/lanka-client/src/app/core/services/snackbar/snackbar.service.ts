import { inject, Injectable, NgZone, Injector } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root',
})
export class SnackbarService {
  private snackBar: MatSnackBar = inject(MatSnackBar);
  private zone: NgZone = inject(NgZone);
  private injector: Injector = inject(Injector);

  private get translate(): TranslateService {
    return this.injector.get(TranslateService);
  }

  public showSuccess(message: string): void {
    const translatedMessage = this.translate.instant(message);
    let closeText = this.translate.instant('COMMON.CLOSE');
    if (closeText === 'COMMON.CLOSE') closeText = 'Close';
    
    this.zone.run(() => {
      this.snackBar.open(translatedMessage, closeText, {
        duration: 5000,
        panelClass: ['snack-success'],
      });
    });
  }

  public showError(message: string): void {
    const translatedMessage = this.translate.instant(message);
    let closeText = this.translate.instant('COMMON.CLOSE');
    if (closeText === 'COMMON.CLOSE') closeText = 'Close';

    this.zone.run(() => {
      this.snackBar.open(translatedMessage, closeText, {
        duration: 5000,
        panelClass: ['snack-error'],
      });
    });
  }
}
