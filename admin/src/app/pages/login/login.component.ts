import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LucideAngularModule, Shield, Mail, Lock, AlertTriangle } from 'lucide-angular';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  readonly icons = { Shield, Mail, Lock, AlertTriangle };

  email = '';
  password = '';
  error = signal<string | null>(null);
  loading = signal(false);

  constructor(private auth: AuthService, private router: Router) {}

  login(): void {
    if (!this.email || !this.password) return;
    this.loading.set(true);
    this.error.set(null);

    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.token) {
          if (!this.auth.isAdmin()) {
            this.error.set('Access denied. Admin privileges required.');
            this.auth.logout();
            return;
          }
          this.router.navigate(['/']);
        } else {
          this.error.set(res.message || 'Login failed');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Login failed. Please try again.');
      }
    });
  }
}
