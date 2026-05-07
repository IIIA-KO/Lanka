import { Component, Input, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil, catchError, of } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CampaignsAgent, IPaymentCheckoutResponse, IPaymentStatus } from '../../../../../core/api/campaigns.agent';
import { SnackbarService } from '../../../../../core/services/snackbar/snackbar.service';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, ButtonModule, TagModule, ProgressSpinnerModule, TranslateModule],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css',
})
export class PaymentComponent implements OnInit, OnDestroy {
  @Input({ required: true }) public campaignId!: string;
  @Input({ required: true }) public amount!: number;
  @Input({ required: true }) public currency!: string;
  @Input({ required: true }) public isClient!: boolean;
  @Input() public allowCta = true;

  public payment: IPaymentStatus | null = null;
  public loading = true;
  public paying = false;

  private readonly destroy$ = new Subject<void>();
  private readonly agent = inject(CampaignsAgent);
  private readonly snackbar = inject(SnackbarService);
  private readonly translate = inject(TranslateService);

  public get statusSeverity(): 'success' | 'warning' | 'danger' | 'secondary' {
    switch (this.payment?.status) {
      case 'Completed': return 'success';
      case 'Pending':   return 'warning';
      case 'Failed':    return 'danger';
      default:          return 'secondary';
    }
  }

  public get canContinuePayment(): boolean {
    return this.isClient
      && this.allowCta
      && (this.payment?.status === 'Pending' || this.payment?.status === 'Failed');
  }

  public ngOnInit(): void {
    if (this.isClient && this.allowCta) {
      this.loading = false;
      return;
    }

    this.loadPayment();
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public onPay(): void {
    this.paying = true;
    this.agent.initiatePayment(this.campaignId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res: IPaymentCheckoutResponse) => {
          this.paying = false;
          this.submitCheckoutForm(res);
        },
        error: () => {
          this.paying = false;
          this.snackbar.showError(this.translate.instant('CAMPAIGNS.PAYMENT.ERROR_INITIATE'));
        },
      });
  }

  private loadPayment(): void {
    this.agent.getPayment(this.campaignId)
      .pipe(takeUntil(this.destroy$), catchError(() => of(null)))
      .subscribe(p => {
        this.payment = p;
        this.loading = false;
      });
  }

  private submitCheckoutForm(checkout: IPaymentCheckoutResponse): void {
    const form = document.createElement('form');
    form.method = checkout.method;
    form.action = checkout.actionUrl;
    form.target = '_blank';

    for (const [name, value] of Object.entries(checkout.fields)) {
      const input = document.createElement('input');
      input.type = 'hidden';
      input.name = name;
      input.value = value;
      form.appendChild(input);
    }

    document.body.appendChild(form);
    form.submit();
    document.body.removeChild(form);
  }
}
