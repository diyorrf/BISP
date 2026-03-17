import { Component, OnInit, signal } from '@angular/core';
import { LucideAngularModule, Users, FileText, MessageSquare, Bell, CreditCard, TrendingUp, AlertTriangle, Activity } from 'lucide-angular';
import { AdminService } from '../../services/admin.service';
import { AdminStats } from '../../models/admin.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  readonly icons = { Users, FileText, MessageSquare, Bell, CreditCard, TrendingUp, AlertTriangle, Activity };

  stats = signal<AdminStats | null>(null);
  loading = signal(true);

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.adminService.getStats().subscribe({
      next: (s) => { this.stats.set(s); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  planKeys(): string[] {
    const s = this.stats();
    return s ? Object.keys(s.planBreakdown) : [];
  }
}
