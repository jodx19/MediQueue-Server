import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type BadgeVariant = 'success' | 'warning' | 'danger' | 'info' | 'default';

@Component({
  selector: 'app-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span class="badge" [class]="'badge--' + variant">{{ label }}</span>
  `,
  styles: [`
    .badge {
      display: inline-flex;
      align-items: center;
      padding: 2px 10px;
      border-radius: var(--radius-full);
      font-size: var(--text-xs);
      font-weight: 600;
      letter-spacing: 0.3px;
    }
    .badge--success  { background: var(--color-success-bg); color: #1a7a30; }
    .badge--warning  { background: var(--color-warning-bg); color: #a05e00; }
    .badge--danger   { background: var(--color-danger-bg); color: #c0312a; }
    .badge--info     { background: var(--color-info-bg); color: #2a7ea8; }
    .badge--default  { background: var(--color-surface-2); color: var(--color-text-secondary); }
  `],
})
export class BadgeComponent {
  @Input() label = '';
  @Input() variant: BadgeVariant = 'default';
}
