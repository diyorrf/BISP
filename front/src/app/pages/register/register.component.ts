import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Shield, Mail, Lock, CheckCircle, AlertTriangle } from 'lucide-angular';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [RouterLink, FormsModule, LucideAngularModule],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  readonly icons = { Shield, Mail, Lock, CheckCircle, AlertTriangle };

  email = '';
  password = '';
  confirmPassword = '';
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  loading = signal(false);

  constructor(private auth: AuthService) {}

  onSubmit(): void {
    this.error.set(null);
    this.success.set(null);

    if (this.password !== this.confirmPassword) {
      this.error.set('Passwords do not match');
      return;
    }

    if (this.password.length < 6) {
      this.error.set('Password must be at least 6 characters');
      return;
    }

    this.loading.set(true);

    this.auth.register({ email: this.email, password: this.password }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.success.set(res.message ?? 'Registration successful. Please check your email to confirm your account.');
        } else {
          this.error.set(res.message ?? 'Registration failed');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message ?? 'Registration failed. Please try again.');
      }
    });
  }
}
