import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="empty-state">
      <div class="empty-state__icon">
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <path d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2"/>
          <rect x="9" y="3" width="6" height="4" rx="2"/>
          <path d="M9 12h6M9 16h4"/>
        </svg>
      </div>
      <h3 class="empty-state__title">{{ title }}</h3>
      <p class="empty-state__message">{{ message }}</p>
      @if (actionLabel) {
        <button class="btn-primary" (click)="action.emit()">{{ actionLabel }}</button>
      }
    </div>
  `,
  styles: [`
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: var(--space-12) var(--space-8);
      text-align: center;
    }
    .empty-state__icon {
      color: var(--color-text-tertiary);
      margin-bottom: var(--space-4);
      opacity: 0.6;
    }
    .empty-state__title {
      font-size: var(--text-lg);
      font-weight: 600;
      color: var(--color-text-primary);
      margin-bottom: var(--space-2);
      letter-spacing: -0.3px;
    }
    .empty-state__message {
      font-size: var(--text-sm);
      color: var(--color-text-secondary);
      max-width: 340px;
      margin-bottom: var(--space-6);
      line-height: 1.6;
    }
    .btn-primary {
      display: inline-flex; align-items: center; gap: var(--space-2);
      background: var(--color-accent); color: white;
      border: none; border-radius: var(--radius-md);
      padding: var(--space-3) var(--space-6);
      font-size: var(--text-sm); font-weight: 600;
      cursor: pointer; font-family: var(--font-family);
      transition: all var(--duration-fast) var(--ease-smooth);
    }
    .btn-primary:hover { background: var(--color-accent-dark); transform: translateY(-1px); }
  `],
})
export class EmptyStateComponent {
  @Input() title = 'No data found';
  @Input() message = 'There are no records to display.';
  @Input() actionLabel = '';
  @Output() action = new EventEmitter<void>();
}
