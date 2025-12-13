import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { catchError, finalize, of, switchMap, take } from 'rxjs'; // Removed unused Observable

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { OffersAgent } from '../../../../core/api/offers.agent'; // Fixed path
import { SnackbarService } from '../../../../core/services/snackbar/snackbar.service'; // Fixed path
import { IOffer, ICreateOfferRequest, IEditOfferRequest } from '../../../../core/models/campaigns'; // Fixed path

@Component({
  standalone: true,
  selector: 'app-offer-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    InputTextModule,
    InputNumberModule,
    TextareaModule,
    SelectModule,
    MessageModule,
    ProgressSpinnerModule,
    TooltipModule,
    TranslateModule
  ],
  templateUrl: './offer-form.component.html',
  styleUrls: ['./offer-form.component.css']
})
export class OfferFormComponent implements OnInit {
  public offerForm!: FormGroup;
  public loading = false;
  public submitLoading = false;
  public error: string | null = null;
  public isEditMode = false;
  public currentOfferId: string | null = null;

  public currencyOptions = [
    { label: 'USD ($)', value: 'USD' },
    { label: 'EUR (€)', value: 'EUR' },
    { label: 'GBP (£)', value: 'GBP' }
  ];

  private readonly fb = inject(FormBuilder);
  private readonly offersAgent = inject(OffersAgent);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackbar = inject(SnackbarService);
  private readonly translate = inject(TranslateService);

  public ngOnInit(): void {
    this.initializeForm();
    this.checkRouteForEditMode();
  }

  public onSubmit(): void {
    if (this.offerForm.invalid) {
      return;
    }

    this.submitLoading = true;
    this.error = null;

    if (this.isEditMode && this.currentOfferId) {
      this.updateOffer();
    } else {
      this.createOffer();
    }
  }

  public onCancel(): void {
    this.router.navigate(['/pact']);
  }

  public onDelete(): void {
    if (this.isEditMode && this.currentOfferId) {
       // Ideally show confirmation dialog
       if (confirm(this.translate.instant('OFFERS.MESSAGES.DELETE_CONFIRM_PROMPT'))) {
          this.submitLoading = true;
          this.offersAgent.deleteOffer(this.currentOfferId).subscribe({
            next: () => {
              this.snackbar.showSuccess(this.translate.instant('OFFERS.MESSAGES.DELETE_SUCCESS'));
              this.router.navigate(['/pact']);
            },
            error: () => {
              this.submitLoading = false;
              this.error = this.translate.instant('OFFERS.MESSAGES.DELETE_FAIL');
            }
          });
       }
    }
  }

  private initializeForm(): void {
    this.offerForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.maxLength(500)]],
      priceAmount: [null, [Validators.required, Validators.min(0)]],
      priceCurrency: ['USD', [Validators.required]],
      // Future: Duration, Content Type, etc. can be added here
    });
  }

  private checkRouteForEditMode(): void {
    this.route.params.pipe(
      take(1), // Only need the initial route parameters
      switchMap(params => {
        if (params['id']) {
          this.isEditMode = true;
          this.currentOfferId = params['id'];
          this.loading = true;
          return this.offersAgent.getOffer(this.currentOfferId!);
        }
        return of(null);
      }),
      catchError(() => { // Removed unused error param
        this.error = this.translate.instant('OFFERS.MESSAGES.LOAD_FAIL');
        return of(null);
      }),
      finalize(() => this.loading = false)
    ).subscribe(offer => {
      if (offer) {
        this.patchForm(offer);
      }
    });
  }

  private patchForm(offer: IOffer): void {
    this.offerForm.patchValue({
      name: offer.name,
      description: offer.description,
      priceAmount: offer.priceAmount,
      priceCurrency: offer.priceCurrency
    });
  }

  private createOffer(): void {
    const request: ICreateOfferRequest = this.offerForm.value;

    this.offersAgent.createOffer(request).subscribe({
      next: () => {
        this.snackbar.showSuccess(this.translate.instant('OFFERS.MESSAGES.CREATE_SUCCESS'));
        this.router.navigate(['/pact']);
      },
      error: () => {
        this.submitLoading = false;
        // Global error interceptor might handle this, but we can set local error too
        this.error = this.translate.instant('OFFERS.MESSAGES.CREATE_FAIL');
      }
    });
  }

  private updateOffer(): void {
    const request: IEditOfferRequest = {
      offerId: this.currentOfferId!,
      ...this.offerForm.value
    };

    this.offersAgent.editOffer(request).subscribe({
      next: () => {
        this.snackbar.showSuccess(this.translate.instant('OFFERS.MESSAGES.UPDATE_SUCCESS'));
        this.router.navigate(['/pact']);
      },
      error: () => {
        this.submitLoading = false;
        this.error = this.translate.instant('OFFERS.MESSAGES.UPDATE_FAIL');
      }
    });
  }
}
