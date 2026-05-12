import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ClinicalVisitsClient, ClinicalVisitSummaryDto } from '../../../core/api/api-facade.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { pageEnter } from '../../../shared/animations/page-animations';

@Component({
  selector: 'app-visit-list',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, LoadingSkeletonComponent, BadgeComponent, EmptyStateComponent],
  animations: [pageEnter],
  template: `
    <app-page-header title="Clinical Visits" subtitle="My patient visits" [hasActions]="false" />

    @if (isLoading()) {
      <app-loading-skeleton [count]="4" />
    } @else if (visits().length === 0) {
      <app-empty-state title="No visits assigned" message="Visits are created when appointments are started." />
    } @else {
      <div class="visits-list" @pageEnter>
        @for (visit of visits(); track visit.id) {
          <div class="visit-card" (click)="open(visit.id!)">
            <div class="visit-card__date">
              <span class="day">{{ visit.startedAt | date:'d' }}</span>
              <span class="month">{{ visit.startedAt | date:'MMM' }}</span>
            </div>
            <div class="visit-card__info">
              <div class="visit-patient">{{ visit.patientName }}</div>
              <div class="visit-reason">{{ visit.chiefComplaint ?? 'General Consultation' }}</div>
            </div>
            <app-badge [label]="visit.status ?? 'InProgress'" [variant]="statusVariant(visit.status)" />
          </div>
        }
      </div>
    }
  `,
  styles: [`
    .visits-list { display: flex; flex-direction: column; gap: var(--space-3); }
    .visit-card {
      display: flex; align-items: center; gap: var(--space-5);
      background: var(--color-surface); border: 1px solid var(--color-border);
      border-radius: var(--radius-lg); padding: var(--space-4) var(--space-5);
      cursor: pointer; transition: all var(--duration-fast); box-shadow: var(--shadow-sm);
    }
    .visit-card:hover { border-color: var(--color-accent); box-shadow: var(--shadow-md); }
    .visit-card__date {
      display: flex; flex-direction: column; align-items: center;
      width: 44px; padding: var(--space-2); background: var(--color-accent-light);
      border-radius: var(--radius-md); flex-shrink: 0;
    }
    .day { font-size: var(--text-xl); font-weight: 700; color: var(--color-accent); line-height: 1; }
    .month { font-size: var(--text-xs); color: var(--color-accent); font-weight: 600; text-transform: uppercase; }
    .visit-card__info { flex: 1; }
    .visit-patient { font-weight: 600; font-size: var(--text-base); color: var(--color-text-primary); }
    .visit-reason { font-size: var(--text-sm); color: var(--color-text-secondary); margin-top: 2px; }
  `],
})
export class VisitListComponent implements OnInit {
  private readonly visitsClient = inject(ClinicalVisitsClient);
  readonly router = inject(Router);

  visits = signal<ClinicalVisitSummaryDto[]>([]);
  isLoading = signal(true);

  async ngOnInit() {
    try {
      const result = await this.visitsClient.getMyVisits();
      this.visits.set(result ?? []);
    } finally {
      this.isLoading.set(false);
    }
  }

  open(id: string) { this.router.navigate(['/clinical-visits', id]); }

  statusVariant(status: string | undefined): 'success' | 'warning' | 'danger' | 'info' | 'default' {
    const map: Record<string, any> = {
      'Finalized': 'success', 'InProgress': 'info', 'Cancelled': 'danger',
    };
    return map[status ?? ''] ?? 'default';
  }
}
