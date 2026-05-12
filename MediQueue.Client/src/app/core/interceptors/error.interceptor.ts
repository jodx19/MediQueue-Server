import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred';

      if (error.status === 0) {
        errorMessage = 'Connection failed. Please check your internet connection.';
        notifications.error(errorMessage);
      } 
      else if (error.status === 401) {
        errorMessage = 'Session expired. Please login again.';
        authService.logout();
        router.navigate(['/auth/login'], { queryParams: { returnUrl: router.url } });
        notifications.warning(errorMessage);
      } 
      else if (error.status === 403) {
        errorMessage = 'You do not have permission to perform this action.';
        notifications.error(errorMessage);
      } 
      else if (error.status === 422) {
        // Validation errors
        const detail = error.error?.detail || 'Validation failed';
        notifications.error(detail);
      } 
      else if (error.status === 429) {
        errorMessage = 'Too many requests. Please wait a moment.';
        notifications.warning(errorMessage);
      } 
      else if (error.status >= 500) {
        errorMessage = 'Server error. Our team has been notified.';
        notifications.error(errorMessage);
      }

      return throwError(() => error);
    })
  );
};
