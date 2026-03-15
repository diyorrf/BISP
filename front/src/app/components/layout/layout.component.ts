import { Component, signal, OnInit, OnDestroy } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, Shield, FileText, MessageSquare, CheckCircle, FolderOpen, Menu, X, LogOut, User, Zap, Crown, Bell, CreditCard } from 'lucide-angular';
import { AuthService } from '../../services/auth.service';
import { AccountService } from '../../services/account.service';
import { AlertService } from '../../services/alert.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LucideAngularModule, DecimalPipe],
  templateUrl: './layout.component.html'
})
export class LayoutComponent implements OnInit, OnDestroy {
  readonly icons = { Shield, FileText, MessageSquare, CheckCircle, FolderOpen, Menu, X, LogOut, User, Zap, Crown, Bell, CreditCard };
  mobileMenuOpen = signal(false);
  accountDropdownOpen = signal(false);

  navigation = [
    { name: 'Dashboard', path: '/', icon: Shield },
    { name: 'Contract Scanner', path: '/scanner', icon: FileText },
    { name: 'AI Assistant', path: '/chat', icon: MessageSquare },
    { name: 'Business Checker', path: '/business-checker', icon: CheckCircle },
    { name: 'Documents', path: '/documents', icon: FolderOpen },
    { name: 'Alerts', path: '/alerts', icon: Bell },
  ];

  constructor(
    public auth: AuthService,
    public account: AccountService,
    public alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.account.load().subscribe();
    this.alertService.startPolling();
  }

  ngOnDestroy(): void {
    this.alertService.stopPolling();
  }

  toggleMobile(): void {
    this.mobileMenuOpen.update(v => !v);
  }

  closeMobile(): void {
    this.mobileMenuOpen.set(false);
  }

  toggleAccountDropdown(): void {
    this.accountDropdownOpen.update(v => !v);
  }

  closeAccountDropdown(): void {
    this.accountDropdownOpen.set(false);
  }

  logout(): void {
    this.account.clear();
    this.auth.logout();
    this.closeAccountDropdown();
    this.closeMobile();
  }

  planLabel(plan: string): string {
    return plan === 'Pro' ? 'Pro' : plan === 'Enterprise' ? 'Enterprise' : 'Free';
  }
}
