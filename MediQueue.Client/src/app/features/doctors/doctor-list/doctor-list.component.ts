import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DoctorsClient, DoctorSummaryDto } from '../../../core/api/api-facade.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { pageEnter, listStagger } from '../../../shared/animations/page-animations';

@Component({
  selector: 'app-doctor-list',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, LoadingSkeletonComponent, EmptyStateComponent],
  animations: [pageEnter, listStagger],
  template: `
    <app-page-header title="Doctors" subtitle="Medical staff management" [hasActions]="false" />

    @if (isLoading()) {
      <app-loading-skeleton [count]="4" />
    } @else if (doctors().length === 0) {
      <app-empty-state title="No doctors registered" message="Doctors are added via the admin panel." />
    } @else {
      <div class="doctors-grid" [@listStagger]="doctors().length" @pageEnter>
        @for (doc of doctors(); track doc.id) {
          <div class="doctor-card" (click)="openDoctor(doc.id!)" tabindex="0">
            <div class="doctor-card__avatar">
              {{ doc.firstName?.charAt(0) }}{{ doc.lastName?.charAt(0) }}
            </div>
            <div class="doctor-card__info">
              <h3 class="doctor-card__name">Dr. {{ doc.firstName }} {{ doc.lastName }}</h3>
              <p class="doctor-card__specialty">{{ doc.specialization ?? 'General Practice' }}</p>
              <p class="doctor-card__contact">{{ doc.email }}</p>
            </div>
            <div class="doctor-card__arrow">›</div>
          </div>
        }
      </div>
    }
  `,
  styles: [`
    .doctors-grid { display: flex; flex-direction: column; gap: var(--space-3); }
    .doctor-card {
      display: flex; align-items: center; gap: var(--space-4);
      background: var(--color-surface); border: 1px solid var(--color-border);
      border-radius: var(--radius-lg); padding: var(--space-5);
      cursor: pointer; transition: all var(--duration-base);
      box-shadow: var(--shadow-sm);
    }
    .doctor-card:hover { border-color: var(--color-accent); box-shadow: var(--shadow-md); transform: translateX(4px); }
    .doctor-card__avatar {
      width: 48px; height: 48px; border-radius: var(--radius-full);
      background: var(--color-accent); color: white;
      display: flex; align-items: center; justify-content: center;
      font-size: var(--text-md); font-weight: 700; flex-shrink: 0;
    }
    .doctor-card__info { flex: 1; }
    .doctor-card__name { font-size: var(--text-base); font-weight: 600; color: var(--color-text-primary); }
    .doctor-card__specialty { font-size: var(--text-sm); color: var(--color-accent); margin-top: 2px; }
    .doctor-card__contact { font-size: var(--text-xs); color: var(--color-text-tertiary); margin-top: 2px; }
    .doctor-card__arrow { font-size: 22px; color: var(--color-text-tertiary); }
  `],
})
export class DoctorListComponent implements OnInit {
  private readonly doctorsClient = inject(DoctorsClient);
  private readonly router = inject(Router);

  doctors = signal<DoctorSummaryDto[]>([]);
  isLoading = signal(true);

  async ngOnInit() {
    try {
      const result = await this.doctorsClient.getAll();
      this.doctors.set(result ?? []);
    } finally {
      this.isLoading.set(false);
    }
  }

  openDoctor(id: string) {
    this.router.navigate(['/doctors', id]);
  }
}
