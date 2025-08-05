import { Component } from '@angular/core';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { instagramClientId, instagramRedirectUri, instagramScope, instagramResponseType, instagramConfigId } from '../../core/constants/instagram.constants';

@Component({
  selector: 'app-link-instagram',
  templateUrl: './link-instagram.component.html',
  imports: [TranslateModule],
})
export class LinkInstagramComponent {
  constructor(private translate: TranslateService) {}

  handleInstagramLink(): void {
    const authUrl = `https://www.facebook.com/v20.0/dialog/oauth?client_id=${instagramClientId}&redirect_uri=${instagramRedirectUri}&scope=${instagramScope}&response_type=${instagramResponseType}&config_id=${instagramConfigId}`;
    window.location.href = authUrl;
  }
}
