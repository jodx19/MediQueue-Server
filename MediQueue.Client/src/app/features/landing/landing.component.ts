import { Component, HostListener, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';
import { LucideAngularModule, CalendarPlus, Stethoscope, Receipt, Shield, Activity, Users, FileText, ArrowRight, Play, Menu, X, CheckCircle, Clock, DollarSign, ActivitySquare, HeartPulse } from 'lucide-angular';
import { ScrollRevealDirective } from '../../shared/directives/scroll-reveal.directive';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterModule, LucideAngularModule, ScrollRevealDirective],
  templateUrl: './landing.component.html',
  animations: [
    trigger('pageEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(24px)' }),
        animate('500ms cubic-bezier(0.34,1.56,0.64,1)', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('list', [
      transition('* => *', [
        query(':enter', [
          style({ opacity: 0, transform: 'translateY(16px)' }),
          stagger(80, [
            animate('400ms cubic-bezier(0.4,0,0.2,1)', style({ opacity: 1, transform: 'translateY(0)' }))
          ])
        ], { optional: true })
      ])
    ])
  ]
})
export class LandingComponent {
  scrolled = signal(false);
  mobileMenuOpen = signal(false);
  activeTab = signal<'Patient' | 'Doctor' | 'Admin'>('Admin');

  readonly LucideIcons = {
    CalendarPlus, Stethoscope, Receipt, Shield, Activity, Users, FileText, ArrowRight, Play, Menu, X, CheckCircle, Clock, DollarSign, ActivitySquare, HeartPulse
  };

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.scrolled.set(window.scrollY > 50);
  }

  toggleMobileMenu() {
    this.mobileMenuOpen.set(!this.mobileMenuOpen());
  }

  setTab(tab: 'Patient' | 'Doctor' | 'Admin') {
    this.activeTab.set(tab);
  }
}
