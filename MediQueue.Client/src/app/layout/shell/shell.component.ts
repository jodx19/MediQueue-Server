import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { 
  LucideAngularModule, 
  LayoutDashboard, 
  Users, 
  Stethoscope, 
  Calendar, 
  Receipt, 
  Shield, 
  List, 
  Clipboard, 
  Search, 
  Bell, 
  ChevronLeft, 
  ChevronRight, 
  LogOut, 
  Menu
} from 'lucide-angular';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterModule, LucideAngularModule],
  templateUrl: './shell.component.html',
  animations: [
    trigger('pageEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(24px)' }),
        animate('500ms cubic-bezier(0.34,1.56,0.64,1)', style({ opacity: 1, transform: 'translateY(0)' }))
      ]),
      transition('* => *', [
        style({ opacity: 0, transform: 'translateY(24px)' }),
        animate('500ms cubic-bezier(0.34,1.56,0.64,1)', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class ShellComponent implements OnInit {
  collapsed = signal(false);
  mobileMenuOpen = signal(false);
  
  // Mock role and SignalR for UI purposes
  userRole = signal<'Admin' | 'Doctor' | 'Receptionist'>('Admin');
  signalRConnected = signal(true);
  notificationCount = signal(3);

  readonly LucideIcons = {
    LayoutDashboard, Users, Stethoscope, Calendar, Receipt, Shield, List, Clipboard, Search, Bell, ChevronLeft, ChevronRight, LogOut, Menu
  };

  adminNav = [
    { label: 'Dashboard', icon: 'LayoutDashboard', route: '/dashboard' },
    { label: 'Patients', icon: 'Users', route: '/patients/list' },
    { label: 'Doctors', icon: 'Stethoscope', route: '/doctors' },
    { label: 'Appointments', icon: 'Calendar', route: '/appointments' },
    { label: 'Invoices', icon: 'Receipt', route: '/invoices' },
    { label: 'Super Admin', icon: 'Shield', route: '/super-admin' }
  ];

  doctorNav = [
    { label: 'My Queue', icon: 'List', route: '/doctor/queue' },
    { label: 'Clinical Visits', icon: 'Clipboard', route: '/clinical-visits' },
    { label: 'My Patients', icon: 'Users', route: '/patients/list' }
  ];

  receptionistNav = [
    { label: 'Patients', icon: 'Users', route: '/patients/list' },
    { label: 'Appointments', icon: 'Calendar', route: '/appointments' },
    { label: 'Invoices', icon: 'Receipt', route: '/invoices' }
  ];

  get currentNav() {
    switch(this.userRole()) {
      case 'Admin': return this.adminNav;
      case 'Doctor': return this.doctorNav;
      case 'Receptionist': return this.receptionistNav;
      default: return [];
    }
  }

  ngOnInit() {
    // Mock signalR connect
    // this.signalR.connect();
  }

  toggleSidebar() {
    this.collapsed.set(!this.collapsed());
  }

  toggleMobileMenu() {
    this.mobileMenuOpen.set(!this.mobileMenuOpen());
  }

  // Used for router animation
  prepareRoute(outlet: any) {
    return outlet && outlet.activatedRouteData && outlet.activatedRouteData['animation'];
  }
}
