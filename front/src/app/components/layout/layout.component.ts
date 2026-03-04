import { Component, signal, OnInit } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, Shield, FileText, MessageSquare, CheckCircle, FolderOpen, Menu, X, LogOut, User, Zap, Crown } from 'lucide-angular';
import { AuthService } from '../../services/auth.service';
import { AccountService } from '../../services/account.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LucideAngularModule, DecimalPipe],
  templateUrl: './layout.component.html'
})
export class LayoutComponent implements OnInit {
  readonly icons = { Shield, FileText, MessageSquare, CheckCircle, FolderOpen, Menu, X, LogOut, User, Zap, Crown };
  mobileMenuOpen = signal(false);
  accountDropdownOpen = signal(false);

  navigation = [
    { name: 'Dashboard', path: '/', icon: Shield },
    { name: 'Contract Scanner', path: '/scanner', icon: FileText },
    { name: 'AI Assistant', path: '/chat', icon: MessageSquare },
    { name: 'Business Checker', path: '/business-checker', icon: CheckCircle },
    { name: 'Documents', path: '/documents', icon: FolderOpen },
  ];

  constructor(
    public auth: AuthService,
    public account: AccountService
  ) {}

  ngOnInit(): void {
    this.account.load().subscribe();
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
