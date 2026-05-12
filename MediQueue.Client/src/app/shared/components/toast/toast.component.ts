import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toastSlide } from '../../animations/page-animations';
import { ToastService, Toast } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  animations: [toastSlide],
  template: `
    <div class="toast-container">
      @for (toast of toasts(); track toast.id) {
        <div class="toast toast--{{ toast.type }}" @toastSlide>
          <span class="toast__icon">{{ iconFor(toast.type) }}</span>
          <span class="toast__message">{{ toast.message }}</span>
          <button class="toast__close" (click)="toastService.remove(toast.id)">✕</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: var(--space-6);
      right: var(--space-6);
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
      max-width: 360px;
    }
    .toast {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-4) var(--space-5);
      border-radius: var(--radius-lg);
      background: var(--color-surface);
      box-shadow: var(--shadow-lg);
      border-left: 4px solid currentColor;
      font-size: var(--text-sm);
    }
    .toast--success { color: var(--color-success); }
    .toast--error   { color: var(--color-danger); }
    .toast--warning { color: var(--color-warning); }
    .toast--info    { color: var(--color-info); }
    .toast__message { flex: 1; color: var(--color-text-primary); }
    .toast__close {
      background: none; border: none; cursor: pointer;
      color: var(--color-text-tertiary); font-size: 12px;
      padding: 2px; line-height: 1;
    }
    .toast__close:hover { color: var(--color-text-secondary); }
  `],
})
export class ToastComponent implements OnInit {
  readonly toastService = inject(ToastService);
  toasts = signal<Toast[]>([]);

  ngOnInit() {
    this.toastService.toasts$.subscribe(t => this.toasts.set(t));
  }

  iconFor(type: string): string {
    const icons: Record<string, string> = {
      success: '✓', error: '✕', warning: '⚠', info: 'ℹ',
    };
    return icons[type] ?? 'ℹ';
  }
}
