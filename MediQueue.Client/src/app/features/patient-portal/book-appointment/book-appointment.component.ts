import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';
import { LucideAngularModule, UserCircle, Stethoscope, CalendarClock, CheckCircle, ArrowRight, ArrowLeft } from 'lucide-angular';

@Component({
  selector: 'app-book-appointment',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './book-appointment.component.html',
  animations: [
    trigger('stepEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(20px)' }),
        animate('400ms cubic-bezier(0.34,1.56,0.64,1)', style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ]),
    trigger('list', [
      transition('* => *', [
        query(':enter', [
          style({ opacity: 0, transform: 'translateY(16px)' }),
          stagger(50, [
            animate('300ms cubic-bezier(0.4,0,0.2,1)', style({ opacity: 1, transform: 'translateY(0)' }))
          ])
        ], { optional: true })
      ])
    ])
  ]
})
export class BookAppointmentComponent {
  currentStep = signal(1);
  patientIdentity = signal('');
  selectedSpecialty = signal('');
  selectedDoctor = signal<any>(null);
  selectedSlot = signal<any>(null);

  readonly LucideIcons = { UserCircle, Stethoscope, CalendarClock, CheckCircle, ArrowRight, ArrowLeft };

  specialties = [
    { id: '1', name: 'Cardiology', icon: 'HeartPulse' },
    { id: '2', name: 'Pediatrics', icon: 'Baby' },
    { id: '3', name: 'Orthopedics', icon: 'Bone' },
    { id: '4', name: 'Dermatology', icon: 'Syringe' }
  ];

  doctors = [
    { id: '1', name: 'Dr. Ahmed Samy', specialty: 'Cardiology', availability: 'Next available: Tomorrow, 10:00 AM' },
    { id: '2', name: 'Dr. Sara Ali', specialty: 'Cardiology', availability: 'Next available: Today, 2:30 PM' }
  ];

  slots = [
    { id: '1', time: '10:00 AM', status: 'available' },
    { id: '2', time: '10:30 AM', status: 'booked' },
    { id: '3', time: '11:00 AM', status: 'available' },
    { id: '4', time: '11:30 AM', status: 'available' }
  ];

  nextStep() {
    if (this.currentStep() < 4) {
      this.currentStep.update(s => s + 1);
    } else {
      // confirm
      this.currentStep.set(5); // success
    }
  }

  prevStep() {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
    }
  }

  selectSpecialty(s: string) {
    this.selectedSpecialty.set(s);
    this.nextStep();
  }

  selectDoctor(d: any) {
    this.selectedDoctor.set(d);
  }

  selectSlot(s: any) {
    if (s.status === 'available') {
      this.selectedSlot.set(s);
    }
  }
}
