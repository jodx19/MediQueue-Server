import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Search, Filter, Plus, MoreVertical, Eye, Edit, CalendarPlus, UserPlus } from 'lucide-angular';
import { Client as PatientsClient, PatientSummaryDto } from '../../../core/api/mediqueue-api';
import { NotificationService } from '../../../core/services/notification.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-patient-list',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './patient-list.component.html'
})
export class PatientListComponent implements OnInit {
  private readonly client = inject(PatientsClient);
  private readonly notifications = inject(NotificationService);

  readonly LucideIcons = { Search, Filter, Plus, MoreVertical, Eye, Edit, CalendarPlus, UserPlus };

  // ═══ State with Signals
  readonly isLoading = signal(true);
  readonly patients = signal<PatientSummaryDto[]>([]);
  readonly error = signal<string | null>(null);
  readonly total = signal(0);

  // ═══ Search/Filter state
  readonly searchQuery = signal('');
  readonly currentPage = signal(1);
  readonly pageSize = 10;
  readonly selectedFilter = signal<'All' | 'Active' | 'Inactive'>('All');

  async ngOnInit(): Promise<void> {
    await this.loadPatients();
  }

  async loadPatients(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      // Use the generated NSwag Client
      const result = await firstValueFrom(
        this.client.patientsGET(this.currentPage(), this.pageSize)
      );
      
      // Handle the PagedResult structure
      // Based on mediqueue-api.ts analysis: patientsGET returns ApiResponse_1OfOfPagedResult...
      const pagedResult = (result as any).data || result;
      this.patients.set(pagedResult.items || []);
      this.total.set(pagedResult.totalCount || 0);
    } catch (err: any) {
      const msg = err?.error?.detail || 'Failed to load patients. Please try again.';
      this.error.set(msg);
      this.notifications.error(msg);
    } finally {
      this.isLoading.set(false);
    }
  }

  async onSearch(term: string): Promise<void> {
    this.searchQuery.set(term);
    this.currentPage.set(1);
    await this.loadPatients();
  }

  async setFilter(f: 'All' | 'Active' | 'Inactive'): Promise<void> {
    this.selectedFilter.set(f);
    this.currentPage.set(1);
    await this.loadPatients();
  }

  async onPageChange(page: number): Promise<void> {
    this.currentPage.set(page);
    await this.loadPatients();
  }
}
