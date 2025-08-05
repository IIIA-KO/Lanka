import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
@Component({
  selector: 'app-root',
  imports: [RouterOutlet ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  title = 'lanka-client';
  constructor(private translate: TranslateService) {
    const browserLang = translate.getBrowserLang();
    translate.use(browserLang?.match(/en|uk/) ? browserLang : 'en');
  }
}
