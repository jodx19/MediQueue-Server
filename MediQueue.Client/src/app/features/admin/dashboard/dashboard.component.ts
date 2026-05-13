import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Users, Calendar, TrendingUp, DollarSign, Activity, Clock, ChevronUp, ChevronDown } from 'lucide-angular';
import { DashboardClient, ClinicStatsDto } from '../../../core/api/mediqueue-api';
import { NotificationService } from '../../../core/services/notification.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  private readonly client = inject(DashboardClient);
  private readonly notifications = inject(NotificationService);

  readonly LucideIcons = { Users, Calendar, TrendingUp, DollarSign, Activity, Clock, ChevronUp, ChevronDown };

  readonly isLoading = signal(true);
  readonly stats = signal<ClinicStatsDto | null>(null);

  // Mock data for dashboard visuals until fully integrated
  readonly metrics = [
    { label: 'Total Patients', value: '1,284', trend: '+12%', up: true, icon: 'Users' },
    { label: 'Appointments', value: '42', trend: '+5%', up: true, icon: 'Calendar' },
    { label: 'Revenue', value: '$12,400', trend: '-2%', up: false, icon: 'DollarSign' },
    { label: 'Active Doctors', value: '18', trend: '0%', up: true, icon: 'Activity' }
  ];

  readonly recentAppointments = [
    { patient: 'Sarah Johnson', doctor: 'Dr. Michael Chen', date: 'Today, 10:30 AM', status: 'Waiting' },
    { patient: 'Robert Smith', doctor: 'Dr. Sarah Wilson', date: 'Today, 11:00 AM', status: 'In Progress' },
    { patient: 'Maria Garcia', doctor: 'Dr. James Lee', date: 'Today, 11:45 AM', status: 'Scheduled' }
  ];

  readonly topDoctors = [
    { name: 'Dr. Michael Chen', specialty: 'Cardiology', patients: 142 },
    { name: 'Dr. Sarah Wilson', specialty: 'Pediatrics', patients: 128 },
    { name: 'Dr. James Lee', specialty: 'Dermatology', patients: 95 }
  ];

  async ngOnInit(): Promise<void> {
    await this.loadStats();
  }

  async loadStats(): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await firstValueFrom(this.client.stats());
      this.stats.set(result);
    } catch (err: any) {
      this.notifications.error('Failed to load dashboard statistics.');
    } finally {
      this.isLoading.set(false);
    }
  }
}
