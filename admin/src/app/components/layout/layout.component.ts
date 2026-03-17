import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, LayoutDashboard, Users, FileText, Bell, CreditCard, Shield, LogOut } from 'lucide-angular';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './layout.component.html'
})
export class LayoutComponent {
  readonly icons = { LayoutDashboard, Users, FileText, Bell, CreditCard, Shield, LogOut };

  navItems = [
    { label: 'Dashboard', route: '/', icon: this.icons.LayoutDashboard, exact: true },
    { label: 'Users', route: '/users', icon: this.icons.Users, exact: false },
    { label: 'Documents', route: '/documents', icon: this.icons.FileText, exact: false },
    { label: 'Regulatory', route: '/regulatory', icon: this.icons.Bell, exact: false },
    { label: 'Payments', route: '/payments', icon: this.icons.CreditCard, exact: false },
  ];

  constructor(private auth: AuthService) {}

  logout(): void {
    this.auth.logout();
  }
}
