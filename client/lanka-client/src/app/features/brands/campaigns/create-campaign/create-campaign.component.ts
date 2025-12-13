import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { catchError, of } from 'rxjs';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { StepperModule } from 'primeng/stepper';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { CheckboxModule } from 'primeng/checkbox';

import { CampaignsAgent } from '../../../../core/api/campaigns.agent';
import { OffersAgent } from '../../../../core/api/offers.agent';
import { ICreateCampaignRequest, IOffer, Currency } from '../../../../core/models/campaigns';
import { SnackbarService } from '../../../../core/services/snackbar/snackbar.service';

@Component({
  standalone: true,
  selector: 'app-create-campaign',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    StepperModule,
    InputTextModule,
    InputNumberModule,
    TextareaModule,
    DatePickerModule,
    SelectModule,
    MessageModule,
    ProgressSpinnerModule,
    CheckboxModule
  ],
  templateUrl: './create-campaign.component.html',
  styleUrls: ['./create-campaign.component.css']
})
export class CreateCampaignComponent implements OnInit {
  public campaignForm!: FormGroup;
  public loading = false;
  public loadingOffer = false;
  public offer: IOffer | null = null;
  public offerId: string | null = null;
  public activeStepIndex = 0;
  public minDate = new Date();
  
  public currencies = [
    { label: 'US Dollar (USD)', value: Currency.USD },
    { label: 'Euro (EUR)', value: Currency.EUR },
    { label: 'Ukrainian Hryvnia (UAH)', value: Currency.UAH },
    { label: 'British Pound (GBP)', value: Currency.GBP }
  ];

  private readonly fb = inject(FormBuilder);
  private readonly campaignsAgent = inject(CampaignsAgent);
  private readonly offersAgent = inject(OffersAgent);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackbarService = inject(SnackbarService);

  public get deliverables(): FormArray {
    return this.campaignForm.get('deliverables') as FormArray;
  }

  public ngOnInit(): void {
    this.offerId = this.route.snapshot.queryParams['offerId'];
    this.initializeForm();
    
    if (this.offerId) {
      this.loadOffer();
    }
  }

  public addDeliverable(): void {
    const deliverableGroup = this.fb.group({
      description: ['', [Validators.required, Validators.minLength(10)]],
      format: ['', [Validators.required]],
      requirements: [''],
      deadline: [null]
    });
    this.deliverables.push(deliverableGroup);
  }

  public removeDeliverable(index: number): void {
    this.deliverables.removeAt(index);
  }

  public onStepChange(index: number): void {
    this.activeStepIndex = index;
  }

  public nextStep(): void {
    if (this.activeStepIndex < 2) {
      this.activeStepIndex++;
    }
  }

  public prevStep(): void {
    if (this.activeStepIndex > 0) {
      this.activeStepIndex--;
    }
  }

  public canProceedToStep(stepIndex: number): boolean {
    if (stepIndex === 1) {
      // Can proceed to deliverables if campaign details are valid
      return (this.campaignForm.get('name')?.valid ?? false) &&
             (this.campaignForm.get('description')?.valid ?? false) &&
             (this.campaignForm.get('expectedCompletionDate')?.valid ?? false);
    }
    if (stepIndex === 2) {
      // Can proceed to review if deliverables are filled
      return this.deliverables.length > 0 && this.deliverables.valid;
    }
    return true;
  }

  public onSubmit(): void {
    if (this.campaignForm.valid && this.offerId) {
      this.loading = true;

      const formValue = this.campaignForm.value;
      const request: ICreateCampaignRequest = {
        offerId: this.offerId,
        name: formValue.name,
        description: formValue.description,
        expectedCompletionDate: formValue.expectedCompletionDate.toISOString(),
        deliverables: formValue.deliverables.map((d: { description: string; format: string; requirements?: string; deadline?: Date }) => ({
          description: d.description,
          format: d.format,
          requirements: d.requirements || undefined,
          deadline: d.deadline ? d.deadline.toISOString() : undefined
        })),
        price: formValue.negotiatePrice && formValue.customPrice ? formValue.customPrice : undefined
      };

      this.campaignsAgent.createCampaign(request).pipe(
        catchError(error => {
          this.snackbarService.showError('Error creating campaign: ' + error.message);
          return of(null);
        })
      ).subscribe({
        next: (campaignId) => {
          if (campaignId) {
            this.snackbarService.showSuccess('Campaign created successfully');
            this.router.navigate(['/campaigns']);
          }
        },
        complete: () => {
          this.loading = false;
        }
      });
    } else if (!this.offerId) {
      this.snackbarService.showError('Offer ID is required to create a campaign');
    }
  }

  public onCancel(): void {
    this.router.navigate(['/offers']);
  }

  private initializeForm(): void {
    this.campaignForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.minLength(20), Validators.maxLength(1000)]],
      expectedCompletionDate: [null, [Validators.required]],
      negotiatePrice: [false],
      customPrice: this.fb.group({
        amount: [0, [Validators.min(0.01)]],
        currency: [Currency.USD]
      }),
      deliverables: this.fb.array([])
    });

    // Add one default deliverable
    this.addDeliverable();
  }

  private loadOffer(): void {
    if (!this.offerId) return;
    
    this.loadingOffer = true;
    this.offersAgent.getOffer(this.offerId).pipe(
      catchError(error => {
        this.snackbarService.showError('Error loading offer: ' + error.message);
        return of(null);
      })
    ).subscribe({
      next: (offer) => {
        if (offer) {
          this.offer = offer;
          // Pre-fill campaign name with offer name
          this.campaignForm.patchValue({
            name: `Campaign for ${offer.name}`
          });

          // Auto-populate deliverables from templates if available
          if (offer.deliverableTemplates && offer.deliverableTemplates.length > 0) {
            this.deliverables.clear();
            offer.deliverableTemplates.forEach(template => {
              this.deliverables.push(this.fb.group({
                description: [template.description, [Validators.required, Validators.minLength(10)]],
                format: [template.format, [Validators.required]],
                requirements: [template.typicalRequirements || ''],
                deadline: [null]
              }));
            });
          } else if (offer.format) {
            // If no templates but format is specified, update the default deliverable's format
            if (this.deliverables.length > 0) {
              this.deliverables.at(0).patchValue({
                format: offer.format
              });
            }
          }
        }
      },
      complete: () => {
        this.loadingOffer = false;
      }
    });
  }
}
