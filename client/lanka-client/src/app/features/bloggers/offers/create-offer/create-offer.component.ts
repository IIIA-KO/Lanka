import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { catchError, of } from 'rxjs';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';

import { OffersAgent } from '../../../../core/api/offers.agent';
import { ICreateOfferRequest, Currency } from '../../../../core/models/campaigns';
import { SnackbarService } from '../../../../core/services/snackbar/snackbar.service';

@Component({
  standalone: true,
  selector: 'app-create-offer',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    TextareaModule,
    ProgressSpinnerModule,
    MessageModule
  ],
  templateUrl: './create-offer.component.html',
  styleUrls: ['./create-offer.component.css']
})
export class CreateOfferComponent implements OnInit {
  public offerForm!: FormGroup;
  public loading = false;
  public currencies = [
    { label: 'US Dollar (USD)', value: Currency.USD },
    { label: 'Euro (EUR)', value: Currency.EUR },
    { label: 'Ukrainian Hryvnia (UAH)', value: Currency.UAH },
    { label: 'British Pound (GBP)', value: Currency.GBP }
  ];

  private readonly fb = inject(FormBuilder);
  private readonly offersAgent = inject(OffersAgent);
  private readonly router = inject(Router);
  private readonly snackbarService = inject(SnackbarService);

  public ngOnInit(): void {
    this.offerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
      priceAmount: [0, [Validators.required, Validators.min(0.01)]],
      priceCurrency: [Currency.USD, [Validators.required]]
    });
  }

  public onSubmit(): void {
    if (this.offerForm.valid) {
      this.loading = true;
      const request: ICreateOfferRequest = this.offerForm.value;
      
      this.offersAgent.createOffer(request).pipe(
        catchError(error => {
          this.snackbarService.showError('Error creating offer: ' + error.message);
          return of(null);
        })
      ).subscribe({
        next: (result) => {
          if (result) {
            this.snackbarService.showSuccess('Offer created successfully');
            this.router.navigate(['/offers']);
          }
        },
        complete: () => {
          this.loading = false;
        }
      });
    }
  }

  public onCancel(): void {
    this.router.navigate(['/offers']);
  }

  public getFieldError(fieldName: string): string | null {
    const field = this.offerForm.get(fieldName);
    if (field && field.touched && field.errors) {
      if (field.errors['required']) {
        return `${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)} is required`;
      }
      if (field.errors['minlength']) {
        return `${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)} must be at least ${field.errors['minlength'].requiredLength} characters`;
      }
      if (field.errors['maxlength']) {
        return `${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)} must not exceed ${field.errors['maxlength'].requiredLength} characters`;
      }
      if (field.errors['min']) {
        return 'Price must be greater than 0';
      }
    }
    return null;
  }
}
