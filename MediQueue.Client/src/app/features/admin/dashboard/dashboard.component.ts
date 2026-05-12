import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Users, Calendar, TrendingUp, DollarSign, Activity, Clock, ChevronUp, ChevronDown } from 'lucide-angular';
import { Client as DashboardClient, ClinicStatsDto } from '../../../core/api/mediqueue-api';
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
