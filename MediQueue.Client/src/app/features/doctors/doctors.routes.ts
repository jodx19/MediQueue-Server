import { Routes } from '@angular/router';

export const DOCTORS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./doctor-list/doctor-list.component').then(m => m.DoctorListComponent),
  },
  {
    path: ':id',
    loadComponent: () => import('./doctor-detail/doctor-detail.component').then(m => m.DoctorDetailComponent),
  },
];
