import { Component, OnInit, signal } from '@angular/core';
import { LucideAngularModule, CreditCard } from 'lucide-angular';
import { AdminService } from '../../services/admin.service';
import { AdminPayment } from '../../models/admin.model';

@Component({
  selector: 'app-payments',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './payments.component.html'
})
export class PaymentsComponent implements OnInit {
  readonly icons = { CreditCard };

  payments = signal<AdminPayment[]>([]);
  loading = signal(true);

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.adminService.getPayments().subscribe({
      next: (p) => { this.payments.set(p); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString();
  }

  totalRevenue(): number {
    return this.payments().reduce((sum, p) => sum + p.amount, 0);
  }

  planBadgeClass(plan: string): string {
    switch (plan) {
      case 'Pro': return 'bg-blue-100 text-blue-700';
      case 'Enterprise': return 'bg-purple-100 text-purple-700';
      default: return 'bg-slate-100 text-slate-600';
    }
  }
}
