import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';
import { ShellComponent } from './layout/shell/shell.component';

export const routes: Routes = [

  // ═══ PUBLIC (no guard) ═══
  {
    path: '',
    loadComponent: () => import('./features/landing/landing.component')
      .then(m => m.LandingComponent),
  },
  {
    path: 'book',
    loadComponent: () => import('./features/patient-portal/book-appointment/book-appointment.component')
      .then(m => m.BookAppointmentComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/patient-portal/patient-self-register/patient-self-register.component')
      .then(m => (m as any).PatientSelfRegisterComponent), // Handle potential naming variations
  },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login/login.component')
      .then(m => m.LoginComponent),
  },

  // ═══ PROTECTED (canActivate: [authGuard]) ═══
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

      // Admin only
      {
        path: 'dashboard',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () => import('./features/admin/dashboard/dashboard.component')
          .then(m => m.DashboardComponent),
      },
      {
        path: 'doctors',
        canActivate: [roleGuard(['Admin'])],
        loadChildren: () => import('./features/doctors/doctors.routes')
          .then(m => m.DOCTORS_ROUTES),
      },
      {
        path: 'super-admin',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () => import('./features/admin/super-admin/super-admin.component')
          .then(m => (m as any).SuperAdminComponent),
      },

      // Admin + Receptionist
      {
        path: 'patients',
        canActivate: [roleGuard(['Admin', 'Receptionist'])],
        loadChildren: () => import('./features/patients/patients.routes')
          .then(m => m.PATIENTS_ROUTES),
      },
      {
        path: 'appointments',
        canActivate: [roleGuard(['Admin', 'Receptionist'])],
        loadChildren: () => import('./features/appointments/appointments.routes')
          .then(m => m.APPOINTMENTS_ROUTES),
      },
      {
        path: 'invoices',
        canActivate: [roleGuard(['Admin', 'Receptionist'])],
        loadChildren: () => import('./features/invoices/invoices.routes')
          .then(m => m.INVOICES_ROUTES),
      },

      // Doctor only
      {
        path: 'clinical-visits',
        canActivate: [roleGuard(['Doctor'])],
        loadChildren: () => import('./features/doctor/clinical-visits.routes')
          .then(m => m.CLINICAL_VISITS_ROUTES),
      },
      {
        path: 'my-queue',
        canActivate: [roleGuard(['Doctor'])],
        loadComponent: () => import('./features/doctor/clinical-visits-dashboard/clinical-visits-dashboard.component')
          .then(m => m.ClinicalVisitsDashboardComponent),
      },

      { path: '**', redirectTo: 'dashboard' },
    ],
  },

  { path: '**', redirectTo: '' }
];
