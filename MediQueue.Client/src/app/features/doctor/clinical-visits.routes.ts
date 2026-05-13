import { Routes } from '@angular/router';

export const CLINICAL_VISITS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./clinical-visits-dashboard/clinical-visits-dashboard.component')
      .then(m => m.ClinicalVisitsDashboardComponent),
  }
];
