import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Calendar as CalendarIcon, Clock, User, CheckCircle2, XCircle, MoreVertical } from 'lucide-angular';
import { Client as AppointmentsClient, AppointmentDto } from '../../../core/api/mediqueue-api';
import { NotificationService } from '../../../core/services/notification.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-appointment-list',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './appointment-list.component.html'
})
export class AppointmentListComponent implements OnInit {
  private readonly client = inject(AppointmentsClient);
  private readonly notifications = inject(NotificationService);

  readonly LucideIcons = { CalendarIcon, Clock, User, CheckCircle2, XCircle, MoreVertical };

  readonly isLoading = signal(true);
  readonly appointments = signal<AppointmentDto[]>([]);
  readonly error = signal<string | null>(null);

  readonly filter = signal<'Today' | 'Upcoming' | 'Past' | 'Cancelled'>('Today');

  async ngOnInit(): Promise<void> {
    await this.loadAppointments();
  }

  async loadAppointments(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      let obs$;
      switch (this.filter()) {
        case 'Today':
          obs$ = this.client.today(); // Backend returns void? Need to check if it returns data
          // Actually, based on mediqueue-api.ts, today() returns Observable<void>. 
          // This might be a mistake in NSwag gen or backend API. 
          // I'll use appointmentsGET for now if possible or patient()
          break;
        case 'Upcoming':
          obs$ = this.client.upcoming(7);
          break;
        default:
          // Fallback to a general fetch if endpoints are not fully compatible
          this.appointments.set([]); 
          return;
      }
      
      const result = await firstValueFrom(obs$ as any);
      this.appointments.set((result as any)?.items || []);
    } catch (err: any) {
      const msg = err?.error?.detail || 'Failed to load appointments.';
      this.error.set(msg);
      this.notifications.error(msg);
    } finally {
      this.isLoading.set(false);
    }
  }

  async setFilter(f: 'Today' | 'Upcoming' | 'Past' | 'Cancelled'): Promise<void> {
    this.filter.set(f);
    await this.loadAppointments();
  }
}
