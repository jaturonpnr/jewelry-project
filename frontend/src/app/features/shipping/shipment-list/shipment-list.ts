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
  SHIPMENT_STATUS_LABELS,
  ShipmentStatus,
  ShipmentStatusValue,
  ShipmentSummary,
} from '../shipping.types';

@Component({
  selector: 'app-shipment-list',
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
        <h1>Shipments</h1>
        <p class="subtitle">Packing &amp; dispatch records</p>
      </div>
      <button mat-flat-button color="primary" routerLink="/shipments/new">
        <mat-icon>add</mat-icon> New Shipment
      </button>
    </div>

    <div class="toolbar">
      <mat-form-field appearance="outline" class="search">
        <mat-label>Search (number / order / tracking)</mat-label>
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
          <th mat-header-cell *matHeaderCellDef>Shipment #</th>
          <td mat-cell *matCellDef="let s"><strong>{{ s.shipmentNumber }}</strong></td>
        </ng-container>

        <ng-container matColumnDef="salesOrder">
          <th mat-header-cell *matHeaderCellDef>Sales Order</th>
          <td mat-cell *matCellDef="let s">{{ s.salesOrderNumber ?? '—' }}</td>
        </ng-container>

        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let s">
            <mat-chip [class]="'status-' + s.status">{{ statusLabel(s.status) }}</mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="shipDate">
          <th mat-header-cell *matHeaderCellDef>Ship Date</th>
          <td mat-cell *matCellDef="let s">{{ s.shipDate ? (s.shipDate | date:'mediumDate') : '—' }}</td>
        </ng-container>

        <ng-container matColumnDef="carrier">
          <th mat-header-cell *matHeaderCellDef>Carrier / Tracking</th>
          <td mat-cell *matCellDef="let s">
            @if (s.carrier) { <span>{{ s.carrier }}</span> }
            @if (s.trackingNumber) { <small class="muted"> · {{ s.trackingNumber }}</small> }
            @if (!s.carrier && !s.trackingNumber) { — }
          </td>
        </ng-container>

        <ng-container matColumnDef="items">
          <th mat-header-cell *matHeaderCellDef class="num">Items</th>
          <td mat-cell *matCellDef="let s" class="num">{{ s.itemCount }}</td>
        </ng-container>

        <ng-container matColumnDef="weight">
          <th mat-header-cell *matHeaderCellDef class="num">Weight (g)</th>
          <td mat-cell *matCellDef="let s" class="num">{{ s.totalWeightGrams | number:'1.2-4' }}</td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let s">
            <button mat-icon-button [routerLink]="['/shipments', s.id]">
              <mat-icon>visibility</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"
            class="clickable" [routerLink]="['/shipments', row.id]"></tr>
        <tr class="empty-row" *matNoDataRow>
          <td [attr.colspan]="displayedColumns.length">
            @if (!loading()) { No shipments found. }
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
    .muted { color: #607d8b; }
    .clickable { cursor: pointer; }
    .clickable:hover { background: #f5f5f5; }
    .empty-row td { text-align: center; padding: 24px; color: #607d8b; }
    .error { display: flex; gap: 8px; align-items: center; padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin-bottom: 12px; }
    mat-chip.status-1 { background: #e3f2fd; color: #0d47a1; }
    mat-chip.status-2 { background: #fff3e0; color: #e65100; }
    mat-chip.status-3 { background: #fff9c4; color: #f57f17; }
    mat-chip.status-4 { background: #c8e6c9; color: #1b5e20; }
    mat-chip.status-5 { background: #ffcdd2; color: #b71c1c; }
  `],
})
export class ShipmentList implements OnInit {
  private readonly svc = inject(ShippingService);

  readonly displayedColumns = ['number', 'salesOrder', 'status', 'shipDate', 'carrier', 'items', 'weight', 'actions'];
  readonly items = signal<ShipmentSummary[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(20);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  search = '';
  statusFilter: ShipmentStatusValue | null = null;

  readonly statusOptions = Object.entries(SHIPMENT_STATUS_LABELS).map(([v, label]) => ({
    value: Number(v) as ShipmentStatusValue, label,
  }));

  statusLabel(v: ShipmentStatusValue) { return SHIPMENT_STATUS_LABELS[v]; }

  ngOnInit(): void { this.load(); }
  onSearch(): void { this.pageIndex.set(0); this.load(); }
  onPage(e: PageEvent): void { this.pageIndex.set(e.pageIndex); this.pageSize.set(e.pageSize); this.load(); }

  private load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.svc.getShipmentsPaged(
      this.pageIndex() + 1, this.pageSize(),
      this.statusFilter ?? undefined,
      this.search || undefined,
    ).subscribe({
      next: (r) => { this.items.set(r.items as ShipmentSummary[]); this.totalCount.set(r.totalCount); this.loading.set(false); },
      error: (err) => { this.loading.set(false); this.errorMessage.set(extractErrorMessage(err, 'Failed to load shipments.')); },
    });
  }
}
