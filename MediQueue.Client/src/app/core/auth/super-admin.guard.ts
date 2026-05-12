import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

export const superAdminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const user = auth.currentUser();

  // Super Admin check: either special email or role Admin
  const isSuper = user?.email === environment.superAdminEmail || auth.hasRole('Admin');

  if (isSuper) {
    return true;
  }

  return router.parseUrl('/dashboard');
};
