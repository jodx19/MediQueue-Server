import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'mrn',
  standalone: true,
})
export class MrnPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) return '—';
    return `MRN-${value}`;
  }
}
