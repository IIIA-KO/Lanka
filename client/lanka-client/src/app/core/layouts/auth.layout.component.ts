import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';

import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterModule, TranslateModule],
  templateUrl: './auth.layout.component.html',
  styleUrls: ['./auth.layout.component.css']
})
export class AuthLayoutComponent {
  private readonly translate = inject(TranslateService);
}
