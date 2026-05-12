import { Routes } from '@angular/router';

export const PATIENTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./patient-list/patient-list.component').then(m => m.PatientListComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./patient-register/patient-register.component').then(m => m.PatientRegisterComponent),
  },
  {
    path: ':id',
    loadComponent: () => import('./patient-detail/patient-detail.component').then(m => m.PatientDetailComponent),
  },
];
