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
import { extractErrorMessage } from '../../../core/api/error-utils';
import { SalesOrderQuery, SalesOrderService } from '../sales-order.service';
import { SalesOrder, SalesOrderStatus, SalesOrderStatusName } from '../sales-order.types';

@Component({
  selector: 'app-sales-order-list',
  imports: [
    FormsModule,
    DecimalPipe,
    DatePipe,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressBarModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h1>Sales Orders</h1>
        <p class="subtitle">B2B orders — Draft → Confirmed → InProduction → QC → Packed → Shipped → Delivered</p>
      </div>
      <button mat-flat-button color="primary" disabled>
        <mat-icon>add</mat-icon> New Order
      </button>
    </div>

    <div class="toolbar">
      <mat-form-field appearance="outline" class="search">
        <mat-label>Search by order # or customer</mat-label>
        <input
          matInput
          [(ngModel)]="searchTerm"
          (keyup.enter)="onSearch()"
          placeholder="e.g. SO-2026-0001"
        />
        <button matSuffix mat-icon-button (click)="onSearch()" aria-label="Search">
          <mat-icon>search</mat-icon>
        </button>
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
      <div class="error">
        <mat-icon>error_outline</mat-icon>
        <span>{{ msg }}</span>
      </div>
    }

    <div class="table-container mat-elevation-z2">
      <table mat-table [dataSource]="orders()">
        <ng-container matColumnDef="orderNumber">
          <th mat-header-cell *matHeaderCellDef>Order #</th>
          <td mat-cell *matCellDef="let o">{{ o.orderNumber }}</td>
        </ng-container>

        <ng-container matColumnDef="customer">
          <th mat-header-cell *matHeaderCellDef>Customer</th>
          <td mat-cell *matCellDef="let o">
            <div class="company-cell">
              <span>{{ o.customerName }}</span>
              <small class="contact">{{ o.customerCode }}</small>
            </div>
          </td>
        </ng-container>

        <ng-container matColumnDef="orderDate">
          <th mat-header-cell *matHeaderCellDef>Order Date</th>
          <td mat-cell *matCellDef="let o">{{ o.orderDate | date: 'mediumDate' }}</td>
        </ng-container>

        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let o">
            <mat-chip [class]="'status-' + statusClass(o.status)">{{ o.status }}</mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="items">
          <th mat-header-cell *matHeaderCellDef class="num">Items</th>
          <td mat-cell *matCellDef="let o" class="num">{{ o.items.length }}</td>
        </ng-container>

        <ng-container matColumnDef="total">
          <th mat-header-cell *matHeaderCellDef class="num">Total</th>
          <td mat-cell *matCellDef="let o" class="num">
            <strong>{{ o.currency }} {{ o.totalAmount | number: '1.2-2' }}</strong>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        <tr class="empty-row" *matNoDataRow>
          <td [attr.colspan]="displayedColumns.length">
            @if (!loading()) { No orders found. }
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
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-end;
      margin-bottom: 16px;
    }
    .page-header h1 { margin: 0 0 4px; font-weight: 500; }
    .subtitle { margin: 0; color: #607d8b; font-size: 13px; }
    .toolbar {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
    }
    .search { flex: 1; max-width: 480px; }
    .filter { width: 200px; }
    .table-container { background: #fff; border-radius: 8px; overflow: hidden; }
    table { width: 100%; }
    .num { text-align: right; }
    .company-cell { display: flex; flex-direction: column; }
    .contact { color: #607d8b; font-size: 12px; }
    .empty-row td { text-align: center; padding: 24px; color: #607d8b; }
    .error {
      display: flex;
      gap: 8px;
      align-items: center;
      padding: 12px;
      background: #ffebee;
      color: #c62828;
      border-radius: 4px;
      margin-bottom: 12px;
    }

    /* Status chip colors — visual lifecycle */
    mat-chip.status-draft        { background: #eceff1; color: #455a64; }
    mat-chip.status-confirmed    { background: #e3f2fd; color: #0d47a1; }
    mat-chip.status-inproduction { background: #fff3e0; color: #e65100; }
    mat-chip.status-qc           { background: #f3e5f5; color: #6a1b9a; }
    mat-chip.status-packed       { background: #e0f7fa; color: #006064; }
    mat-chip.status-shipped      { background: #e8eaf6; color: #1a237e; }
    mat-chip.status-delivered    { background: #c8e6c9; color: #1b5e20; }
    mat-chip.status-cancelled    { background: #ffcdd2; color: #b71c1c; }
  `],
})
export class SalesOrderList implements OnInit {
  private readonly service = inject(SalesOrderService);

  readonly displayedColumns = ['orderNumber', 'customer', 'orderDate', 'status', 'items', 'total'];

  readonly orders = signal<SalesOrder[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(20);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  searchTerm = '';
  statusFilter: number | null = null;

  readonly statusOptions = [
    { value: SalesOrderStatus.Draft, label: 'Draft' },
    { value: SalesOrderStatus.Confirmed, label: 'Confirmed' },
    { value: SalesOrderStatus.InProduction, label: 'In Production' },
    { value: SalesOrderStatus.Qc, label: 'QC' },
    { value: SalesOrderStatus.Packed, label: 'Packed' },
    { value: SalesOrderStatus.Shipped, label: 'Shipped' },
    { value: SalesOrderStatus.Delivered, label: 'Delivered' },
    { value: SalesOrderStatus.Cancelled, label: 'Cancelled' },
  ];

  ngOnInit(): void { this.load(); }

  onSearch(): void {
    this.pageIndex.set(0);
    this.load();
  }

  onPage(e: PageEvent): void {
    this.pageIndex.set(e.pageIndex);
    this.pageSize.set(e.pageSize);
    this.load();
  }

  statusClass(s: SalesOrderStatusName): string {
    return s.toLowerCase();
  }

  private load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    const req: SalesOrderQuery = {
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      search: this.searchTerm.trim() || undefined,
      status: this.statusFilter ?? undefined,
    };
    this.service.getPaged(req).subscribe({
      next: (paged) => {
        this.orders.set(paged.items);
        this.totalCount.set(paged.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to load sales orders.'));
      },
    });
  }
}
