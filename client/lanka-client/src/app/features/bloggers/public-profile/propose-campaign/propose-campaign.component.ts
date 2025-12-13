import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CampaignsAgent } from '../../../../core/api/campaigns.agent';
import { IPendCampaignRequest } from '../../../../core/models/campaigns';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { DatePickerModule } from 'primeng/datepicker';
import { SnackbarService } from '../../../../core/services/snackbar/snackbar.service';

@Component({
  selector: 'app-propose-campaign',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    TextareaModule,
    DatePickerModule
  ],
  templateUrl: './propose-campaign.component.html',
  styleUrls: ['./propose-campaign.component.css']
})
export class ProposeCampaignComponent {
  @Input() public visible = false;
  @Input() public offerId: string | null = null;
  @Input() public offerName: string | null = null;
  @Output() public visibleChange = new EventEmitter<boolean>();
  @Output() public campaignCreated = new EventEmitter<void>();

  public form: FormGroup;
  public loading = false;
  public minDate = new Date();

  private readonly fb = inject(FormBuilder);
  private readonly campaignsApi = inject(CampaignsAgent);
  private readonly snackbar = inject(SnackbarService);

  constructor() {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
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
    const formValue = this.form.value;

    const request: IPendCampaignRequest = {
      name: formValue.name,
      description: formValue.description,
      scheduledOnUtc: formValue.scheduledDate.toISOString(),
      offerId: this.offerId
    };

    this.campaignsApi.proposeCampaign(request).subscribe({
      next: () => {
        this.snackbar.showSuccess('Campaign proposal sent successfully!');
        this.loading = false;
        this.campaignCreated.emit();
        this.onHide();
      },
      error: (err: unknown) => {
        console.error('Failed to propose campaign', err);
        this.snackbar.showError('Failed to send proposal. Please try again.');
        this.loading = false;
      }
    });
  }
}
