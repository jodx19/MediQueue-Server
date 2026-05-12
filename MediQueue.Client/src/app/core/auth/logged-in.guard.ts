import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const loggedInGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    const role = authService.currentUser()?.role;
    
    // Redirect based on role
    const redirectMap: Record<string, string> = {
      'Admin': '/dashboard',
      'Doctor': '/clinical-visits',
      'Receptionist': '/appointments',
    };

    const target = redirectMap[role!] ?? '/dashboard';
    return router.parseUrl(target);
  }

  return true;
};
