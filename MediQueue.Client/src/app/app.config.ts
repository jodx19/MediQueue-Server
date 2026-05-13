import { ApplicationConfig, provideZoneChangeDetection, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { LucideAngularModule, icons } from 'lucide-angular';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { 
  API_BASE_URL, 
  AuthClient, 
  PatientsClient, 
  DoctorsClient, 
  AppointmentsClient, 
  ClinicalVisitsClient, 
  InvoicesClient, 
  DashboardClient 
} from './core/api/mediqueue-api';
import { environment } from '../environments/environment';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(
      withInterceptors([authInterceptor, errorInterceptor])
    ),
    { provide: API_BASE_URL, useValue: environment.apiBaseUrl },
    AuthClient,
    PatientsClient,
    DoctorsClient,
    AppointmentsClient,
    ClinicalVisitsClient,
    InvoicesClient,
    DashboardClient,
    importProvidersFrom(LucideAngularModule.pick(icons))
  ]
};
