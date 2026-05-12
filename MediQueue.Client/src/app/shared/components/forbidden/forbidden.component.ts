import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="forbidden-container">
      <div class="content" @fadeSlideIn>
        <div class="icon">🚫</div>
        <h1>Access Denied</h1>
        <p>You don't have the required permissions to view this page.</p>
        <div class="actions">
          <button (click)="goHome()" class="btn btn-primary">Go to My Home</button>
          <button (click)="logout()" class="btn btn-ghost">Sign Out</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .forbidden-container {
      height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--color-bg);
      text-align: center;
      padding: var(--space-6);
    }
    .content {
      max-width: 400px;
      padding: var(--space-10);
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-2xl);
      box-shadow: var(--shadow-xl);
    }
    .icon {
      font-size: 64px;
      margin-bottom: var(--space-6);
    }
    h1 {
      font-size: var(--text-2xl);
      font-weight: 700;
      color: var(--color-text-primary);
      margin-bottom: var(--space-3);
    }
    p {
      color: var(--color-text-secondary);
      margin-bottom: var(--space-8);
      line-height: 1.6;
    }
    .actions {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
    }
  `]
})
export class ForbiddenComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  goHome() {
    const role = this.authService.currentUser()?.role;
    const redirectMap: any = {
      'Admin': '/dashboard',
      'Doctor': '/clinical-visits',
      'Receptionist': '/appointments',
    };
    this.router.navigate([redirectMap[role!] ?? '/']);
  }

  logout() {
    this.authService.logout();
  }
}
