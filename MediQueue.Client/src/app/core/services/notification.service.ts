import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info' | 'warning';
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  readonly toasts = signal<Toast[]>([]);
  private counter = 0;

  success(message: string) { this.add(message, 'success'); }
  error(message: string)   { this.add(message, 'error'); }
  info(message: string)    { this.add(message, 'info'); }
  warning(message: string) { this.add(message, 'warning'); }

  private add(message: string, type: Toast['type']) {
    const id = ++this.counter;
    this.toasts.update(t => [...t, { id, message, type }]);
    
    // Auto-remove after 5 seconds
    setTimeout(() => this.remove(id), 5000);
  }

  remove(id: number) {
    this.toasts.update(t => t.filter(toast => toast.id !== id));
  }
}
