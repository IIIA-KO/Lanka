import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { Router } from '@angular/router';
import { catchError, of } from 'rxjs';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { SelectModule } from 'primeng/select';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';

import { OffersAgent } from '../../../core/api/offers.agent';
import { IOffer } from '../../../core/models/campaigns';
import { SnackbarService } from '../../../core/services/snackbar/snackbar.service';

@Component({
  standalone: true,
  selector: 'app-offers',
  imports: [
    CommonModule,
    CurrencyPipe,
    ButtonModule,
    CardModule,
    SelectModule,
    ProgressSpinnerModule,
    MessageModule
  ],
  templateUrl: './offers.component.html',
  styleUrls: ['./offers.component.css']
})
export class OffersComponent implements OnInit {
  public offers: IOffer[] = [];
  public loading = false;
  public error: string | null = null;

  private readonly offersAgent = inject(OffersAgent);
  private readonly router = inject(Router);
  private readonly snackbarService = inject(SnackbarService);

  public ngOnInit(): void {
    this.loadOffers();
  }

  public navigateToCreate(): void {
    this.router.navigate(['/offers/create']);
  }

  public viewOffer(id: string): void {
    this.router.navigate(['/offers', id]);
  }

  public editOffer(id: string): void {
    this.router.navigate(['/offers', id, 'edit']);
  }

  public deleteOffer(id: string): void {
    if (confirm('Are you sure you want to delete this offer?')) {
      this.loading = true;
      this.offersAgent.deleteOffer(id).pipe(
        catchError(error => {
          this.snackbarService.showError('Error deleting offer: ' + error.message);
          return of(null);
        })
      ).subscribe({
        next: () => {
          this.snackbarService.showSuccess('Offer deleted successfully');
          // Reload offers list
          this.ngOnInit();
        },
        complete: () => {
          this.loading = false;
        }
      });
    }
  }

  public onOfferAction(event: { value?: string } | string, offerId: string): void {
    const action = event.value || event;
    switch (action) {
      case 'view':
        this.viewOffer(offerId);
        break;
      case 'edit':
        this.editOffer(offerId);
        break;
      case 'delete':
        this.deleteOffer(offerId);
        break;
    }
  }

  private loadOffers(): void {
    this.loading = true;
    this.error = null;
    
    // For now, we'll show a placeholder since there's no "get all offers" endpoint
    // This would typically load the user's offers
    this.offers = [
      {
        id: '1',
        name: 'Sample Offer 1',
        description: 'This is a sample offer description',
        priceAmount: 100,
        priceCurrency: 'USD',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      }
    ];
    
    this.loading = false;
  }
} 