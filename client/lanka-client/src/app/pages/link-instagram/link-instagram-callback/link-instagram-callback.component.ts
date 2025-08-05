import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { AgentService } from '../../../core/api/agent';

@Component({
  selector: 'app-link-instagram-callback',
  templateUrl: './link-instagram-callback.component.html',
  imports: [TranslateModule],
})
export class LinkInstagramCallbackComponent {
  message = 'Loading...';
  private isFetched = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: AgentService,
    private translate: TranslateService
  ) {}

  ngOnInit() {
    if (this.isFetched) {
      return;
    }

    this.isFetched = true;

    this.message = 'Linking Your Instagram Account...';

    const params = new URLSearchParams(window.location.search);
    const code = params.get('code');
    console.log('Linking Instagram with code:', code);

    if (code) {
      this.api.Users.linkInstagram(code).subscribe({
        next: (response) => {
          this.message = 'Instagram account linked successfully';
          this.router.navigate(['/']);
        },
        error: (error) => {
          this.message = 'Failed to link Instagram account';
          console.error('Error:', error);
        },
      });
    } else {
      this.message = 'No code provided for linking Instagram account';
      console.warn('No code found in URL parameters');
      this.router.navigate(['/']);
    }
  }
}
