import { Component, OnInit } from '@angular/core';
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
  offer: IOffer | null = null;
  loading = false;
  offerId!: string;

  constructor(
    private offersAgent: OffersAgent,
    private router: Router,
    private route: ActivatedRoute,
    private snackbarService: SnackbarService
  ) {}

  ngOnInit(): void {
    this.offerId = this.route.snapshot.params['id'];
    this.loadOffer();
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

  onEdit(): void {
    this.router.navigate(['/offers', this.offerId, 'edit']);
  }

  onDelete(): void {
    if (confirm('Are you sure you want to delete this offer? This action cannot be undone.')) {
      this.loading = true;
      this.offersAgent.deleteOffer(this.offerId).pipe(
        catchError(error => {
          this.snackbarService.showError('Error deleting offer: ' + error.message);
          return of(null);
        })
      ).subscribe({
        next: () => {
          this.snackbarService.showSuccess('Offer deleted successfully');
          this.router.navigate(['/offers']);
        },
        complete: () => {
          this.loading = false;
        }
      });
    }
  }

  onBack(): void {
    this.router.navigate(['/offers']);
  }
}
