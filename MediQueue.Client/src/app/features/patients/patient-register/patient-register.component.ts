import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { RegisterPatientCommand } from '../../../core/api/mediqueue-api';
import { PatientsClient, GenderType, BloodType } from '../../../core/api/api-facade.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { pageEnter } from '../../../shared/animations/page-animations';

@Component({
  selector: 'app-patient-register',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent],
  animations: [pageEnter],
  template: `
    <app-page-header title="Register Patient" subtitle="Create a new patient record">
      <button class="btn-secondary" (click)="cancel()">Cancel</button>
    </app-page-header>

    <div class="form-card" @pageEnter>
      <form (ngSubmit)="onSubmit()" novalidate>
        <div class="form-section">
          <h3 class="form-section__title">Personal Information</h3>
          <div class="form-grid">
            <div class="field-group">
              <label class="field-label" for="firstName">First Name *</label>
              <input id="firstName" type="text" class="field-input" [(ngModel)]="form.firstName" name="firstName" required placeholder="Mohamed" />
            </div>
            <div class="field-group">
              <label class="field-label" for="lastName">Last Name *</label>
              <input id="lastName" type="text" class="field-input" [(ngModel)]="form.lastName" name="lastName" required placeholder="Ahmed" />
            </div>
            <div class="field-group">
              <label class="field-label" for="dob">Date of Birth *</label>
              <input id="dob" type="date" class="field-input" [(ngModel)]="form.dateOfBirth" name="dateOfBirth" required />
            </div>
            <div class="field-group">
              <label class="field-label" for="gender">Gender *</label>
              <select id="gender" class="field-input" [(ngModel)]="form.gender" name="gender">
                @for (g of genderOptions; track g) {
                  <option [value]="g">{{ g }}</option>
                }
              </select>
            </div>
            <div class="field-group">
              <label class="field-label" for="nationalId">National ID</label>
              <input id="nationalId" type="text" class="field-input" [(ngModel)]="form.nationalId" name="nationalId" placeholder="12345678901234" maxlength="14" />
            </div>
            <div class="field-group">
              <label class="field-label" for="bloodType">Blood Type</label>
              <select id="bloodType" class="field-input" [(ngModel)]="form.bloodType" name="bloodType">
                @for (bt of bloodTypeOptions; track bt) {
                  <option [value]="bt">{{ bt }}</option>
                }
              </select>
            </div>
          </div>
        </div>

        <div class="form-section">
          <h3 class="form-section__title">Contact Information</h3>
          <div class="form-grid">
            <div class="field-group">
              <label class="field-label" for="phone">Phone *</label>
              <input id="phone" type="tel" class="field-input" [(ngModel)]="form.phone" name="phone" required placeholder="01012345678" />
            </div>
            <div class="field-group">
              <label class="field-label" for="email">Email</label>
              <input id="email" type="email" class="field-input" [(ngModel)]="form.email" name="email" placeholder="patient@example.com" />
            </div>
            <div class="field-group field-group--full">
              <label class="field-label" for="address">Address</label>
              <input id="address" type="text" class="field-input" [(ngModel)]="form.address" name="address" placeholder="123 Main Street, Cairo" />
            </div>
          </div>
        </div>

        <div class="form-actions">
          <button type="button" class="btn-secondary" (click)="cancel()">Cancel</button>
          <button type="submit" id="btn-submit-patient" class="btn-primary" [disabled]="isLoading()">
            @if (isLoading()) {
              <span class="spinner"></span> Registering...
            } @else {
              Register Patient
            }
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .form-card {
      background: var(--color-surface); border: 1px solid var(--color-border);
      border-radius: var(--radius-xl); padding: var(--space-8);
      box-shadow: var(--shadow-sm);
    }
    .form-section { margin-bottom: var(--space-8); }
    .form-section__title {
      font-size: var(--text-base); font-weight: 600; color: var(--color-text-primary);
      margin-bottom: var(--space-5); padding-bottom: var(--space-3);
      border-bottom: 1px solid var(--color-border);
    }
    .form-grid {
      display: grid; grid-template-columns: repeat(2, 1fr); gap: var(--space-4);
    }
    .field-group { display: flex; flex-direction: column; }
    .field-group--full { grid-column: 1 / -1; }
    .field-label {
      font-size: var(--text-sm); font-weight: 500; color: var(--color-text-primary);
      margin-bottom: var(--space-2);
    }
    .field-input {
      padding: var(--space-3) var(--space-4); border: 1px solid var(--color-border-strong);
      border-radius: var(--radius-md); font-size: var(--text-sm);
      font-family: var(--font-family); color: var(--color-text-primary);
      background: var(--color-surface); outline: none;
      transition: border-color var(--duration-fast);
    }
    .field-input:focus { border-color: var(--color-accent); box-shadow: 0 0 0 3px var(--color-accent-light); }
    .form-actions {
      display: flex; justify-content: flex-end; gap: var(--space-3);
      padding-top: var(--space-6); border-top: 1px solid var(--color-border);
    }
    .btn-primary {
      display: inline-flex; align-items: center; gap: var(--space-2);
      background: var(--color-accent); color: white;
      border: none; border-radius: var(--radius-md);
      padding: var(--space-3) var(--space-6); font-size: var(--text-sm); font-weight: 600;
      cursor: pointer; font-family: var(--font-family); transition: all var(--duration-fast);
    }
    .btn-primary:hover:not(:disabled) { background: var(--color-accent-dark); }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-secondary {
      background: var(--color-surface-2); color: var(--color-text-primary);
      border: 1px solid var(--color-border); border-radius: var(--radius-md);
      padding: var(--space-3) var(--space-5); font-size: var(--text-sm); font-weight: 500;
      cursor: pointer; font-family: var(--font-family); transition: all var(--duration-fast);
    }
    .btn-secondary:hover { background: var(--color-border); }
    .spinner {
      width: 14px; height: 14px;
      border: 2px solid rgba(255,255,255,0.3); border-top-color: white;
      border-radius: 50%; animation: spin 0.7s linear infinite; display: inline-block;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
})
export class PatientRegisterComponent {
  private readonly patientsClient = inject(PatientsClient);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  isLoading = signal(false);
  genderOptions = Object.values(GenderType);
  bloodTypeOptions = Object.values(BloodType);

  form = {
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    gender: GenderType.Male as GenderType,
    nationalId: '',
    phone: '',
    email: '',
    bloodType: BloodType.Unknown as BloodType,
    address: '',
  };

  async onSubmit() {
    this.isLoading.set(true);
    try {
      const command = new RegisterPatientCommand({
        firstName: this.form.firstName,
        lastName: this.form.lastName,
        dateOfBirth: new Date(this.form.dateOfBirth),
        gender: this.form.gender as any,
        nationalId: this.form.nationalId,
        phone: this.form.phone,
        email: this.form.email,
        bloodType: this.form.bloodType as any,
      });
      const result = await this.patientsClient.register(command);
      this.notifications.success(`Patient registered! MRN: ${result.mrn}`);
      this.router.navigate(['/patients', result.id]);
    } catch (err: any) {
      this.notifications.error(err?.error?.detail ?? 'Failed to register patient.');
    } finally {
      this.isLoading.set(false);
    }
  }

  cancel() {
    this.router.navigate(['/patients']);
  }
}
