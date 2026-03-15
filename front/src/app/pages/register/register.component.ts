import { Component, signal, ViewChildren, QueryList, ElementRef, AfterViewInit } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Shield, Mail, Lock, CheckCircle, AlertTriangle, ArrowLeft } from 'lucide-angular';
import { AuthService } from '../../services/auth.service';

type Step = 'register' | 'verify';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [RouterLink, FormsModule, LucideAngularModule],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  readonly icons = { Shield, Mail, Lock, CheckCircle, AlertTriangle, ArrowLeft };

  step = signal<Step>('register');
  email = '';
  password = '';
  confirmPassword = '';
  registeredEmail = '';
  digits: string[] = ['', '', '', '', '', ''];
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  loading = signal(false);
  resending = signal(false);

  @ViewChildren('digitInput') digitInputs!: QueryList<ElementRef<HTMLInputElement>>;

  constructor(private auth: AuthService, private router: Router) {}

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
          this.registeredEmail = this.email;
          this.step.set('verify');
          this.error.set(null);
          setTimeout(() => this.focusDigit(0), 50);
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

  onDigitInput(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.replace(/\D/g, '');

    if (value.length > 1) {
      const chars = value.split('');
      chars.forEach((ch, i) => {
        if (index + i < 6) this.digits[index + i] = ch;
      });
      this.focusDigit(Math.min(index + chars.length, 5));
      return;
    }

    this.digits[index] = value;

    if (value && index < 5) {
      this.focusDigit(index + 1);
    }

    if (this.digits.every(d => d.length === 1)) {
      this.verifyCode();
    }
  }

  onDigitKeydown(index: number, event: KeyboardEvent): void {
    if (event.key === 'Backspace' && !this.digits[index] && index > 0) {
      this.digits[index - 1] = '';
      this.focusDigit(index - 1);
    }
  }

  onDigitPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const pasted = (event.clipboardData?.getData('text') ?? '').replace(/\D/g, '').slice(0, 6);
    pasted.split('').forEach((ch, i) => {
      if (i < 6) this.digits[i] = ch;
    });
    this.focusDigit(Math.min(pasted.length, 5));
    if (this.digits.every(d => d.length === 1)) {
      this.verifyCode();
    }
  }

  verifyCode(): void {
    const code = this.digits.join('');
    if (code.length !== 6) {
      this.error.set('Please enter the full 6-digit code');
      return;
    }

    this.error.set(null);
    this.loading.set(true);

    this.auth.verifyCode({ email: this.registeredEmail, code }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.router.navigate(['/']);
        } else {
          this.error.set(res.message ?? 'Verification failed');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message ?? 'Invalid or expired code. Please try again.');
      }
    });
  }

  resendCode(): void {
    this.error.set(null);
    this.success.set(null);
    this.resending.set(true);

    this.auth.resendCode(this.registeredEmail).subscribe({
      next: (res) => {
        this.resending.set(false);
        if (res.success) {
          this.success.set('A new code has been sent to your email.');
          this.digits = ['', '', '', '', '', ''];
          setTimeout(() => this.focusDigit(0), 50);
        } else {
          this.error.set(res.message ?? 'Could not resend code');
        }
      },
      error: (err) => {
        this.resending.set(false);
        this.error.set(err.error?.message ?? 'Failed to resend code. Please try again.');
      }
    });
  }

  goBackToRegister(): void {
    this.step.set('register');
    this.error.set(null);
    this.success.set(null);
    this.digits = ['', '', '', '', '', ''];
  }

  private focusDigit(index: number): void {
    const inputs = this.digitInputs?.toArray();
    if (inputs && inputs[index]) {
      inputs[index].nativeElement.focus();
    }
  }
}
