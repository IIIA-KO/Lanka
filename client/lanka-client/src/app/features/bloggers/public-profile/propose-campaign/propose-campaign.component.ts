import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CampaignsAgent } from '../../../../core/api/campaigns.agent';
import { IPendCampaignRequest } from '../../../../core/models/campaigns';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { DatePickerModule } from 'primeng/datepicker';
import { SnackbarService } from '../../../../core/services/snackbar/snackbar.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

const CAMPAIGN_NAME_MAX_LENGTH = 50;

@Component({
  selector: 'app-propose-campaign',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    DialogModule,
    DatePickerModule,
    TranslateModule
  ],
  templateUrl: './propose-campaign.component.html',
  styleUrls: ['./propose-campaign.component.css']
})
export class ProposeCampaignComponent {
  @Input() public visible = false;
  @Input() public offerId: string | null = null;
  @Input() public offerName: string | null = null;
  @Input() public offerDescription: string | null = null;
  @Output() public visibleChange = new EventEmitter<boolean>();
  @Output() public campaignCreated = new EventEmitter<void>();

  public form: FormGroup;
  public loading = false;
  public minDate = new Date();

  private readonly fb = inject(FormBuilder);
  private readonly campaignsApi = inject(CampaignsAgent);
  private readonly snackbar = inject(SnackbarService);
  private readonly translate = inject(TranslateService);

  constructor() {
    this.form = this.fb.group({
      scheduledDate: [null, [Validators.required]]
    });
  }

  public onHide(): void {
    this.visible = false;
    this.visibleChange.emit(false);
    this.form.reset();
  }

  public onSubmit(): void {
    if (this.form.invalid || !this.offerId) return;

    this.loading = true;
    const offerName = (this.offerName ?? '').trim();
    const offerDescription = (this.offerDescription ?? '').trim();
    const fallback = offerName.length > 0
      ? this.translate.instant('PUBLIC_PROFILE.PROPOSE.DEFAULT_NAME', { name: offerName })
      : this.translate.instant('PUBLIC_PROFILE.PROPOSE.NEW_CAMPAIGN');

    const name = fallback.slice(0, CAMPAIGN_NAME_MAX_LENGTH);
    const description = offerDescription.length > 0 ? offerDescription : fallback;

    const request: IPendCampaignRequest = {
      name,
      description,
      scheduledOnUtc: (this.form.value.scheduledDate as Date).toISOString(),
      offerId: this.offerId
    };

    this.campaignsApi.proposeCampaign(request).subscribe({
      next: () => {
        this.snackbar.showSuccess(this.translate.instant('PUBLIC_PROFILE.PROPOSE.SUCCESS'));
        this.loading = false;
        this.campaignCreated.emit();
        this.onHide();
      },
      error: (err: unknown) => {
        console.error('Failed to propose campaign', err);
        this.snackbar.showError(this.translate.instant('PUBLIC_PROFILE.PROPOSE.ERROR'));
        this.loading = false;
      }
    });
  }
}
