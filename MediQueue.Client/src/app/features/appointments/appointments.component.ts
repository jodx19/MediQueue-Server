import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';
import { LucideAngularModule, Calendar as CalendarIcon, List, Clock, User, CheckCircle2, XCircle, CalendarPlus, ChevronLeft, ChevronRight, X } from 'lucide-angular';

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './appointments.component.html',
  animations: [
    trigger('fade', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease-out', style({ opacity: 1 }))
      ])
    ]),
    trigger('listAnimation', [
      transition('* => *', [
        query(':enter', [
          style({ opacity: 0, transform: 'translateY(10px)' }),
          stagger(50, [
            animate('300ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
          ])
        ], { optional: true })
      ])
    ]),
    trigger('modalEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.95) translateY(20px)' }),
        animate('400ms cubic-bezier(0.34,1.56,0.64,1)', style({ opacity: 1, transform: 'scale(1) translateY(0)' }))
      ])
    ])
  ]
})
export class AppointmentsComponent {
  readonly LucideIcons = { CalendarIcon, List, Clock, User, CheckCircle2, XCircle, CalendarPlus, ChevronLeft, ChevronRight, X };

  view = signal<'list' | 'calendar'>('list');
  filter = signal<'Today' | 'Upcoming' | 'Past' | 'Cancelled'>('Today');
  
  isBookModalOpen = signal(false);

  appointments = signal([
    { id: 1, time: '10:00 AM', patient: 'Omar Tarek', doctor: 'Dr. Ahmed Samy', type: 'Follow Up', status: 'Scheduled' },
    { id: 2, time: '11:30 AM', patient: 'Sara Ali', doctor: 'Dr. Mona Hassan', type: 'New Patient', status: 'Scheduled' },
    { id: 3, time: '01:00 PM', patient: 'Khaled Hassan', doctor: 'Dr. Tarek Ziad', type: 'Consultation', status: 'Completed' },
    { id: 4, time: '03:45 PM', patient: 'Nour Yasser', doctor: 'Dr. Ahmed Samy', type: 'Procedure', status: 'Cancelled' },
  ]);

  weekDays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
  calendarHours = ['09:00 AM', '10:00 AM', '11:00 AM', '12:00 PM', '01:00 PM', '02:00 PM', '03:00 PM', '04:00 PM'];

  setView(v: 'list' | 'calendar') {
    this.view.set(v);
  }

  setFilter(f: 'Today' | 'Upcoming' | 'Past' | 'Cancelled') {
    this.filter.set(f);
  }

  openBookModal() {
    this.isBookModalOpen.set(true);
  }

  closeBookModal() {
    this.isBookModalOpen.set(false);
  }
}
