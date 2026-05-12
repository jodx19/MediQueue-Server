import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { RecordPaymentCommand, ApplyDiscountCommand } from '../../../core/api/mediqueue-api';
import { InvoicesClient, PaymentMethod, DiscountType } from '../../../core/api/api-facade.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { CurrencyEgpPipe } from '../../../shared/pipes/currency-egp.pipe';
import { pageEnter } from '../../../shared/animations/page-animations';

@Component({
  selector: 'app-invoice-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent, LoadingSkeletonComponent, BadgeComponent, CurrencyEgpPipe],
  animations: [pageEnter],
  template: `
    <app-page-header
      [title]="invoice() ? 'Invoice #' + invoice().invoiceNumber : 'Invoice'"
      [subtitle]="invoice() ? (invoice().patientName ?? '') : ''"
    >
      <button class="btn-secondary" (click)="router.navigate(['/invoices'])">← Back</button>
    </app-page-header>

    @if (isLoading()) {
      <app-loading-skeleton [count]="3" />
    } @else if (invoice()) {
      <div class="invoice-layout" @pageEnter>
        <!-- Invoice Summary -->
        <div class="invoice-card">
          <div class="invoice-header">
            <div>
              <h3 class="inv-number">Invoice #{{ invoice().invoiceNumber }}</h3>
              <p class="inv-date">{{ invoice().issuedAt | date:'longDate' }}</p>
            </div>
            <app-badge [label]="invoice().status ?? 'Pending'" [variant]="statusVariant(invoice().status)" />
          </div>

          <!-- Line Items -->
          <div class="line-items">
            @for (item of invoice().items ?? []; track item.id) {
              <div class="line-item">
                <span class="item-desc">{{ item.description }}</span>
                <span class="item-qty">× {{ item.quantity }}</span>
                <span class="item-price">{{ item.unitPrice | currencyEgp }}</span>
                <span class="item-total">{{ (item.unitPrice! * item.quantity!) | currencyEgp }}</span>
              </div>
            }
          </div>

          <div class="invoice-totals">
            <div class="total-row"><span>Subtotal</span><span>{{ invoice().subtotal | currencyEgp }}</span></div>
            @if (invoice().discountAmount) {
              <div class="total-row discount"><span>Discount</span><span>-{{ invoice().discountAmount | currencyEgp }}</span></div>
            }
            <div class="total-row total"><span>Total Due</span><span>{{ invoice().totalAmount | currencyEgp }}</span></div>
            @if (invoice().paidAmount) {
              <div class="total-row paid"><span>Paid</span><span>{{ invoice().paidAmount | currencyEgp }}</span></div>
              <div class="total-row remaining"><span>Balance</span><span>{{ (invoice().totalAmount! - invoice().paidAmount!) | currencyEgp }}</span></div>
            }
          </div>
        </div>

        <!-- Actions sidebar -->
        @if (invoice().status !== 'Paid') {
          <div class="actions-panel">
            <!-- Apply Discount -->
            <div class="panel-section">
              <h4 class="panel-title">Apply Discount</h4>
              <div class="panel-form">
                <select class="field-input" [(ngModel)]="discount.type" name="discountType">
                  @for (dt of discountTypeKeys; track dt) { <option [value]="DiscountTypeEnum[dt]">{{ dt }}</option> }
                </select>
                <input class="field-input" type="number" [(ngModel)]="discount.value"
                  placeholder="{{ discount.type === DiscountTypeEnum.Percentage ? '0–100%' : 'Amount (EGP)' }}" />
                <button class="btn-secondary-sm" (click)="applyDiscount()">Apply</button>
              </div>
            </div>

            <!-- Record Payment -->
            <div class="panel-section">
              <h4 class="panel-title">Record Payment</h4>
              <div class="panel-form">
                <input class="field-input" type="number" [(ngModel)]="payment.amount"
                  placeholder="Amount (EGP)" [max]="invoice().totalAmount" />
                <select class="field-input" [(ngModel)]="payment.method" name="method">
                  @for (m of paymentMethodKeys; track m) { <option [value]="PaymentMethodEnum[m]">{{ m }}</option> }
                </select>
                <input class="field-input" type="text" [(ngModel)]="payment.notes" placeholder="Notes (optional)" />
                <button class="btn-primary" id="btn-pay" (click)="recordPayment()">
                  💳 Record Payment
                </button>
              </div>
            </div>
          </div>
        }
      </div>
    }
  `,
  styles: [`
    .invoice-layout { display: grid; grid-template-columns: 1fr 320px; gap: var(--space-5); align-items: start; }
    .invoice-card {
      background: var(--color-surface); border: 1px solid var(--color-border);
      border-radius: var(--radius-xl); padding: var(--space-6); box-shadow: var(--shadow-sm);
    }
    .invoice-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: var(--space-5); }
    .inv-number { font-size: var(--text-xl); font-weight: 700; color: var(--color-text-primary); }
    .inv-date { font-size: var(--text-sm); color: var(--color-text-secondary); margin-top: 2px; }
    .line-items { border: 1px solid var(--color-border); border-radius: var(--radius-md); overflow: hidden; margin-bottom: var(--space-5); }
    .line-item {
      display: grid; grid-template-columns: 1fr 60px 120px 120px;
      padding: var(--space-3) var(--space-4); border-bottom: 1px solid var(--color-border);
      font-size: var(--text-sm); align-items: center;
    }
    .line-item:last-child { border-bottom: none; }
    .item-desc { color: var(--color-text-primary); }
    .item-qty { color: var(--color-text-secondary); text-align: center; }
    .item-price, .item-total { text-align: right; font-family: var(--font-mono); font-size: var(--text-xs); }
    .item-total { font-weight: 600; color: var(--color-text-primary); }
    .invoice-totals { display: flex; flex-direction: column; gap: var(--space-2); }
    .total-row {
      display: flex; justify-content: space-between;
      font-size: var(--text-sm); padding: var(--space-2) 0;
      color: var(--color-text-secondary);
    }
    .total-row.total { font-size: var(--text-base); font-weight: 700; color: var(--color-text-primary); border-top: 1px solid var(--color-border); padding-top: var(--space-3); }
    .total-row.discount { color: var(--color-success); }
    .total-row.paid { color: var(--color-success); }
    .total-row.remaining { color: var(--color-danger); font-weight: 600; }
    .actions-panel { display: flex; flex-direction: column; gap: var(--space-4); }
    .panel-section {
      background: var(--color-surface); border: 1px solid var(--color-border);
      border-radius: var(--radius-lg); padding: var(--space-5);
    }
    .panel-title { font-size: var(--text-sm); font-weight: 600; margin-bottom: var(--space-4); color: var(--color-text-primary); }
    .panel-form { display: flex; flex-direction: column; gap: var(--space-3); }
    .field-input {
      padding: var(--space-3) var(--space-4); border: 1px solid var(--color-border-strong);
      border-radius: var(--radius-md); font-size: var(--text-sm); font-family: var(--font-family);
      color: var(--color-text-primary); background: var(--color-surface); outline: none;
    }
    .field-input:focus { border-color: var(--color-accent); }
    .btn-primary {
      background: var(--color-accent); color: white; border: none;
      border-radius: var(--radius-md); padding: var(--space-3) var(--space-5);
      font-size: var(--text-sm); font-weight: 600; cursor: pointer; font-family: var(--font-family);
    }
    .btn-secondary-sm {
      background: var(--color-surface-2); color: var(--color-text-primary);
      border: 1px solid var(--color-border); border-radius: var(--radius-md);
      padding: var(--space-2) var(--space-4); font-size: var(--text-sm); cursor: pointer; font-family: var(--font-family);
    }
    .btn-secondary {
      background: var(--color-surface-2); color: var(--color-text-primary);
      border: 1px solid var(--color-border); border-radius: var(--radius-md);
      padding: var(--space-2) var(--space-4); font-size: var(--text-sm); cursor: pointer; font-family: var(--font-family);
    }
  `],
})
export class InvoiceDetailComponent implements OnInit {
  private readonly invoicesClient = inject(InvoicesClient);
  private readonly notifications = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);
  readonly router = inject(Router);

  invoiceId = '';
  invoice = signal<any>(null);
  isLoading = signal(true);

  payment = { amount: 0, method: PaymentMethod.Cash as PaymentMethod, notes: '' };
  discount = { type: DiscountType.Percentage as DiscountType, value: 0 };
  DiscountTypeEnum = DiscountType;
  PaymentMethodEnum = PaymentMethod;
  paymentMethodKeys = Object.keys(PaymentMethod) as (keyof typeof PaymentMethod)[];
  discountTypeKeys = Object.keys(DiscountType) as (keyof typeof DiscountType)[];

  async ngOnInit() {
    this.invoiceId = this.route.snapshot.paramMap.get('id')!;
    await this.loadInvoice();
  }

  private async loadInvoice() {
    try {
      const result = await this.invoicesClient.getById(this.invoiceId);
      this.invoice.set(result);
      this.payment.amount = result.totalAmount ?? 0;
    } finally {
      this.isLoading.set(false);
    }
  }

  async applyDiscount() {
    try {
      const command = new ApplyDiscountCommand({
        invoiceId: this.invoiceId,
        discountAmount: this.discount.type === DiscountType.Percentage 
          ? (this.invoice()?.totalAmount ?? 0) * (this.discount.value / 100)
          : this.discount.value,
        reason: 'Applied via UI',
      });
      await this.invoicesClient.applyDiscount(this.invoiceId, command);
      this.notifications.success('Discount applied.');
      await this.loadInvoice();
    } catch (err: any) {
      this.notifications.error(err?.error?.detail ?? 'Failed to apply discount.');
    }
  }

  async recordPayment() {
    try {
      const command = new RecordPaymentCommand({
        invoiceId: this.invoiceId,
        amount: this.payment.amount,
        paymentMethod: this.payment.method as any,
        notes: this.payment.notes,
      });
      await this.invoicesClient.recordPayment(this.invoiceId, command);
      this.notifications.success('Payment recorded successfully!');
      await this.loadInvoice();
    } catch (err: any) {
      this.notifications.error(err?.error?.detail ?? 'Failed to record payment.');
    }
  }

  statusVariant(status: string | undefined): 'success' | 'warning' | 'danger' | 'default' {
    const map: Record<string, any> = { 'Paid': 'success', 'Overdue': 'danger', 'Pending': 'warning' };
    return map[status ?? ''] ?? 'default';
  }
}
