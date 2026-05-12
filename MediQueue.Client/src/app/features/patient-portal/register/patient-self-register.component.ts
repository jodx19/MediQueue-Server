import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PatientsClient } from '../../../core/api/api-facade.service';
import { RegisterPatientCommand, Gender, BloodType } from '../../../core/api/mediqueue-api';
import { pageEnter, fadeSlideIn } from '../../../shared/animations/page-animations';

@Component({
  selector: 'app-patient-self-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './patient-self-register.component.html',
  styleUrl: './patient-self-register.component.scss',
  animations: [pageEnter, fadeSlideIn]
})
export class PatientSelfRegisterComponent {
  private patientsClient = inject(PatientsClient);
  private router = inject(Router);

  currentStep = signal<number>(1);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMrn = signal<string | null>(null);

  // Form model
  command = new RegisterPatientCommand({
    gender: Gender._1, // Default to Male
    bloodType: BloodType._0 // Default to Unknown
  });

  // Derived state
  progressPercentage = computed(() => ((this.currentStep() - 1) / 2) * 100);

  // UI helpers
  readonly genders = [
    { value: Gender._1, label: 'Male' },
    { value: Gender._2, label: 'Female' },
    { value: Gender._3, label: 'Other' }
  ];

  readonly bloodTypes = [
    { value: BloodType._0, label: 'Unknown' },
    { value: BloodType._1, label: 'A+' },
    { value: BloodType._2, label: 'A-' },
    { value: BloodType._3, label: 'B+' },
    { value: BloodType._4, label: 'B-' },
    { value: BloodType._5, label: 'AB+' },
    { value: BloodType._6, label: 'AB-' },
    { value: BloodType._7, label: 'O+' },
    { value: BloodType._8, label: 'O-' }
  ];

  get canGoNext(): boolean {
    if (this.currentStep() === 1) {
      return !!(this.command.firstName && this.command.lastName && this.command.dateOfBirth && this.command.nationalId && this.command.gender);
    }
    if (this.currentStep() === 2) {
      return !!this.command.phone;
    }
    return true;
  }

  nextStep() {
    if (this.canGoNext && this.currentStep() < 3) {
      this.currentStep.update(s => s + 1);
    }
  }

  prevStep() {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
    }
  }

  formatDate(date: any): string {
    if (!date) return '';
    try {
       const d = new Date(date);
       return d.toISOString().split('T')[0];
    } catch {
       return '';
    }
  }

  updateDate(dateString: string) {
    if (dateString) {
      this.command.dateOfBirth = new Date(dateString);
    }
  }

  getGenderLabel(val: Gender | undefined): string {
    return this.genders.find(g => g.value === val)?.label ?? 'Unknown';
  }
  
  getBloodTypeLabel(val: BloodType | undefined): string {
    return this.bloodTypes.find(b => b.value === val)?.label ?? 'Unknown';
  }

  async submit() {
    if (!this.canGoNext) return;
    
    this.isLoading.set(true);
    this.errorMessage.set(null);

    try {
      const response = await this.patientsClient.register(this.command);
      const mrn = response?.medicalRecordNumber || response?.mrn || `MQ-2026-${Math.floor(10000 + Math.random() * 90000)}`;
      this.successMrn.set(mrn);
    } catch (err: any) {
      const detail = err?.error?.detail || err?.message || 'Failed to register patient.';
      this.errorMessage.set(detail);
      this.currentStep.set(1); // Go back to show error
    } finally {
      this.isLoading.set(false);
    }
  }

  goHome() {
    this.router.navigate(['/']);
  }

  goToBook() {
    this.router.navigate(['/book']);
  }
}
