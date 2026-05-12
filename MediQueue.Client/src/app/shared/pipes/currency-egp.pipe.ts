import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'currencyEgp',
  standalone: true,
})
export class CurrencyEgpPipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    if (value == null) return '—';
    return `EGP ${value.toLocaleString('en-EG', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }
}
