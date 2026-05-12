import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Toast {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private _toasts = new BehaviorSubject<Toast[]>([]);
  readonly toasts$ = this._toasts.asObservable();

  private show(type: Toast['type'], message: string, duration = 4000) {
    const id = Math.random().toString(36).slice(2);
    const toast: Toast = { id, type, message };
    this._toasts.next([...this._toasts.value, toast]);
    setTimeout(() => this.remove(id), duration);
  }

  success(message: string) { this.show('success', message); }
  error(message: string)   { this.show('error', message, 6000); }
  warning(message: string) { this.show('warning', message); }
  info(message: string)    { this.show('info', message); }

  remove(id: string) {
    this._toasts.next(this._toasts.value.filter(t => t.id !== id));
  }
}
