import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PlanDto {
  id: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  dailyTokens: number;
  maxDocuments: number;
  features: string[];
}

export interface ProcessPaymentRequest {
  plan: string;
  paymentToken: string;
  transactionId?: string;
}

export interface PaymentResultDto {
  success: boolean;
  message: string;
  plan: string;
  tokensRemaining: number;
  planExpiresAt?: string;
}

export interface PaymentHistoryDto {
  id: string;
  plan: string;
  amount: number;
  currency: string;
  status: string;
  createdAt: string;
  planExpiresAt?: string;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private url = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  getPlans(): Observable<PlanDto[]> {
    return this.http.get<PlanDto[]>(`${this.url}/plans`);
  }

  processPayment(request: ProcessPaymentRequest): Observable<PaymentResultDto> {
    return this.http.post<PaymentResultDto>(`${this.url}/process`, request);
  }

  getHistory(): Observable<PaymentHistoryDto[]> {
    return this.http.get<PaymentHistoryDto[]>(`${this.url}/history`);
  }
}
