import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DashboardClient, ClinicStatsDto } from '../../core/api/api-facade.service';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';
import { CurrencyEgpPipe } from '../../shared/pipes/currency-egp.pipe';
import { pageEnter } from '../../shared/animations/page-animations';

interface MetricCard {
  label: string;
  value: string | number;
  change: string;
  positive: boolean;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, LoadingSkeletonComponent, CurrencyEgpPipe],
  animations: [pageEnter],
  template: `
    <app-page-header title="Dashboard" subtitle="Today's clinic overview" [hasActions]="false" />

    @if (isLoading()) {
      <app-loading-skeleton [count]="4" type="metric-card" />
    } @else {
      <div class="metrics-grid" @pageEnter>
        @for (card of metrics(); track card.label) {
          <div class="metric-card" [style.--accent]="card.color">
            <div class="metric-card__header">
              <span class="metric-card__label">{{ card.label }}</span>
              <span class="metric-card__icon" [innerHTML]="card.icon"></span>
            </div>
            <div class="metric-card__value">{{ card.value }}</div>
            <div class="metric-card__change" [class.positive]="card.positive" [class.negative]="!card.positive">
              {{ card.change }}
            </div>
          </div>
        }
      </div>

      <!-- Quick Actions -->
      <div class="quick-actions" @pageEnter>
        <h2 class="section-title">Quick Actions</h2>
        <div class="actions-grid">
          <button class="action-card" (click)="navigate('/patients/register')">
            <span class="action-card__icon">➕</span>
            <span class="action-card__label">Register Patient</span>
          </button>
          <button class="action-card" (click)="navigate('/appointments/book')">
            <span class="action-card__icon">📅</span>
            <span class="action-card__label">Book Appointment</span>
          </button>
          <button class="action-card" (click)="navigate('/patients')">
            <span class="action-card__icon">👥</span>
            <span class="action-card__label">View Patients</span>
          </button>
          <button class="action-card" (click)="navigate('/invoices')">
            <span class="action-card__icon">💰</span>
            <span class="action-card__label">Manage Invoices</span>
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .metrics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: var(--space-5);
      margin-bottom: var(--space-8);
    }
    .metric-card {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      padding: var(--space-5);
      box-shadow: var(--shadow-sm);
      transition: box-shadow var(--duration-base) var(--ease-smooth), transform var(--duration-base);
    }
    .metric-card:hover { box-shadow: var(--shadow-md); transform: translateY(-2px); }
    .metric-card__header {
      display: flex; align-items: center; justify-content: space-between;
      margin-bottom: var(--space-3);
    }
    .metric-card__label { font-size: var(--text-sm); color: var(--color-text-secondary); font-weight: 500; }
    .metric-card__icon { font-size: 20px; }
    .metric-card__value {
      font-size: var(--text-3xl); font-weight: 700;
      color: var(--color-text-primary); letter-spacing: -1px;
      margin-bottom: var(--space-2);
    }
    .metric-card__change { font-size: var(--text-xs); font-weight: 500; }
    .metric-card__change.positive { color: var(--color-success); }
    .metric-card__change.negative { color: var(--color-text-tertiary); }

    .quick-actions { margin-top: var(--space-4); }
    .section-title {
      font-size: var(--text-lg); font-weight: 600;
      color: var(--color-text-primary); margin-bottom: var(--space-4);
      letter-spacing: -0.3px;
    }
    .actions-grid {
      display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
      gap: var(--space-4);
    }
    .action-card {
      display: flex; flex-direction: column; align-items: center;
      gap: var(--space-3); padding: var(--space-6);
      background: var(--color-surface); border: 1px solid var(--color-border);
      border-radius: var(--radius-lg); cursor: pointer;
      transition: all var(--duration-base) var(--ease-spring);
      font-family: var(--font-family); font-size: var(--text-sm);
    }
    .action-card:hover {
      border-color: var(--color-accent); background: var(--color-accent-light);
      transform: translateY(-3px); box-shadow: var(--shadow-md);
    }
    .action-card__icon { font-size: 28px; }
    .action-card__label { font-weight: 500; color: var(--color-text-primary); }
  `],
})
export class DashboardComponent implements OnInit {
  private readonly dashboardClient = inject(DashboardClient);
  private readonly router = inject(Router);

  isLoading = signal(true);
  stats = signal<ClinicStatsDto | null>(null);

  metrics = computed((): MetricCard[] => {
    const s = this.stats();
    if (!s) return [];
    return [
      {
        label: 'Total Patients',
        value: s.totalPatients ?? 0,
        change: '↑ vs yesterday',
        positive: true,
        icon: `<span>🏥</span>`,
        color: '#0066CC',
      },
      {
        label: 'Monthly Revenue',
        value: `EGP ${(s.revenueMonthToDate ?? 0).toLocaleString()}`,
        change: 'Collected today',
        positive: true,
        icon: `<span>💵</span>`,
        color: '#34C759',
      },
      {
        label: 'Appointments',
        value: s.appointmentsToday ?? 0,
        change: 'Scheduled today',
        positive: false,
        icon: `<span>📋</span>`,
        color: '#FF9500',
      },
      {
        label: 'Active Doctors',
        value: s.totalDoctors ?? 0,
        change: 'On duty now',
        positive: true,
        icon: `<span>👨‍⚕️</span>`,
        color: '#5AC8FA',
      },
    ];
  });

  async ngOnInit() {
    try {
      const result = await this.dashboardClient.getStats();
      this.stats.set(result);
    } catch (err) {
      console.error('Failed to load dashboard stats', err);
    } finally {
      this.isLoading.set(false);
    }
  }

  navigate(path: string) {
    this.router.navigate([path]);
  }
}
