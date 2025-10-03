import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
@Component({
  selector: 'app-root',
  imports: [RouterOutlet ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  public title = 'lanka-client';
  private readonly translate = inject(TranslateService);

  constructor() {
    const browserLang = this.translate.getBrowserLang();
    this.translate.use(browserLang?.match(/en|uk/) ? browserLang : 'en');
  }
}
