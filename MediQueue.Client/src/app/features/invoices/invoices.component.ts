import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { LucideAngularModule, Receipt, Search, Filter, Printer, Download, Plus, CheckCircle, Clock } from 'lucide-angular';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './invoices.component.html',
  animations: [
    trigger('fade', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(10px)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class InvoicesComponent {
  readonly LucideIcons = { Receipt, Search, Filter, Printer, Download, Plus, CheckCircle, Clock };

  filter = signal<'All' | 'Pending' | 'Paid' | 'Cancelled'>('All');
  
  invoices = signal([
    { id: 'INV-2026-1042', patient: 'Omar Tarek', doctor: 'Dr. Ahmed Samy', amount: 450, status: 'Pending', date: '12 May 2026' },
    { id: 'INV-2026-1041', patient: 'Sara Ali', doctor: 'Dr. Mona Hassan', amount: 300, status: 'Paid', date: '11 May 2026' },
    { id: 'INV-2026-1040', patient: 'Khaled Hassan', doctor: 'Dr. Tarek Ziad', amount: 850, status: 'Paid', date: '10 May 2026' },
    { id: 'INV-2026-1039', patient: 'Nour Yasser', doctor: 'Dr. Ahmed Samy', amount: 450, status: 'Cancelled', date: '09 May 2026' },
  ]);

  setFilter(f: 'All' | 'Pending' | 'Paid' | 'Cancelled') {
    this.filter.set(f);
  }

  get filteredInvoices() {
    if (this.filter() === 'All') return this.invoices();
    return this.invoices().filter(i => i.status === this.filter());
  }
}
