import { Component, OnInit, signal, computed } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Search, Eye, UserCheck, UserX } from 'lucide-angular';
import { AdminService } from '../../services/admin.service';
import { AdminUser } from '../../models/admin.model';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [DecimalPipe, RouterLink, FormsModule, LucideAngularModule],
  templateUrl: './users.component.html'
})
export class UsersComponent implements OnInit {
  readonly icons = { Search, Eye, UserCheck, UserX };

  users = signal<AdminUser[]>([]);
  search = signal('');
  loading = signal(true);

  filtered = computed(() => {
    const q = this.search().toLowerCase();
    return this.users().filter(u =>
      u.email.toLowerCase().includes(q) ||
      (u.fullName?.toLowerCase().includes(q) ?? false)
    );
  });

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.adminService.getUsers().subscribe({
      next: (u) => { this.users.set(u); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  toggleActive(user: AdminUser): void {
    this.adminService.toggleUserActive(user.id).subscribe({
      next: (res) => {
        this.users.update(users =>
          users.map(u => u.id === user.id ? { ...u, isActive: res.isActive } : u)
        );
      }
    });
  }

  planBadgeClass(plan: string): string {
    switch (plan) {
      case 'Pro': return 'bg-blue-100 text-blue-700';
      case 'Enterprise': return 'bg-purple-100 text-purple-700';
      default: return 'bg-slate-100 text-slate-600';
    }
  }

  formatDate(date: string | null): string {
    if (!date) return 'Never';
    return new Date(date).toLocaleDateString();
  }
}
