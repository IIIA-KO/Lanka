import { Component, Input, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil, catchError, of } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CampaignsAgent, ILiqPayCheckoutResponse, IPaymentStatus } from '../../../../../core/api/campaigns.agent';
import { SnackbarService } from '../../../../../core/services/snackbar/snackbar.service';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, ButtonModule, TagModule, ProgressSpinnerModule, TranslateModule],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css',
})
export class PaymentComponent implements OnInit, OnDestroy {
  @Input({ required: true }) campaignId!: string;
  @Input({ required: true }) amount!: number;
  @Input({ required: true }) currency!: string;
  @Input({ required: true }) isClient!: boolean;

  public payment: IPaymentStatus | null = null;
  public loading = true;
  public paying = false;

  private readonly destroy$ = new Subject<void>();
  private readonly agent = inject(CampaignsAgent);
  private readonly snackbar = inject(SnackbarService);
  private readonly translate = inject(TranslateService);

  public ngOnInit(): void {
    this.loadPayment();
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public get statusSeverity(): 'success' | 'warning' | 'danger' | 'secondary' {
    switch (this.payment?.status) {
      case 'Completed': return 'success';
      case 'Pending':   return 'warning';
      case 'Failed':    return 'danger';
      default:          return 'secondary';
    }
  }

  public onPay(): void {
    this.paying = true;
    this.agent.initiatePayment(this.campaignId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res: ILiqPayCheckoutResponse) => {
          this.paying = false;
          this.submitLiqPayForm(res.data, res.signature);
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

  private submitLiqPayForm(data: string, signature: string): void {
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = 'https://www.liqpay.ua/api/3/checkout';
    form.target = '_blank';

    const dataInput = document.createElement('input');
    dataInput.type = 'hidden';
    dataInput.name = 'data';
    dataInput.value = data;

    const sigInput = document.createElement('input');
    sigInput.type = 'hidden';
    sigInput.name = 'signature';
    sigInput.value = signature;

    form.appendChild(dataInput);
    form.appendChild(sigInput);
    document.body.appendChild(form);
    form.submit();
    document.body.removeChild(form);
  }
}
