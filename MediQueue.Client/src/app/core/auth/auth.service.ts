import { Injectable, inject, signal, computed } from '@angular/core';
import { Client as AuthClient, LoginCommand as LoginRequest } from '../api/mediqueue-api';
import { firstValueFrom } from 'rxjs';

export interface UserSession {
  token: string;
  email: string;
  role: 'Admin' | 'Doctor' | 'Receptionist';
  name: string;
  expiresAt: Date;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly authClient = inject(AuthClient); // NSwag Generated

  private _session = signal<UserSession | null>(this.loadSession());

  readonly currentUser  = this._session.asReadonly();
  readonly isLoggedIn   = computed(() => !!this._session());
  readonly userRole     = computed(() => this._session()?.role ?? null);
  readonly isSuperAdmin = computed(() => this._session()?.role === 'Admin');

  async login(email: string, password: string): Promise<void> {
    try {
      const response = await firstValueFrom(
        this.authClient.login(new LoginRequest({ email, password }))
      );

      // The role comes directly from JWT claims mapped by backend
      const session: UserSession = {
        token:     response.token!,
        email:     response.username || email,
        role:      (response.role as any) || 'Admin', // Ensure backend sends valid role string
        name:      response.username || 'User',
        expiresAt: new Date(response.expiryTime!),
      };

      sessionStorage.setItem('mq_session', JSON.stringify(session));
      this._session.set(session);
      
      console.log('User logged in successfully:', session.email);
    } catch (error) {
      console.error('Login failed:', error);
      throw error;
    }
  }

  logout(): void {
    sessionStorage.removeItem('mq_session');
    this._session.set(null);
  }

  getToken(): string | null {
    return this._session()?.token ?? null;
  }

  hasRole(...roles: string[]): boolean {
    const r = this.userRole();
    return r ? roles.includes(r) : false;
  }

  private loadSession(): UserSession | null {
    try {
      const raw = sessionStorage.getItem('mq_session');
      if (!raw) return null;
      
      const s = JSON.parse(raw) as UserSession;
      
      // Check for token expiration
      if (new Date(s.expiresAt) < new Date()) {
        sessionStorage.removeItem('mq_session');
        return null;
      }
      
      return s;
    } catch {
      return null;
    }
  }
}
