import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-header">
      <div class="page-header__content">
        <h1 class="page-header__title">{{ title }}</h1>
        @if (subtitle) {
          <p class="page-header__subtitle">{{ subtitle }}</p>
        }
      </div>
      @if (hasActions) {
        <div class="page-header__actions">
          <ng-content />
        </div>
      }
    </div>
  `,
  styles: [`
    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--space-6) 0 var(--space-5);
      border-bottom: 1px solid var(--color-border);
      margin-bottom: var(--space-6);
    }
    .page-header__title {
      font-size: var(--text-2xl);
      font-weight: 700;
      color: var(--color-text-primary);
      letter-spacing: -0.5px;
    }
    .page-header__subtitle {
      font-size: var(--text-sm);
      color: var(--color-text-secondary);
      margin-top: var(--space-1);
    }
    .page-header__actions {
      display: flex;
      gap: var(--space-3);
    }
  `],
})
export class PageHeaderComponent {
  @Input() title = '';
  @Input() subtitle = '';
  @Input() hasActions = true;
}
