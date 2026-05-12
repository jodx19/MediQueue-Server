import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate, stagger, query } from '@angular/animations';
import { LucideAngularModule, Activity, AlertCircle, Clock, CheckCircle2, User, Save, RefreshCw } from 'lucide-angular';

@Component({
  selector: 'app-clinical-visits-dashboard',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './clinical-visits-dashboard.component.html',
  animations: [
    trigger('staggerList', [
      transition('* => *', [
        query(':enter', [
          style({ opacity: 0, transform: 'translateX(-10px)' }),
          stagger(50, [
            animate('300ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
          ])
        ], { optional: true })
      ])
    ])
  ]
})
export class ClinicalVisitsDashboardComponent {
  readonly LucideIcons = { Activity, AlertCircle, Clock, CheckCircle2, User, Save, RefreshCw };

  queue = signal([
    { id: 1, name: 'Omar Tarek', reason: 'Fever & Cough', status: 'In Visit', time: '10:00 AM', active: true },
    { id: 2, name: 'Sara Ali', reason: 'Follow up', status: 'Waiting', time: '10:30 AM', active: false },
    { id: 3, name: 'Khaled Hassan', reason: 'Hypertension', status: 'Waiting', time: '11:00 AM', active: false },
  ]);

  saveStatus = signal<'Saved' | 'Saving...' | 'Unsaved'>('Saved');

  // Quick SOAP form signals
  subjective = signal('Patient complains of fever for 3 days...');
  objective = signal('Temp: 38.5C, BP: 120/80, HR: 95');
  assessment = signal('Viral Pharyngitis');
  plan = signal('Paracetamol 500mg SOS, Rest, Fluids.');

  onInput(type: 'S' | 'O' | 'A' | 'P', value: string) {
    this.saveStatus.set('Saving...');
    // Mock auto-save debounce
    setTimeout(() => this.saveStatus.set('Saved'), 1000);
  }

  selectPatient(id: number) {
    this.queue.update(q => q.map(p => ({ ...p, active: p.id === id })));
  }
}
