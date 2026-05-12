import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DoctorsClient, DoctorDto as DoctorDetailDto } from '../../../core/api/api-facade.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';
import { pageEnter } from '../../../shared/animations/page-animations';

@Component({
  selector: 'app-doctor-detail',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, LoadingSkeletonComponent],
  animations: [pageEnter],
  template: `
    <app-page-header
      [title]="doctor() ? 'Dr. ' + doctor()!.fullName : 'Doctor'"
      [subtitle]="doctor()?.specialty ? (doctor()!.specialty + '') : ''"
    >
      <button class="btn-secondary" (click)="router.navigate(['/doctors'])">← Back</button>
    </app-page-header>

    @if (isLoading()) {
      <app-loading-skeleton [count]="3" />
    } @else if (doctor()) {
      <div class="info-card" @pageEnter>
        <div class="doctor-header">
          <div class="doctor-avatar">
            {{ doctor()!.fullName?.charAt(0) }}
          </div>
          <div>
            <h2>Dr. {{ doctor()!.fullName }}</h2>
            <p class="specialty">{{ doctor()!.specialty }}</p>
          </div>
        </div>
        <div class="info-rows">
          <div class="info-row"><span class="info-label">License #</span><span class="mono">{{ doctor()!.licenseNumber ?? '—' }}</span></div>
        </div>
      </div>
    }
  `,
  styles: [`
    .info-card {
      background: var(--color-surface); border: 1px solid var(--color-border);
      border-radius: var(--radius-lg); padding: var(--space-6); box-shadow: var(--shadow-sm);
    }
    .doctor-header { display: flex; align-items: center; gap: var(--space-5); margin-bottom: var(--space-6); }
    .doctor-avatar {
      width: 64px; height: 64px; border-radius: var(--radius-full);
      background: var(--color-accent); color: white;
      display: flex; align-items: center; justify-content: center;
      font-size: var(--text-xl); font-weight: 700;
    }
    h2 { font-size: var(--text-xl); font-weight: 700; color: var(--color-text-primary); }
    .specialty { color: var(--color-accent); font-size: var(--text-sm); }
    .info-rows { display: flex; flex-direction: column; gap: var(--space-4); }
    .info-row { display: flex; justify-content: space-between; align-items: center; padding: var(--space-3) 0; border-bottom: 1px solid var(--color-border); }
    .info-row:last-child { border-bottom: none; }
    .info-label { font-size: var(--text-sm); color: var(--color-text-secondary); }
    .mono { font-family: var(--font-mono); font-size: var(--text-xs); }
    .btn-secondary {
      background: var(--color-surface-2); color: var(--color-text-primary);
      border: 1px solid var(--color-border); border-radius: var(--radius-md);
      padding: var(--space-2) var(--space-4); font-size: var(--text-sm);
      cursor: pointer; font-family: var(--font-family);
    }
  `],
})
export class DoctorDetailComponent implements OnInit {
  private readonly doctorsClient = inject(DoctorsClient);
  private readonly route = inject(ActivatedRoute);
  readonly router = inject(Router);

  doctor = signal<DoctorDetailDto | null>(null);
  isLoading = signal(true);

  async ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    try {
      const result = await this.doctorsClient.getById(id);
      this.doctor.set(result);
    } finally {
      this.isLoading.set(false);
    }
  }
}
