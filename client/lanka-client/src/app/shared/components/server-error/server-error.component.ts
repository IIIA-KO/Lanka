import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-server-error',
  standalone: true,
  imports: [
    ButtonModule,
    CardModule,
    MessageModule,
    TranslateModule
],
  templateUrl: './server-error.component.html',
})
export class ServerErrorComponent {
  public error?: { message: string; details?: string };
  private readonly router = inject(Router);

  constructor() {
    const navigation = this.router.getCurrentNavigation();
    this.error = navigation?.extras.state?.['error'];
  }
}
