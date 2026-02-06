import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, firstValueFrom } from 'rxjs';
import { Router } from '@angular/router';
import {
  instagramClientId,
  instagramRedirectUri,
  instagramScope,
  instagramResponseType,
  instagramConfigId,
} from '../../core/constants/instagram.constants';
import { InstagramStatusService } from '../../core/services/instagram-status.service';
import { IInstagramStatus } from '../../core/models/instagram';
import { InstagramStatusBannerComponent } from '../../shared/components/instagram-status-banner/instagram-status-banner.component';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-link-instagram',
  templateUrl: './link-instagram.component.html',
  styleUrl: './link-instagram.component.css',
  imports: [CommonModule, InstagramStatusBannerComponent, ButtonModule, TranslateModule],
})
export class LinkInstagramComponent implements OnInit {
  public linkStatus$!: Observable<IInstagramStatus | null>;

  private readonly instagramStatusService = inject(InstagramStatusService);
  private readonly router = inject(Router);

  public ngOnInit(): void {
    void this.initialize();
  }

  public handleInstagramLink(): void {
    const authUrl = `https://www.facebook.com/v20.0/dialog/oauth?client_id=${instagramClientId}&redirect_uri=${instagramRedirectUri}&scope=${instagramScope}&response_type=${instagramResponseType}&config_id=${instagramConfigId}`;
    window.location.href = authUrl;
  }

  private async initialize(): Promise<void> {
    this.instagramStatusService.init({ force: true });
    await this.instagramStatusService.ensureSignalRConnection();
    await this.instagramStatusService.syncLinkingStatus();

    this.linkStatus$ = this.instagramStatusService.linkingStatus$;

    const latestStatus = await firstValueFrom(this.linkStatus$);
    if (latestStatus?.status === 'completed') {
      this.router.navigate(['/profile']);
    }
  }
}
