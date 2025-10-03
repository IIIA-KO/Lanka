import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, of } from 'rxjs';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';

import { OffersAgent } from '../../../../core/api/offers.agent';
import { IOffer } from '../../../../core/models/campaigns';
import { SnackbarService } from '../../../../core/services/snackbar/snackbar.service';

@Component({
  standalone: true,
  selector: 'app-offer-details',
  imports: [
    CommonModule,
    CurrencyPipe,
    ButtonModule,
    CardModule,
    ProgressSpinnerModule,
    MessageModule
  ],
  templateUrl: './offer-details.component.html',
  styleUrls: ['./offer-details.component.css']
})
export class OfferDetailsComponent implements OnInit {
  public offer: IOffer | null = null;
  public loading = false;
  public offerId!: string;

  private readonly offersAgent = inject(OffersAgent);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackbarService = inject(SnackbarService);

  public ngOnInit(): void {
    this.offerId = this.route.snapshot.params['id'];
    this.loadOffer();
  }

  public onEdit(): void {
    this.router.navigate(['/offers', this.offerId, 'edit']);
  }

  public onDelete(): void {
    if (confirm('Are you sure you want to delete this offer?')) {
      this.loading = true;
      this.offersAgent.deleteOffer(this.offerId).pipe(
        catchError(error => {
          this.snackbarService.showError('Error deleting offer: ' + error.message);
          return of(null);
        })
      ).subscribe({
        next: (result) => {
          if (result !== null) {
            this.snackbarService.showSuccess('Offer deleted successfully');
            this.router.navigate(['/offers']);
          }
        },
        complete: () => {
          this.loading = false;
        }
      });
    }
  }

  public onBack(): void {
    this.router.navigate(['/offers']);
  }

  private loadOffer(): void {
    this.loading = true;
    this.offersAgent.getOffer(this.offerId).pipe(
      catchError(error => {
        this.snackbarService.showError('Error loading offer: ' + error.message);
        this.router.navigate(['/offers']);
        return of(null);
      })
    ).subscribe({
      next: (offer: IOffer | null) => {
        this.offer = offer;
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
}
