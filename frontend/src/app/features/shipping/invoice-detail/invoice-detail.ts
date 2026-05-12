import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../../core/api/error-utils';
import { ShippingService } from '../shipping.service';
import {
  CURRENCY_CODES,
  INVOICE_STATUS_LABELS,
  INVOICE_STATUS_TRANSITIONS,
  InvoiceResponse,
  InvoiceStatus,
  InvoiceStatusValue,
  UpdateInvoiceStatusDto,
} from '../shipping.types';

@Component({
  selector: 'app-invoice-detail',
  imports: [
    FormsModule, RouterLink, DatePipe, DecimalPipe,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatDividerModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatSnackBarModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h1>{{ invoice()?.invoiceNumber ?? 'Invoice Detail' }}</h1>
        <p class="subtitle">Commercial invoice</p>
      </div>
      <div class="header-actions">
        <button mat-button routerLink="/invoices">Back to list</button>
      </div>
    </div>

    @if (loading()) {
      <div class="loading">Loading…</div>
    }
    @if (errorMessage(); as msg) {
      <div class="error"><mat-icon>error_outline</mat-icon> {{ msg }}</div>
    }

    @if (invoice(); as inv) {
      <div class="cards-grid">

        <!-- Header info -->
        <mat-card>
          <mat-card-header><mat-card-title>Invoice Info</mat-card-title></mat-card-header>
          <mat-card-content>
            <div class="info-grid">
              <span class="label">Invoice #</span><strong>{{ inv.invoiceNumber }}</strong>
              <span class="label">Status</span>
              <mat-chip [class]="'inv-status-' + inv.status">{{ statusLabel(inv.status) }}</mat-chip>
              <span class="label">Customer</span><span>{{ inv.customerName }}</span>
              <span class="label">Sales Order</span><span>{{ inv.salesOrderNumber ?? '—' }}</span>
              <span class="label">Shipment</span><span>{{ inv.shipmentNumber ?? '—' }}</span>
              <span class="label">Invoice Date</span><span>{{ inv.invoiceDate | date:'mediumDate' }}</span>
              <span class="label">Due Date</span><span>{{ inv.dueDate | date:'mediumDate' }}</span>
              <span class="label">Payment Terms</span><span>{{ inv.paymentTerms ?? '—' }}</span>
              @if (inv.paidAt) {
                <span class="label">Paid At</span><span>{{ inv.paidAt | date:'medium' }}</span>
              }
              @if (inv.paymentReference) {
                <span class="label">Payment Ref</span><span>{{ inv.paymentReference }}</span>
              }
              @if (inv.notes) {
                <span class="label">Notes</span><span>{{ inv.notes }}</span>
              }
              <span class="label">Created</span><span>{{ inv.createdAt | date:'medium' }}</span>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Totals card -->
        <mat-card>
          <mat-card-header><mat-card-title>Financials</mat-card-title></mat-card-header>
          <mat-card-content>
            <div class="info-grid">
              <span class="label">Currency</span><span>{{ currencyCode(inv.currency) }}</span>
              <span class="label">Exchange Rate</span><span>{{ inv.exchangeRateToThb | number:'1.2-6' }}</span>
              <span class="label">VAT Applicable</span><span>{{ inv.vatApplicable ? 'Yes (' + inv.vatPercent + '%)' : 'No' }}</span>
              <mat-divider class="col-span"></mat-divider>
              <span class="label">Subtotal</span><span>{{ currencyCode(inv.currency) }} {{ inv.subtotal | number:'1.2-2' }}</span>
              @if (inv.discountAmount > 0) {
                <span class="label">Discount</span><span>- {{ currencyCode(inv.currency) }} {{ inv.discountAmount | number:'1.2-2' }}</span>
              }
              @if (inv.vatApplicable) {
                <span class="label">VAT ({{ inv.vatPercent }}%)</span><span>{{ currencyCode(inv.currency) }} {{ inv.vatAmount | number:'1.2-2' }}</span>
              }
              <span class="label"><strong>Total</strong></span>
              <span><strong>{{ currencyCode(inv.currency) }} {{ inv.totalAmount | number:'1.2-2' }}</strong></span>
              <span class="label">Total (THB)</span>
              <span>฿ {{ inv.totalAmountThb | number:'1.0-0' }}</span>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Status transition -->
        @if (nextStatuses().length > 0) {
          <mat-card>
            <mat-card-header><mat-card-title>Update Status</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="transition-form">
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>New Status</mat-label>
                  <mat-select [(ngModel)]="newStatus">
                    @for (ns of nextStatuses(); track ns) {
                      <mat-option [value]="ns">{{ statusLabel(ns) }}</mat-option>
                    }
                  </mat-select>
                </mat-form-field>

                @if (newStatus === 3) {
                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>Paid Date</mat-label>
                    <input matInput type="date" [(ngModel)]="paidAt" />
                  </mat-form-field>

                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>Payment Reference</mat-label>
                    <input matInput [(ngModel)]="paymentReference" placeholder="Bank ref / transfer no." />
                  </mat-form-field>
                }

                <button mat-flat-button color="primary"
                        [disabled]="!newStatus || saving()"
                        (click)="updateStatus()">
                  <mat-icon>check</mat-icon>
                  {{ saving() ? 'Saving…' : 'Confirm Status' }}
                </button>
              </div>
            </mat-card-content>
          </mat-card>
        }
      </div>

      <!-- Line items table -->
      <mat-card class="items-card">
        <mat-card-header><mat-card-title>Line Items ({{ inv.lineItems.length }})</mat-card-title></mat-card-header>
        <mat-card-content>
          <table mat-table [dataSource]="inv.lineItems" class="items-table">

            <ng-container matColumnDef="line">
              <th mat-header-cell *matHeaderCellDef class="num">#</th>
              <td mat-cell *matCellDef="let item" class="num">{{ item.lineNumber }}</td>
            </ng-container>

            <ng-container matColumnDef="description">
              <th mat-header-cell *matHeaderCellDef>Description</th>
              <td mat-cell *matCellDef="let item">
                {{ item.description }}
                @if (item.designCode) { <small class="muted">({{ item.designCode }})</small> }
              </td>
            </ng-container>

            <ng-container matColumnDef="qty">
              <th mat-header-cell *matHeaderCellDef class="num">Qty</th>
              <td mat-cell *matCellDef="let item" class="num">{{ item.quantity }}</td>
            </ng-container>

            <ng-container matColumnDef="unitPrice">
              <th mat-header-cell *matHeaderCellDef class="num">Unit Price</th>
              <td mat-cell *matCellDef="let item" class="num">{{ item.unitPrice | number:'1.2-2' }}</td>
            </ng-container>

            <ng-container matColumnDef="discount">
              <th mat-header-cell *matHeaderCellDef class="num">Disc %</th>
              <td mat-cell *matCellDef="let item" class="num">
                {{ item.discountPercent > 0 ? (item.discountPercent | number:'1.1-2') + '%' : '—' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="lineTotal">
              <th mat-header-cell *matHeaderCellDef class="num">Line Total</th>
              <td mat-cell *matCellDef="let item" class="num"><strong>{{ item.lineTotal | number:'1.2-2' }}</strong></td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="lineColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: lineColumns;"></tr>
          </table>
        </mat-card-content>
      </mat-card>
    }
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 16px; }
    .page-header h1 { margin: 0 0 4px; font-weight: 500; }
    .subtitle { margin: 0; color: #607d8b; font-size: 13px; }
    .header-actions { display: flex; gap: 8px; }
    .loading { padding: 24px; text-align: center; color: #607d8b; }
    .error { display: flex; gap: 8px; align-items: center; padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin-bottom: 12px; }
    .cards-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 16px; margin-bottom: 16px; }
    mat-card-content { padding-top: 8px !important; }
    .info-grid { display: grid; grid-template-columns: 140px 1fr; gap: 8px 12px; align-items: center; }
    .col-span { grid-column: 1 / -1; }
    .label { color: #607d8b; font-size: 13px; }
    .muted { color: #607d8b; }
    .transition-form { display: flex; flex-direction: column; gap: 12px; }
    .full-width { width: 100%; }
    .items-card { margin-top: 0; }
    .items-table { width: 100%; }
    .num { text-align: right; }
    mat-chip.inv-status-1 { background: #e8eaf6; color: #1a237e; }
    mat-chip.inv-status-2 { background: #fff3e0; color: #e65100; }
    mat-chip.inv-status-3 { background: #c8e6c9; color: #1b5e20; }
    mat-chip.inv-status-4 { background: #ffcdd2; color: #b71c1c; font-weight: 600; }
    mat-chip.inv-status-99 { background: #f5f5f5; color: #9e9e9e; }
  `],
})
export class InvoiceDetail implements OnInit {
  private readonly svc = inject(ShippingService);
  private readonly route = inject(ActivatedRoute);
  private readonly snack = inject(MatSnackBar);

  readonly invoice = signal<InvoiceResponse | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly nextStatuses = signal<InvoiceStatusValue[]>([]);

  readonly lineColumns = ['line', 'description', 'qty', 'unitPrice', 'discount', 'lineTotal'];

  newStatus: InvoiceStatusValue | null = null;
  paidAt = '';
  paymentReference = '';

  statusLabel(v: InvoiceStatusValue) { return INVOICE_STATUS_LABELS[v]; }
  currencyCode(v: number) { return CURRENCY_CODES[v as keyof typeof CURRENCY_CODES] ?? '?'; }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loading.set(true);
    this.svc.getInvoiceById(id).subscribe({
      next: (inv) => {
        this.invoice.set(inv);
        this.nextStatuses.set(INVOICE_STATUS_TRANSITIONS[inv.status] ?? []);
        this.loading.set(false);
      },
      error: (err) => { this.loading.set(false); this.errorMessage.set(extractErrorMessage(err, 'Failed to load invoice.')); },
    });
  }

  updateStatus(): void {
    if (!this.newStatus || !this.invoice()) return;
    const dto: UpdateInvoiceStatusDto = {
      newStatus: this.newStatus,
      paidAt: this.paidAt || null,
      paymentReference: this.paymentReference || null,
    };
    this.saving.set(true);
    this.svc.updateInvoiceStatus(this.invoice()!.id, dto).subscribe({
      next: () => {
        this.saving.set(false);
        this.snack.open('Status updated.', 'OK', { duration: 3000 });
        const id = this.route.snapshot.paramMap.get('id')!;
        this.svc.getInvoiceById(id).subscribe((inv) => {
          this.invoice.set(inv);
          this.nextStatuses.set(INVOICE_STATUS_TRANSITIONS[inv.status] ?? []);
          this.newStatus = null;
          this.paidAt = '';
          this.paymentReference = '';
        });
      },
      error: (err) => { this.saving.set(false); this.snack.open(extractErrorMessage(err, 'Update failed.'), 'OK', { duration: 5000 }); },
    });
  }
}
