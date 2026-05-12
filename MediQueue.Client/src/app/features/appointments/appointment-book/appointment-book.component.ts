import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { BookAppointmentCommand } from '../../../core/api/mediqueue-api';
import {
  AppointmentsClient, DoctorsClient, PatientsClient,
  AppointmentType, AppointmentPriority,
} from '../../../core/api/api-facade.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { pageEnter } from '../../../shared/animations/page-animations';

@Component({
  selector: 'app-appointment-book',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent],
  animations: [pageEnter],
  template: `
    <app-page-header title="Book Appointment" subtitle="Schedule a new patient visit">
      <button class="btn-secondary" (click)="router.navigate(['/appointments'])">Cancel</button>
    </app-page-header>

    <div class="form-card" @pageEnter>
      <form (ngSubmit)="onSubmit()" novalidate>
        <div class="form-grid">
          <div class="field-group">
            <label class="field-label" for="patientSearch">Patient Name or MRN *</label>
            <input id="patientSearch" type="text" class="field-input"
              [(ngModel)]="form.patientId" name="patientId"
              required placeholder="Type patient ID or search…" />
          </div>

          <div class="field-group">
            <label class="field-label" for="doctor">Doctor *</label>
            <select id="doctor" class="field-input" [(ngModel)]="form.doctorId" name="doctorId" required>
              <option value="">— Select Doctor —</option>
              @for (doc of doctors(); track doc.id) {
                <option [value]="doc.id">Dr. {{ doc.firstName }} {{ doc.lastName }} — {{ doc.specialization }}</option>
              }
            </select>
          </div>

          <div class="field-group">
            <label class="field-label" for="scheduledAt">Date & Time *</label>
            <input id="scheduledAt" type="datetime-local" class="field-input"
              [(ngModel)]="form.scheduledAt" name="scheduledAt" required />
          </div>

          <div class="field-group">
            <label class="field-label" for="type">Appointment Type</label>
            <select id="type" class="field-input" [(ngModel)]="form.type" name="type">
              @for (t of typeOptions; track t) {
                <option [value]="t">{{ t }}</option>
              }
            </select>
          </div>

          <div class="field-group">
            <label class="field-label" for="priority">Priority</label>
            <select id="priority" class="field-input" [(ngModel)]="form.priority" name="priority">
              @for (p of priorityOptions; track p) {
                <option [value]="p">{{ p }}</option>
              }
            </select>
          </div>

          <div class="field-group field-group--full">
            <label class="field-label" for="reason">Reason for Visit</label>
            <textarea id="reason" class="field-input field-textarea"
              [(ngModel)]="form.reason" name="reason"
              placeholder="Describe the reason for this appointment…" rows="3"></textarea>
          </div>
        </div>

        <div class="form-actions">
          <button type="button" class="btn-secondary" (click)="router.navigate(['/appointments'])">Cancel</button>
          <button type="submit" id="btn-book" class="btn-primary" [disabled]="isLoading()">
            @if (isLoading()) { <span class="spinner"></span> Booking… }
            @else { Book Appointment }
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .form-card {
      background: var(--color-surface); border: 1px solid var(--color-border);
      border-radius: var(--radius-xl); padding: var(--space-8); box-shadow: var(--shadow-sm);
    }
    .form-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: var(--space-5); }
    .field-group { display: flex; flex-direction: column; }
    .field-group--full { grid-column: 1 / -1; }
    .field-label { font-size: var(--text-sm); font-weight: 500; color: var(--color-text-primary); margin-bottom: var(--space-2); }
    .field-input {
      padding: var(--space-3) var(--space-4); border: 1px solid var(--color-border-strong);
      border-radius: var(--radius-md); font-size: var(--text-sm); font-family: var(--font-family);
      color: var(--color-text-primary); background: var(--color-surface); outline: none;
      transition: border-color var(--duration-fast);
    }
    .field-input:focus { border-color: var(--color-accent); box-shadow: 0 0 0 3px var(--color-accent-light); }
    .field-textarea { resize: vertical; }
    .form-actions { display: flex; justify-content: flex-end; gap: var(--space-3); padding-top: var(--space-6); border-top: 1px solid var(--color-border); margin-top: var(--space-6); }
    .btn-primary {
      display: inline-flex; align-items: center; gap: var(--space-2);
      background: var(--color-accent); color: white; border: none;
      border-radius: var(--radius-md); padding: var(--space-3) var(--space-6);
      font-size: var(--text-sm); font-weight: 600; cursor: pointer; font-family: var(--font-family);
    }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-secondary {
      background: var(--color-surface-2); color: var(--color-text-primary);
      border: 1px solid var(--color-border); border-radius: var(--radius-md);
      padding: var(--space-3) var(--space-5); font-size: var(--text-sm); cursor: pointer; font-family: var(--font-family);
    }
    .spinner { width: 14px; height: 14px; border: 2px solid rgba(255,255,255,0.3); border-top-color: white; border-radius: 50%; animation: spin 0.7s linear infinite; display: inline-block; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
})
export class AppointmentBookComponent implements OnInit {
  private readonly appointmentsClient = inject(AppointmentsClient);
  private readonly doctorsClient = inject(DoctorsClient);
  private readonly notifications = inject(NotificationService);
  readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  doctors = signal<any[]>([]);
  isLoading = signal(false);
  typeOptions = Object.values(AppointmentType);
  priorityOptions = Object.values(AppointmentPriority);

  form = {
    patientId: '',
    doctorId: '',
    scheduledAt: '',
    type: AppointmentType.Regular as AppointmentType,
    priority: AppointmentPriority.Normal as AppointmentPriority,
    reason: '',
  };

  async ngOnInit() {
    const patientId = this.route.snapshot.queryParamMap.get('patientId');
    if (patientId) this.form.patientId = patientId;
    const result = await this.doctorsClient.getAll();
    this.doctors.set(result ?? []);
  }

  async onSubmit() {
    this.isLoading.set(true);
    try {
      const command = new BookAppointmentCommand({
        patientId: this.form.patientId,
        doctorId: this.form.doctorId,
        scheduledAt: new Date(this.form.scheduledAt),
      });
      const result = await this.appointmentsClient.book(command);
      this.notifications.success('Appointment booked successfully!');
      this.router.navigate(['/appointments', result.id]);
    } catch (err: any) {
      this.notifications.error(err?.error?.detail ?? 'Booking failed.');
    } finally {
      this.isLoading.set(false);
    }
  }
}
