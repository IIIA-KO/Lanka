import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

import { TranslateService } from '@ngx-translate/core';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'lnk-auth-layout',
  standalone: true,
  imports: [RouterModule, TranslateModule],
  templateUrl: './auth.layout.component.html',
  styleUrls: ['./auth.layout.component.css']
})
export class AuthLayoutComponent {
  constructor(private translate: TranslateService) {}
}
