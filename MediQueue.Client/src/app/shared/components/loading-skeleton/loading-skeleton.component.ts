import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="skeleton-container" [class]="'skeleton-' + type">
      @for (i of items; track i) {
        <div class="skeleton-item">
          <div class="skeleton-pulse"></div>
        </div>
      }
    </div>
  `,
  styles: [`
    .skeleton-container { display: flex; flex-direction: column; gap: var(--space-4); }
    .skeleton-metric-card { flex-direction: row; flex-wrap: wrap; }
    .skeleton-metric-card .skeleton-item {
      flex: 1 1 200px;
      height: 100px;
      border-radius: var(--radius-lg);
      overflow: hidden;
    }
    .skeleton-item {
      height: 64px;
      border-radius: var(--radius-md);
      overflow: hidden;
      background: var(--color-surface-2);
    }
    .skeleton-pulse {
      width: 100%;
      height: 100%;
      background: linear-gradient(
        90deg,
        var(--color-surface-2) 25%,
        var(--color-border) 50%,
        var(--color-surface-2) 75%
      );
      background-size: 200% 100%;
      animation: shimmer 1.5s infinite;
    }
    @keyframes shimmer {
      0% { background-position: 200% 0; }
      100% { background-position: -200% 0; }
    }
  `],
})
export class LoadingSkeletonComponent {
  @Input() count = 3;
  @Input() type: 'default' | 'metric-card' | 'table-row' = 'default';

  get items(): number[] {
    return Array.from({ length: this.count }, (_, i) => i);
  }
}
