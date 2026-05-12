import { Routes } from '@angular/router';

export const APPOINTMENTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./appointment-list/appointment-list.component').then(m => m.AppointmentListComponent),
  },
  {
    path: 'book',
    loadComponent: () => import('./appointment-book/appointment-book.component').then(m => m.AppointmentBookComponent),
  },
];
