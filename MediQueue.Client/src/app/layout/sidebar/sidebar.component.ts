import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

interface NavItem {
  label: string;
  path: string;
  roles: string[];
  icon: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <aside class="sidebar">
      <!-- Brand -->
      <div class="sidebar__brand">
        <div class="sidebar__logo">
          <svg width="28" height="28" viewBox="0 0 40 40" fill="none">
            <rect width="40" height="40" rx="10" fill="var(--color-accent)"/>
            <path d="M20 8C14 8 10 13 10 20C10 27 14 32 20 32C26 32 30 27 30 20"
                  stroke="white" stroke-width="2.5" stroke-linecap="round"/>
            <circle cx="28" cy="12" r="3" fill="white"/>
          </svg>
        </div>
        <span class="sidebar__brand-name">MediQueue</span>
      </div>

      <!-- Nav Items -->
      <nav class="sidebar__nav">
        @for (item of visibleItems(); track item.path) {
          <a
            [routerLink]="item.path"
            routerLinkActive="sidebar__nav-item--active"
            class="sidebar__nav-item"
          >
            <span class="sidebar__nav-icon" [innerHTML]="item.icon"></span>
            <span class="sidebar__nav-label">{{ item.label }}</span>
          </a>
        }
      </nav>

      <!-- User info -->
      <div class="sidebar__footer">
        <div class="sidebar__user">
          <div class="sidebar__avatar">
            {{ authService.currentUser()?.email?.charAt(0)?.toUpperCase() }}
          </div>
          <div class="sidebar__user-info">
            <span class="sidebar__user-email">{{ authService.currentUser()?.email }}</span>
            <span class="sidebar__user-role">{{ authService.currentUser()?.role }}</span>
          </div>
        </div>
        <button class="sidebar__logout" (click)="authService.logout()" title="Sign out">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4"/>
            <polyline points="16 17 21 12 16 7"/>
            <line x1="21" y1="12" x2="9" y2="12"/>
          </svg>
        </button>
      </div>
    </aside>
  `,
  styles: [`
    .sidebar {
      width: 240px;
      min-height: 100vh;
      background: var(--color-surface);
      border-right: 1px solid var(--color-border);
      display: flex;
      flex-direction: column;
      padding: var(--space-5) 0;
      position: sticky;
      top: 0;
    }
    .sidebar__brand {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: 0 var(--space-5) var(--space-5);
      border-bottom: 1px solid var(--color-border);
      margin-bottom: var(--space-4);
    }
    .sidebar__brand-name {
      font-size: var(--text-md);
      font-weight: 700;
      color: var(--color-text-primary);
      letter-spacing: -0.3px;
    }
    .sidebar__nav {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 2px;
      padding: 0 var(--space-3);
    }
    .sidebar__nav-item {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-3) var(--space-3);
      border-radius: var(--radius-md);
      color: var(--color-text-secondary);
      text-decoration: none;
      font-size: var(--text-sm);
      font-weight: 500;
      transition: all var(--duration-fast) var(--ease-smooth);
    }
    .sidebar__nav-item:hover {
      background: var(--color-surface-2);
      color: var(--color-text-primary);
    }
    .sidebar__nav-item--active {
      background: var(--color-accent-light);
      color: var(--color-accent);
      font-weight: 600;
    }
    .sidebar__nav-icon { display: flex; align-items: center; width: 18px; }
    .sidebar__nav-icon ::ng-deep svg { width: 18px; height: 18px; }
    .sidebar__footer {
      padding: var(--space-4) var(--space-4) 0;
      border-top: 1px solid var(--color-border);
      display: flex;
      align-items: center;
      gap: var(--space-2);
    }
    .sidebar__user { flex: 1; display: flex; align-items: center; gap: var(--space-3); min-width: 0; }
    .sidebar__avatar {
      width: 32px; height: 32px;
      border-radius: var(--radius-full);
      background: var(--color-accent);
      color: white;
      display: flex; align-items: center; justify-content: center;
      font-size: var(--text-sm); font-weight: 700;
      flex-shrink: 0;
    }
    .sidebar__user-info { display: flex; flex-direction: column; min-width: 0; }
    .sidebar__user-email {
      font-size: var(--text-xs); font-weight: 500;
      color: var(--color-text-primary);
      white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
    }
    .sidebar__user-role {
      font-size: 10px; color: var(--color-text-tertiary);
    }
    .sidebar__logout {
      background: none; border: none; cursor: pointer;
      color: var(--color-text-tertiary); padding: var(--space-2);
      border-radius: var(--radius-sm);
      transition: color var(--duration-fast);
    }
    .sidebar__logout:hover { color: var(--color-danger); }
  `],
})
export class SidebarComponent {
  readonly authService = inject(AuthService);

  private readonly navItems: NavItem[] = [
    {
      label: 'Dashboard', path: '/dashboard', roles: ['Admin'],
      icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>`,
    },
    {
      label: 'Patients', path: '/patients', roles: ['Admin', 'Receptionist'],
      icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 00-3-3.87"/><path d="M16 3.13a4 4 0 010 7.75"/></svg>`,
    },
    {
      label: 'Doctors', path: '/doctors', roles: ['Admin'],
      icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>`,
    },
    {
      label: 'Appointments', path: '/appointments', roles: ['Admin', 'Receptionist'],
      icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>`,
    },
    {
      label: 'Clinical Visits', path: '/clinical-visits', roles: ['Doctor'],
      icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="12" y1="18" x2="12" y2="12"/><line x1="9" y1="15" x2="15" y2="15"/></svg>`,
    },
    {
      label: 'Invoices', path: '/invoices', roles: ['Admin', 'Receptionist'],
      icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 000 7h5a3.5 3.5 0 010 7H6"/></svg>`,
    },
  ];

  visibleItems() {
    return this.navItems.filter(item =>
      this.authService.hasRole(...item.roles)
    );
  }
}
