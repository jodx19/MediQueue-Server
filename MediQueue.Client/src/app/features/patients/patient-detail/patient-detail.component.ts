import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PatientsClient, PatientDetailDto } from '../../../core/api/api-facade.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';
import { MrnPipe } from '../../../shared/pipes/mrn.pipe';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { pageEnter } from '../../../shared/animations/page-animations';

@Component({
  selector: 'app-patient-detail',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, LoadingSkeletonComponent, MrnPipe, BadgeComponent],
  animations: [pageEnter],
  template: `
    <app-page-header
      [title]="patient()?.fullName ?? 'Patient'"
      [subtitle]="patient() ? (patient()!.medicalRecordNumber | mrn) : ''"
    >
      <button class="btn-secondary" (click)="goBack()">← Back</button>
      <button class="btn-primary" (click)="bookAppointment()">Book Appointment</button>
    </app-page-header>

    @if (isLoading()) {
      <app-loading-skeleton [count]="4" />
    } @else if (patient()) {
      <div class="detail-grid" @pageEnter>
        <!-- Info Card -->
        <div class="info-card">
          <h3 class="card-title">Personal Information</h3>
          <div class="info-rows">
            <div class="info-row">
              <span class="info-label">Full Name</span>
              <span class="info-value">{{ patient()!.fullName }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Date of Birth</span>
              <span class="info-value">{{ patient()!.dateOfBirth | date:'longDate' }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Gender</span>
              <span class="info-value">{{ patient()!.gender }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Blood Type</span>
              <span class="info-value blood">{{ patient()!.bloodType ?? '—' }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">National ID</span>
              <span class="info-value mono">{{ patient()!.nationalId ?? '—' }}</span>
            </div>
          </div>
        </div>

        <!-- Contact Card -->
        <div class="info-card">
          <h3 class="card-title">Contact</h3>
          <div class="info-rows">
            <div class="info-row">
              <span class="info-label">Phone</span>
              <span class="info-value">{{ patient()!.phone ?? '—' }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Email</span>
              <span class="info-value">{{ patient()!.email ?? '—' }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Address</span>
              <span class="info-value">—</span>
            </div>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .btn-primary {
      display: inline-flex; align-items: center; gap: var(--space-2);
      background: var(--color-accent); color: white; border: none;
      border-radius: var(--radius-md); padding: var(--space-2) var(--space-5);
      font-size: var(--text-sm); font-weight: 600; cursor: pointer;
      font-family: var(--font-family); transition: all var(--duration-fast);
    }
    .btn-primary:hover { background: var(--color-accent-dark); }
    .btn-secondary {
      background: var(--color-surface-2); color: var(--color-text-primary);
      border: 1px solid var(--color-border); border-radius: var(--radius-md);
      padding: var(--space-2) var(--space-4); font-size: var(--text-sm);
      cursor: pointer; font-family: var(--font-family); transition: all var(--duration-fast);
    }
    .detail-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-5); }
    .info-card {
      background: var(--color-surface); border: 1px solid var(--color-border);
      border-radius: var(--radius-lg); padding: var(--space-6); box-shadow: var(--shadow-sm);
    }
    .card-title {
      font-size: var(--text-base); font-weight: 600; color: var(--color-text-primary);
      margin-bottom: var(--space-4); padding-bottom: var(--space-3);
      border-bottom: 1px solid var(--color-border);
    }
    .info-rows { display: flex; flex-direction: column; gap: var(--space-4); }
    .info-row { display: flex; justify-content: space-between; align-items: baseline; }
    .info-label { font-size: var(--text-sm); color: var(--color-text-secondary); }
    .info-value { font-size: var(--text-sm); font-weight: 500; color: var(--color-text-primary); }
    .info-value.mono { font-family: var(--font-mono); font-size: var(--text-xs); }
    .info-value.blood {
      background: var(--color-danger-bg); color: var(--color-danger);
      padding: 2px 10px; border-radius: var(--radius-full); font-size: var(--text-xs); font-weight: 700;
    }
  `],
})
export class PatientDetailComponent implements OnInit {
  private readonly patientsClient = inject(PatientsClient);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  patient = signal<PatientDetailDto | null>(null);
  isLoading = signal(true);

  async ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    try {
      const result = await this.patientsClient.getById(id);
      this.patient.set(result);
    } finally {
      this.isLoading.set(false);
    }
  }

  goBack() { this.router.navigate(['/patients']); }
  bookAppointment() { this.router.navigate(['/appointments/book'], { queryParams: { patientId: this.patient()?.id } }); }
}
