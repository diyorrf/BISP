import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, ArrowLeft, Save, FileText, CreditCard } from 'lucide-angular';
import { AdminService } from '../../services/admin.service';
import { AdminUserDetail } from '../../models/admin.model';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [RouterLink, FormsModule, LucideAngularModule],
  templateUrl: './user-detail.component.html'
})
export class UserDetailComponent implements OnInit {
  readonly icons = { ArrowLeft, Save, FileText, CreditCard };
  readonly plans = ['Free', 'Pro', 'Enterprise'];

  user = signal<AdminUserDetail | null>(null);
  loading = signal(true);
  selectedPlan = '';
  tokenAmount = 0;
  message = signal<string | null>(null);

  constructor(
    private route: ActivatedRoute,
    private adminService: AdminService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.adminService.getUser(id).subscribe({
      next: (u) => {
        this.user.set(u);
        this.selectedPlan = u.plan;
        this.tokenAmount = u.tokensRemaining;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  updatePlan(): void {
    const u = this.user();
    if (!u) return;
    this.adminService.updateUserPlan(u.id, this.selectedPlan).subscribe({
      next: () => {
        this.user.update(usr => usr ? { ...usr, plan: this.selectedPlan } : usr);
        this.flash('Plan updated successfully');
      }
    });
  }

  updateTokens(): void {
    const u = this.user();
    if (!u) return;
    this.adminService.setUserTokens(u.id, this.tokenAmount).subscribe({
      next: () => {
        this.user.update(usr => usr ? { ...usr, tokensRemaining: this.tokenAmount } : usr);
        this.flash('Tokens updated successfully');
      }
    });
  }

  toggleActive(): void {
    const u = this.user();
    if (!u) return;
    this.adminService.toggleUserActive(u.id).subscribe({
      next: (res) => {
        this.user.update(usr => usr ? { ...usr, isActive: res.isActive } : usr);
        this.flash(res.isActive ? 'User activated' : 'User deactivated');
      }
    });
  }

  formatDate(date: string | null | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleString();
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1048576) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / 1048576).toFixed(1) + ' MB';
  }

  private flash(msg: string): void {
    this.message.set(msg);
    setTimeout(() => this.message.set(null), 3000);
  }
}
