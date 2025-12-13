import { ChangeDetectionStrategy, Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  IInstagramStatus,
  InstagramOperationState,
} from '../../../core/models/instagram';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-instagram-status-banner',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './instagram-status-banner.component.html',
  styleUrl: './instagram-status-banner.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InstagramStatusBannerComponent {
  @Input() public status: IInstagramStatus | null = null;
  @Input() public title = 'INSTAGRAM_STATUS.DEFAULT_TITLE';
  @Input() public hideWhenEmpty = true;
  @Input() public showTimestamp = true;

  private readonly translate = inject(TranslateService);

  public get statusLabel(): string {
    if (!this.status) {
      return this.translate.instant('INSTAGRAM_STATUS.NO_OPERATIONS');
    }

    switch (this.status.status) {
      case 'pending':
        return this.translate.instant('INSTAGRAM_STATUS.PENDING');
      case 'processing':
        return this.translate.instant('INSTAGRAM_STATUS.PROCESSING');
      case 'completed':
        return this.translate.instant('INSTAGRAM_STATUS.COMPLETED');
      case 'failed':
        return this.translate.instant('INSTAGRAM_STATUS.FAILED');
      default:
        return this.translate.instant('INSTAGRAM_STATUS.UNAVAILABLE');
    }
  }

  public get statusClass(): string {
    if (!this.status) {
      return 'status-idle';
    }

    return `status-${this.status.status}`;
  }

  public get isLoading(): boolean {
    return this.status?.status === 'pending' || this.status?.status === 'processing';
  }

  public get shouldShowEmptyState(): boolean {
    return !this.status && !this.hideWhenEmpty;
  }

  public isFinal(status: InstagramOperationState): boolean {
    return status === 'completed' || status === 'failed';
  }
}
