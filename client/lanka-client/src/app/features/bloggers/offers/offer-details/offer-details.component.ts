import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { OffersAgent } from '../../../../core/api/offers.agent';
import { IOffer } from '../../../../core/models/campaigns';
import { SnackbarService } from '../../../../core/services/snackbar/snackbar.service';
import { DeliverableTemplatesService } from '../../../../core/services/deliverable-templates.service';

@Component({
  standalone: true,
  selector: 'app-offer-details',
  imports: [
    CommonModule,
    CurrencyPipe,
    ButtonModule,
    CardModule,
    ProgressSpinnerModule,
    MessageModule,
    TagModule,
    DividerModule,
    ProgressSpinnerModule,
    TranslateModule
  ],
  templateUrl: './offer-details.component.html',
  styleUrls: ['./offer-details.component.css']
})
export class OfferDetailsComponent implements OnInit {
  public offer: IOffer | null = null;
  public loading = false;
  public offerId!: string;

  public readonly templatesService = inject(DeliverableTemplatesService);

  private readonly offersAgent = inject(OffersAgent);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackbarService = inject(SnackbarService);
  private readonly translate = inject(TranslateService);

  public ngOnInit(): void {
    this.offerId = this.route.snapshot.params['id'];
    this.loadOffer();
  }

  public onEdit(): void {
    this.router.navigate(['/offers', this.offerId, 'edit']);
  }

  public onProposeCampaign(): void {
    this.router.navigate(['/campaigns/create'], {
      queryParams: { offerId: this.offerId }
    });
  }

  public onDelete(): void {
    if (confirm(this.translate.instant('OFFERS.MESSAGES.DELETE_CONFIRM_PROMPT'))) {
      this.loading = true;
      this.offersAgent.deleteOffer(this.offerId).pipe(
        catchError(error => {
          this.snackbarService.showError(
            this.translate.instant('OFFERS.MESSAGES.DELETE_ERROR', { message: error?.message || '' })
          );
          return of(null);
        })
      ).subscribe({
        next: (result) => {
          if (result !== null) {
            this.snackbarService.showSuccess(this.translate.instant('OFFERS.MESSAGES.DELETE_SUCCESS'));
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
    if (!this.offerId) {
      this.snackbarService.showError(this.translate.instant('OFFERS.MESSAGES.NO_ID'));
      this.router.navigate(['/offers']);
      return;
    }

    this.loading = true;
    this.offersAgent.getOffer(this.offerId).pipe(
      catchError(error => {
        this.snackbarService.showError(
          this.translate.instant('OFFERS.MESSAGES.LOAD_ERROR', { message: error?.message || '' })
        );
        this.router.navigate(['/offers']);
        return of(null);
      }),
      finalize(() => {
        this.loading = false;
      })
    ).subscribe((offer: IOffer | null) => {
      this.offer = offer;
      this.loading = false;
    });
  }
}
