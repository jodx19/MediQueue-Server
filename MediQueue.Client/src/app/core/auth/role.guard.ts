import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * Guard to check if the user has the required roles to access a route.
 * @param allowedRoles List of roles that can access this route.
 */
export function roleGuard(allowedRoles: string[]): CanActivateFn {
  return () => {
    const auth   = inject(AuthService);
    const router = inject(Router);

    if (!auth.isLoggedIn()) {
      return router.createUrlTree(['/auth/login']);
    }

    if (auth.hasRole(...allowedRoles)) {
      return true;
    }

    // Redirect to the default dashboard for the user's role if unauthorized
    const roleHome: Record<string, string> = {
      'Admin':        '/dashboard',
      'Doctor':       '/my-queue',
      'Receptionist': '/appointments',
    };
    
    const userRole = auth.currentUser()?.role ?? '';
    const target = roleHome[userRole] ?? '/auth/login';
    
    console.warn(`Unauthorized access attempt. Redirecting ${userRole} to ${target}`);
    return router.createUrlTree([target]);
  };
}
