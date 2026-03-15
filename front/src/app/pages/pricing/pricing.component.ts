import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DecimalPipe, DatePipe } from '@angular/common';
import { LucideAngularModule, Check, Zap, Crown, Shield, Loader } from 'lucide-angular';
import { GooglePayButtonModule } from '@google-pay/button-angular';
import { PaymentService, PlanDto, PaymentHistoryDto } from '../../services/payment.service';
import { AccountService } from '../../services/account.service';

@Component({
  selector: 'app-pricing',
  standalone: true,
  imports: [DecimalPipe, DatePipe, LucideAngularModule, GooglePayButtonModule],
  templateUrl: './pricing.component.html'
})
export class PricingComponent implements OnInit {
  readonly icons = { Check, Zap, Crown, Shield, Loader };

  plans = signal<PlanDto[]>([]);
  history = signal<PaymentHistoryDto[]>([]);
  currentPlan = signal<string>('Free');
  loading = signal(true);
  processingPlan = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  constructor(
    private paymentService: PaymentService,
    private accountService: AccountService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.paymentService.getPlans().subscribe({
      next: (plans) => {
        this.plans.set(plans);
        this.loading.set(false);
      }
    });

    this.paymentService.getHistory().subscribe({
      next: (h) => this.history.set(h)
    });

    const acc = this.accountService.account();
    if (acc) {
      this.currentPlan.set(acc.plan);
    }
  }

  getPaymentRequest(plan: PlanDto): google.payments.api.PaymentDataRequest {
    return {
      apiVersion: 2,
      apiVersionMinor: 0,
      allowedPaymentMethods: [
        {
          type: 'CARD',
          parameters: {
            allowedAuthMethods: ['PAN_ONLY', 'CRYPTOGRAM_3DS'],
            allowedCardNetworks: ['VISA', 'MASTERCARD']
          },
          tokenizationSpecification: {
            type: 'PAYMENT_GATEWAY',
            parameters: {
              gateway: 'example',
              gatewayMerchantId: 'exampleGatewayMerchantId'
            }
          }
        }
      ],
      merchantInfo: {
        merchantId: 'BCR2DN4T7XKJIJYR',
        merchantName: 'LegalGuard'
      },
      transactionInfo: {
        totalPriceStatus: 'FINAL',
        totalPriceLabel: `${plan.name} Plan - Monthly`,
        totalPrice: plan.price.toFixed(2),
        currencyCode: plan.currency,
        countryCode: 'US'
      }
    };
  }

  private paymentCallbacks = new Map<string, (data: google.payments.api.PaymentData) => void>();
  private errorCallbacks = new Map<string, (error: any) => void>();

  getPaymentDataCallback(plan: PlanDto): (data: google.payments.api.PaymentData) => void {
    if (!this.paymentCallbacks.has(plan.id)) {
      this.paymentCallbacks.set(plan.id, (data) => this.onPaymentData(plan, data));
    }
    return this.paymentCallbacks.get(plan.id)!;
  }

  getErrorCallback(plan: PlanDto): (error: any) => void {
    if (!this.errorCallbacks.has(plan.id)) {
      this.errorCallbacks.set(plan.id, (error) => this.onPaymentError(plan, error));
    }
    return this.errorCallbacks.get(plan.id)!;
  }

  onPaymentData(plan: PlanDto, paymentData: google.payments.api.PaymentData): void {
    this.processingPlan.set(plan.id);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    const token = paymentData.paymentMethodData.tokenizationData.token;

    this.paymentService.processPayment({
      plan: plan.id,
      paymentToken: token,
      transactionId: undefined
    }).subscribe({
      next: (result) => {
        this.processingPlan.set(null);
        if (result.success) {
          this.successMessage.set(result.message);
          this.currentPlan.set(result.plan);
          this.accountService.load().subscribe();
          this.paymentService.getHistory().subscribe({
            next: (h) => this.history.set(h)
          });
        } else {
          this.errorMessage.set(result.message);
        }
      },
      error: () => {
        this.processingPlan.set(null);
        this.errorMessage.set('Payment processing failed. Please try again.');
      }
    });
  }

  onPaymentError(plan: PlanDto, error: any): void {
    this.processingPlan.set(null);
    this.errorMessage.set('Google Pay encountered an error. Please try again.');
  }

  getPlanIcon(planId: string) {
    switch (planId) {
      case 'Pro': return this.icons.Zap;
      case 'Enterprise': return this.icons.Crown;
      default: return this.icons.Shield;
    }
  }

  getPlanColors(planId: string) {
    switch (planId) {
      case 'Pro': return { card: 'border-blue-300 bg-blue-50/50', badge: 'bg-blue-100 text-blue-700', icon: 'text-blue-600', btn: 'bg-blue-600 hover:bg-blue-700' };
      case 'Enterprise': return { card: 'border-purple-300 bg-purple-50/50', badge: 'bg-purple-100 text-purple-700', icon: 'text-purple-600', btn: 'bg-purple-600 hover:bg-purple-700' };
      default: return { card: 'border-slate-200 bg-white', badge: 'bg-slate-100 text-slate-700', icon: 'text-slate-600', btn: 'bg-slate-600 hover:bg-slate-700' };
    }
  }

  isCurrentPlan(planId: string): boolean {
    return this.currentPlan() === planId;
  }

  isUpgrade(planId: string): boolean {
    const order = ['Free', 'Pro', 'Enterprise'];
    return order.indexOf(planId) > order.indexOf(this.currentPlan());
  }
}
