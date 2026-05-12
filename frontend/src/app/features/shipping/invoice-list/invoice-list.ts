import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../../core/api/error-utils';
import { ShippingService } from '../shipping.service';
import {
  CURRENCY_CODES,
  INVOICE_STATUS_LABELS,
  InvoiceStatusValue,
  InvoiceSummary,
} from '../shipping.types';

@Component({
  selector: 'app-invoice-list',
  imports: [
    FormsModule, RouterLink, DatePipe, DecimalPipe,
    MatTableModule, MatPaginatorModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatIconModule, MatButtonModule,
    MatChipsModule, MatProgressBarModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h1>Invoices</h1>
        <p class="subtitle">Commercial invoices &amp; payment tracking</p>
      </div>
      <button mat-flat-button color="primary" routerLink="/invoices/new">
        <mat-icon>add</mat-icon> New Invoice
      </button>
    </div>

    <div class="toolbar">
      <mat-form-field appearance="outline" class="search">
        <mat-label>Search (number / customer / order)</mat-label>
        <input matInput [(ngModel)]="search" (keyup.enter)="onSearch()" />
        <mat-icon matSuffix>search</mat-icon>
      </mat-form-field>

      <mat-form-field appearance="outline" class="filter">
        <mat-label>Status</mat-label>
        <mat-select [(ngModel)]="statusFilter" (selectionChange)="onSearch()">
          <mat-option [value]="null">All</mat-option>
          @for (opt of statusOptions; track opt.value) {
            <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate" /> }
    @if (errorMessage(); as msg) {
      <div class="error"><mat-icon>error_outline</mat-icon><span>{{ msg }}</span></div>
    }

    <div class="table-container mat-elevation-z2">
      <table mat-table [dataSource]="items()">

        <ng-container matColumnDef="number">
          <th mat-header-cell *matHeaderCellDef>Invoice #</th>
          <td mat-cell *matCellDef="let inv"><strong>{{ inv.invoiceNumber }}</strong></td>
        </ng-container>

        <ng-container matColumnDef="customer">
          <th mat-header-cell *matHeaderCellDef>Customer</th>
          <td mat-cell *matCellDef="let inv">{{ inv.customerName }}</td>
        </ng-container>

        <ng-container matColumnDef="salesOrder">
          <th mat-header-cell *matHeaderCellDef>Sales Order</th>
          <td mat-cell *matCellDef="let inv">{{ inv.salesOrderNumber ?? '—' }}</td>
        </ng-container>

        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let inv">
            <mat-chip [class]="'inv-status-' + inv.status">{{ statusLabel(inv.status) }}</mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="invoiceDate">
          <th mat-header-cell *matHeaderCellDef>Invoice Date</th>
          <td mat-cell *matCellDef="let inv">{{ inv.invoiceDate | date:'mediumDate' }}</td>
        </ng-container>

        <ng-container matColumnDef="dueDate">
          <th mat-header-cell *matHeaderCellDef>Due Date</th>
          <td mat-cell *matCellDef="let inv">{{ inv.dueDate | date:'mediumDate' }}</td>
        </ng-container>

        <ng-container matColumnDef="amount">
          <th mat-header-cell *matHeaderCellDef class="num">Amount</th>
          <td mat-cell *matCellDef="let inv" class="num">
            {{ currencyCode(inv.currency) }} {{ inv.totalAmount | number:'1.2-2' }}
          </td>
        </ng-container>

        <ng-container matColumnDef="amountThb">
          <th mat-header-cell *matHeaderCellDef class="num">THB</th>
          <td mat-cell *matCellDef="let inv" class="num">{{ inv.totalAmountThb | number:'1.0-0' }}</td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let inv">
            <button mat-icon-button [routerLink]="['/invoices', inv.id]">
              <mat-icon>visibility</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"
            class="clickable" [routerLink]="['/invoices', row.id]"></tr>
        <tr class="empty-row" *matNoDataRow>
          <td [attr.colspan]="displayedColumns.length">
            @if (!loading()) { No invoices found. }
          </td>
        </tr>
      </table>

      <mat-paginator
        [length]="totalCount()"
        [pageSize]="pageSize()"
        [pageIndex]="pageIndex()"
        [pageSizeOptions]="[10, 20, 50]"
        (page)="onPage($event)"
        showFirstLastButtons
      />
    </div>
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 16px; }
    .page-header h1 { margin: 0 0 4px; font-weight: 500; }
    .subtitle { margin: 0; color: #607d8b; font-size: 13px; }
    .toolbar { display: flex; gap: 12px; flex-wrap: wrap; margin-bottom: 16px; }
    .search { width: 280px; }
    .filter { width: 160px; }
    .table-container { background: #fff; border-radius: 8px; overflow: hidden; }
    table { width: 100%; }
    .num { text-align: right; }
    .clickable { cursor: pointer; }
    .clickable:hover { background: #f5f5f5; }
    .empty-row td { text-align: center; padding: 24px; color: #607d8b; }
    .error { display: flex; gap: 8px; align-items: center; padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin-bottom: 12px; }
    mat-chip.inv-status-1 { background: #e8eaf6; color: #1a237e; }
    mat-chip.inv-status-2 { background: #fff3e0; color: #e65100; }
    mat-chip.inv-status-3 { background: #c8e6c9; color: #1b5e20; }
    mat-chip.inv-status-4 { background: #ffcdd2; color: #b71c1c; font-weight: 600; }
    mat-chip.inv-status-99 { background: #f5f5f5; color: #9e9e9e; }
  `],
})
export class InvoiceList implements OnInit {
  private readonly svc = inject(ShippingService);

  readonly displayedColumns = ['number', 'customer', 'salesOrder', 'status', 'invoiceDate', 'dueDate', 'amount', 'amountThb', 'actions'];
  readonly items = signal<InvoiceSummary[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(20);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  search = '';
  statusFilter: InvoiceStatusValue | null = null;

  readonly statusOptions = Object.entries(INVOICE_STATUS_LABELS).map(([v, label]) => ({
    value: Number(v) as InvoiceStatusValue, label,
  }));

  statusLabel(v: InvoiceStatusValue) { return INVOICE_STATUS_LABELS[v]; }
  currencyCode(v: number) { return CURRENCY_CODES[v as keyof typeof CURRENCY_CODES] ?? '?'; }

  ngOnInit(): void { this.load(); }
  onSearch(): void { this.pageIndex.set(0); this.load(); }
  onPage(e: PageEvent): void { this.pageIndex.set(e.pageIndex); this.pageSize.set(e.pageSize); this.load(); }

  private load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.svc.getInvoicesPaged(
      this.pageIndex() + 1, this.pageSize(),
      this.statusFilter ?? undefined,
      this.search || undefined,
    ).subscribe({
      next: (r) => { this.items.set(r.items as InvoiceSummary[]); this.totalCount.set(r.totalCount); this.loading.set(false); },
      error: (err) => { this.loading.set(false); this.errorMessage.set(extractErrorMessage(err, 'Failed to load invoices.')); },
    });
  }
}
