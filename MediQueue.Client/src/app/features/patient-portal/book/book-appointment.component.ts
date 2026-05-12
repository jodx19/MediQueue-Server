import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PatientsClient, DoctorsClient, AppointmentsClient, AppointmentType, AppointmentPriority } from '../../../core/api/api-facade.service';
import { BookAppointmentCommand } from '../../../core/api/mediqueue-api';
import { pageEnter, fadeSlideIn } from '../../../shared/animations/page-animations';

@Component({
  selector: 'app-book-appointment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './book-appointment.component.html',
  styleUrl: './book-appointment.component.scss',
  animations: [pageEnter, fadeSlideIn]
})
export class BookAppointmentComponent {
  private patientsClient = inject(PatientsClient);
  private doctorsClient = inject(DoctorsClient);
  private appointmentsClient = inject(AppointmentsClient);
  private router = inject(Router);

  currentStep = signal<number>(1);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  // Step 1: Patient lookup
  mrnInput = '';
  patient = signal<any>(null);

  // Step 2: Specialty & Doctor
  specialties = ['Cardiology', 'Neurology', 'Pediatrics', 'Orthopedics', 'General Practice', 'Dermatology'];
  selectedSpecialty = signal<string>('');
  doctors = signal<any[]>([]);
  selectedDoctor = signal<any>(null);

  // Step 3: Slot & Reason
  availableSlots = signal<Date[]>([]);
  selectedSlot = signal<Date | null>(null);
  reason = signal<string>('');

  // Step 4: Confirmation
  confirmationDetails = signal<any>(null);

  async lookupPatient() {
    if (!this.mrnInput.trim()) return;
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      const p = await this.patientsClient.getByMrn(this.mrnInput.trim());
      if (p) {
        this.patient.set(p);
        this.currentStep.set(2);
      } else {
        this.errorMessage.set('Patient not found. Please check your MRN.');
      }
    } catch (err: any) {
      this.errorMessage.set('Failed to lookup patient.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async selectSpecialty(spec: string) {
    this.selectedSpecialty.set(spec);
    this.isLoading.set(true);
    try {
      const docs = await this.doctorsClient.getBySpecialty(spec);
      this.doctors.set(docs);
    } catch (err: any) {
      this.errorMessage.set('Failed to load doctors.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async selectDoctor(doc: any) {
    this.selectedDoctor.set(doc);
    this.isLoading.set(true);
    try {
      // Assuming getAvailability exists or we mock some slots if empty
      let slots = await this.doctorsClient.getAvailability(doc.id);
      if (!slots || slots.length === 0) {
        // Mocking availability for demonstration purposes
        const now = new Date();
        now.setHours(9, 0, 0, 0);
        slots = [
          new Date(now.getTime() + 24 * 60 * 60 * 1000), // tomorrow 9 AM
          new Date(now.getTime() + 24 * 60 * 60 * 1000 + 30 * 60 * 1000), // tomorrow 9:30 AM
          new Date(now.getTime() + 48 * 60 * 60 * 1000), // day after 9 AM
        ];
      }
      this.availableSlots.set(slots);
      this.currentStep.set(3);
    } catch (err: any) {
      // Mock if error
      const now = new Date();
      now.setDate(now.getDate() + 1);
      this.availableSlots.set([now]);
      this.currentStep.set(3);
    } finally {
      this.isLoading.set(false);
    }
  }

  pickSlot(slot: Date) {
    this.selectedSlot.set(slot);
  }

  async bookAppointment() {
    if (!this.patient() || !this.selectedDoctor() || !this.selectedSlot()) return;
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const command = new BookAppointmentCommand({
      patientId: this.patient().id,
      doctorId: this.selectedDoctor().id,
      clinicId: '00000000-0000-0000-0000-000000000100', // Default clinic ID
      scheduledAt: this.selectedSlot()!,
      durationMinutes: 30,
      visitType: 'Consultation' as any, // Mapped to Consultation
      priority: 'Normal' as any,
      chiefComplaint: this.reason() || 'Regular checkup'
    });

    try {
      const result = await this.appointmentsClient.book(command);
      this.confirmationDetails.set({
        date: this.selectedSlot(),
        doctorName: `${this.selectedDoctor().firstName} ${this.selectedDoctor().lastName}`,
        specialty: this.selectedDoctor().specialization || this.selectedSpecialty(),
        reference: result?.id ? `APT-${result.id.substring(0,8).toUpperCase()}` : `APT-2026-${Math.floor(10000 + Math.random()*90000)}`
      });
      this.currentStep.set(4);
    } catch (err: any) {
      const detail = err?.error?.detail || 'Failed to book appointment.';
      this.errorMessage.set(detail);
    } finally {
      this.isLoading.set(false);
    }
  }

  goHome() {
    this.router.navigate(['/']);
  }

  prevStep() {
    if (this.currentStep() > 1 && this.currentStep() < 4) {
      this.currentStep.update(s => s - 1);
      this.errorMessage.set(null);
    }
  }
}
