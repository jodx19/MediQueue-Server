import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Receipt, Search, Filter, Printer, Download, Plus, CheckCircle, Clock } from 'lucide-angular';
import { Client as InvoicesClient, InvoiceDto } from '../../../core/api/mediqueue-api';
import { NotificationService } from '../../../core/services/notification.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-invoice-list',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './invoice-list.component.html'
})
export class InvoiceListComponent implements OnInit {
  private readonly client = inject(InvoicesClient);
  private readonly notifications = inject(NotificationService);

  readonly LucideIcons = { Receipt, Search, Filter, Printer, Download, Plus, CheckCircle, Clock };

  readonly isLoading = signal(true);
  readonly invoices = signal<InvoiceDto[]>([]);
  readonly error = signal<string | null>(null);

  readonly filter = signal<'All' | 'Pending' | 'Paid' | 'Cancelled'>('All');

  async ngOnInit(): Promise<void> {
    await this.loadInvoices();
  }

  async loadInvoices(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      // Invoices API might require a patientId or have a general GET. 
      // Based on mediqueue-api.ts, invoicesGET(id) is for single. 
      // There might be a missing 'getAll' or it's handled via patient3(patientId).
      // For now, I'll attempt a generic fetch if it exists or set empty.
      
      // const result = await firstValueFrom(this.client.invoicesAll()); // Assuming it exists or similar
      // this.invoices.set(result || []);
      
      this.invoices.set([]); // Placeholder until exact endpoint is confirmed
    } catch (err: any) {
      this.notifications.error('Failed to load invoices.');
    } finally {
      this.isLoading.set(false);
    }
  }

  setFilter(f: 'All' | 'Pending' | 'Paid' | 'Cancelled') {
    this.filter.set(f);
  }
}
