import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { fadeSlideIn } from '../../../shared/animations/page-animations';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  animations: [fadeSlideIn],
  template: `
    <div class="login-wrapper">
      <a routerLink="/" class="back-link">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M19 12H5M12 19l-7-7 7-7"/>
        </svg>
        Back to Home
      </a>
      <!-- Background decoration -->
      <div class="login-bg"></div>

      <!-- Brand -->
      <div class="login-brand" @fadeSlideIn>
        <div class="logo-mark">
          <svg width="48" height="48" viewBox="0 0 40 40" fill="none">
            <rect width="40" height="40" rx="10" fill="var(--color-accent)"/>
            <path d="M20 8C14 8 10 13 10 20C10 27 14 32 20 32C26 32 30 27 30 20"
                  stroke="white" stroke-width="2.5" stroke-linecap="round"/>
            <circle cx="28" cy="12" r="3" fill="white"/>
          </svg>
        </div>
        <h1 class="brand-name">MediQueue</h1>
        <p class="brand-subtitle">Electronic Medical Records System</p>
      </div>

      <!-- Card -->
      <div class="login-card" @fadeSlideIn>
        <h2 class="card-title">Sign In</h2>
        <p class="card-subtitle">Staff accounts are managed by your administrator</p>

        @if (errorMessage()) {
          <div class="error-banner" role="alert">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/>
              <line x1="15" y1="9" x2="9" y2="15"/>
              <line x1="9" y1="9" x2="15" y2="15"/>
            </svg>
            {{ errorMessage() }}
          </div>
        }

        <form (ngSubmit)="onSubmit()" #loginForm="ngForm" novalidate>
          <div class="field-group">
            <label class="field-label" for="email">Email Address</label>
            <input
              id="email" type="email" class="field-input"
              placeholder="doctor@mediqueue.com"
              [(ngModel)]="email" name="email"
              required autocomplete="email"
            />
          </div>

          <div class="field-group">
            <label class="field-label" for="password">Password</label>
            <div class="input-wrapper">
              <input
                id="password"
                [type]="showPassword() ? 'text' : 'password'"
                class="field-input with-action"
                placeholder="••••••••"
                [(ngModel)]="password" name="password"
                required autocomplete="current-password"
              />
              <button type="button" class="input-action-btn"
                (click)="showPassword.set(!showPassword())">
                {{ showPassword() ? 'Hide' : 'Show' }}
              </button>
            </div>
          </div>

          <button
            type="submit" id="btn-login" class="btn-primary btn-full"
            [disabled]="isLoading() || !email || !password"
          >
            @if (isLoading()) {
              <span class="spinner"></span>
              Signing in...
            } @else {
              Sign In
            }
          </button>
        </form>

        <div class="login-hint">
          <p>Your account is created by the clinic administrator.</p>
          <div class="patient-redirect">
            Are you a patient? <a routerLink="/register-patient">Register here</a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .login-wrapper {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: var(--space-6);
      position: relative;
      background: var(--color-bg);
    }
    .back-link {
      position: absolute; top: var(--space-6); left: var(--space-6);
      display: inline-flex; align-items: center; gap: var(--space-2);
      color: var(--color-text-secondary); text-decoration: none;
      font-size: var(--text-sm); font-weight: 500; transition: color var(--duration-fast);
      z-index: 10;
    }
    .back-link:hover { color: var(--color-accent); }
    .login-bg {
      position: fixed; inset: 0; z-index: 0;
      background: radial-gradient(ellipse 80% 60% at 50% -20%, rgba(0,102,204,0.08), transparent);
      pointer-events: none;
    }
    .login-brand {
      text-align: center; margin-bottom: var(--space-8);
      position: relative; z-index: 1;
    }
    .logo-mark { margin-bottom: var(--space-4); display: inline-block; }
    .brand-name {
      font-size: var(--text-2xl); font-weight: 700;
      color: var(--color-text-primary); letter-spacing: -0.5px;
    }
    .brand-subtitle {
      font-size: var(--text-sm); color: var(--color-text-secondary);
      margin-top: var(--space-1);
    }
    .login-card {
      width: 100%; max-width: 420px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-xl);
      padding: var(--space-8);
      box-shadow: var(--shadow-lg);
      position: relative; z-index: 1;
    }
    .card-title {
      font-size: var(--text-xl); font-weight: 700;
      color: var(--color-text-primary); letter-spacing: -0.4px;
      margin-bottom: var(--space-1);
    }
    .card-subtitle {
      font-size: var(--text-sm); color: var(--color-text-secondary);
      margin-bottom: var(--space-6);
    }
    .error-banner {
      display: flex; align-items: center; gap: var(--space-2);
      background: var(--color-danger-bg);
      color: var(--color-danger);
      border: 1px solid rgba(255,59,48,0.2);
      border-radius: var(--radius-md);
      padding: var(--space-3) var(--space-4);
      font-size: var(--text-sm);
      margin-bottom: var(--space-5);
    }
    .field-group { margin-bottom: var(--space-4); }
    .field-label {
      display: block; font-size: var(--text-sm); font-weight: 500;
      color: var(--color-text-primary); margin-bottom: var(--space-2);
    }
    .field-input {
      width: 100%; padding: var(--space-3) var(--space-4);
      border: 1px solid var(--color-border-strong);
      border-radius: var(--radius-md);
      font-size: var(--text-base); font-family: var(--font-family);
      background: var(--color-surface); color: var(--color-text-primary);
      outline: none; transition: border-color var(--duration-fast);
      box-sizing: border-box;
    }
    .field-input:focus { border-color: var(--color-accent); box-shadow: 0 0 0 3px var(--color-accent-light); }
    .field-input.with-action { padding-right: 70px; }
    .input-wrapper { position: relative; }
    .input-action-btn {
      position: absolute; right: var(--space-3); top: 50%; transform: translateY(-50%);
      background: none; border: none; cursor: pointer;
      font-size: var(--text-xs); color: var(--color-accent); font-weight: 600;
    }
    .btn-primary {
      display: inline-flex; align-items: center; justify-content: center;
      gap: var(--space-2);
      background: var(--color-accent); color: white;
      border: none; border-radius: var(--radius-md);
      padding: var(--space-3) var(--space-6);
      font-size: var(--text-base); font-weight: 600;
      cursor: pointer; transition: all var(--duration-fast) var(--ease-smooth);
    }
    .btn-primary:hover:not(:disabled) { background: var(--color-accent-dark); transform: translateY(-1px); box-shadow: var(--shadow-md); }
    .btn-primary:active { transform: translateY(0); }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-full { width: 100%; margin-top: var(--space-2); }
    .spinner {
      width: 16px; height: 16px;
      border: 2px solid rgba(255,255,255,0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 0.7s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .login-hint {
      margin-top: var(--space-8);
      text-align: center;
      font-size: var(--text-xs);
      color: var(--color-text-tertiary);
    }
    .patient-redirect {
      margin-top: var(--space-2);
      a {
        color: var(--color-accent);
        text-decoration: none;
        font-weight: 600;
      }
      a:hover { text-decoration: underline; }
    }
  `],
})
export class LoginComponent {
  private readonly authService = inject(AuthService);

  email = '';
  password = '';
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);

  async onSubmit() {
    if (!this.email || !this.password) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);

    try {
      await this.authService.login(this.email, this.password);
    } catch (err: any) {
      const detail = err?.error?.detail ?? 'Invalid email or password.';
      this.errorMessage.set(detail);
    } finally {
      this.isLoading.set(false);
    }
  }
}
