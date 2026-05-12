import { DatePipe } from '@angular/common';
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
import { WorkOrderQuery, WorkOrderService } from '../work-order.service';
import { WorkOrder, WorkOrderPriority, WorkOrderStatus } from '../work-order.types';

@Component({
  selector: 'app-work-order-list',
  imports: [
    FormsModule,
    RouterLink,
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
        <h1>Work Orders</h1>
        <p class="subtitle">9-stage production workflow · Wax → Casting → Setting → Polishing → … → Packaging</p>
      </div>
      <button mat-flat-button color="primary" disabled>
        <mat-icon>add</mat-icon> New Work Order
      </button>
    </div>

    <div class="toolbar">
      <mat-form-field appearance="outline" class="search">
        <mat-label>Search by WO # or description</mat-label>
        <input matInput [(ngModel)]="searchTerm" (keyup.enter)="onSearch()" placeholder="WO-2026-0001" />
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

      <mat-form-field appearance="outline" class="filter">
        <mat-label>Priority</mat-label>
        <mat-select [(ngModel)]="priorityFilter" (selectionChange)="onSearch()">
          <mat-option [value]="null">All</mat-option>
          @for (opt of priorityOptions; track opt.value) {
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
      <table mat-table [dataSource]="items()">
        <ng-container matColumnDef="number">
          <th mat-header-cell *matHeaderCellDef>WO #</th>
          <td mat-cell *matCellDef="let w">{{ w.workOrderNumber }}</td>
        </ng-container>

        <ng-container matColumnDef="description">
          <th mat-header-cell *matHeaderCellDef>Description</th>
          <td mat-cell *matCellDef="let w">
            <div>{{ w.description }}</div>
            @if (w.designCode) { <small class="muted">{{ w.designCode }}</small> }
          </td>
        </ng-container>

        <ng-container matColumnDef="qty">
          <th mat-header-cell *matHeaderCellDef class="num">Qty</th>
          <td mat-cell *matCellDef="let w" class="num">{{ w.quantity }}</td>
        </ng-container>

        <ng-container matColumnDef="priority">
          <th mat-header-cell *matHeaderCellDef>Priority</th>
          <td mat-cell *matCellDef="let w">
            <mat-chip [class]="'priority-' + w.priority.toLowerCase()">{{ w.priority }}</mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let w">
            <mat-chip [class]="'wo-status wo-status-' + w.status.toLowerCase()">{{ w.status }}</mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="progress">
          <th mat-header-cell *matHeaderCellDef>Progress</th>
          <td mat-cell *matCellDef="let w">
            <div class="progress-bar" [title]="completedCount(w) + ' of ' + nonSkippedCount(w) + ' active stages done'">
              <div class="progress-fill" [style.width.%]="progressPercent(w)"></div>
            </div>
            <small class="muted">{{ completedCount(w) }} / {{ nonSkippedCount(w) }} stages</small>
          </td>
        </ng-container>

        <ng-container matColumnDef="schedule">
          <th mat-header-cell *matHeaderCellDef>Schedule</th>
          <td mat-cell *matCellDef="let w">
            @if (w.scheduledStart || w.scheduledEnd) {
              <small>{{ w.scheduledStart | date: 'mediumDate' }} → {{ w.scheduledEnd | date: 'mediumDate' }}</small>
            } @else { — }
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr
          mat-row
          *matRowDef="let row; columns: displayedColumns;"
          class="clickable"
          [routerLink]="['/work-orders', row.id]"
        ></tr>
        <tr class="empty-row" *matNoDataRow>
          <td [attr.colspan]="displayedColumns.length">
            @if (!loading()) { No work orders yet. }
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
      display: flex; justify-content: space-between; align-items: flex-end;
      margin-bottom: 16px;
    }
    .page-header h1 { margin: 0 0 4px; font-weight: 500; }
    .subtitle { margin: 0; color: #607d8b; font-size: 13px; }
    .toolbar { display: flex; gap: 16px; margin-bottom: 16px; }
    .search { flex: 1; max-width: 400px; }
    .filter { width: 180px; }
    .table-container { background: #fff; border-radius: 8px; overflow: hidden; }
    table { width: 100%; }
    .num { text-align: right; }
    .muted { color: #607d8b; font-size: 12px; }
    .clickable { cursor: pointer; }
    .clickable:hover { background: #f5f5f5; }
    .empty-row td { text-align: center; padding: 24px; color: #607d8b; }
    .error {
      display: flex; gap: 8px; align-items: center;
      padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin-bottom: 12px;
    }

    .progress-bar {
      width: 120px; height: 6px; background: #eceff1; border-radius: 3px; overflow: hidden;
    }
    .progress-fill { height: 100%; background: #43a047; transition: width .3s; }

    /* Priority chips */
    mat-chip.priority-low      { background: #eceff1; color: #455a64; }
    mat-chip.priority-normal   { background: #e0f7fa; color: #006064; }
    mat-chip.priority-high     { background: #fff3e0; color: #e65100; }
    mat-chip.priority-urgent   { background: #ffcdd2; color: #b71c1c; font-weight: 600; }

    /* WO status chips */
    mat-chip.wo-status-draft      { background: #eceff1; color: #455a64; }
    mat-chip.wo-status-released   { background: #e3f2fd; color: #0d47a1; }
    mat-chip.wo-status-inprogress { background: #fff3e0; color: #e65100; }
    mat-chip.wo-status-onhold     { background: #fff8e1; color: #6d4c00; }
    mat-chip.wo-status-completed  { background: #c8e6c9; color: #1b5e20; }
    mat-chip.wo-status-cancelled  { background: #ffcdd2; color: #b71c1c; }
  `],
})
export class WorkOrderList implements OnInit {
  private readonly service = inject(WorkOrderService);

  readonly displayedColumns = ['number', 'description', 'qty', 'priority', 'status', 'progress', 'schedule'];

  readonly items = signal<WorkOrder[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(20);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  searchTerm = '';
  statusFilter: number | null = null;
  priorityFilter: number | null = null;

  readonly statusOptions = [
    { value: WorkOrderStatus.Draft, label: 'Draft' },
    { value: WorkOrderStatus.Released, label: 'Released' },
    { value: WorkOrderStatus.InProgress, label: 'In Progress' },
    { value: WorkOrderStatus.OnHold, label: 'On Hold' },
    { value: WorkOrderStatus.Completed, label: 'Completed' },
    { value: WorkOrderStatus.Cancelled, label: 'Cancelled' },
  ];

  readonly priorityOptions = [
    { value: WorkOrderPriority.Low, label: 'Low' },
    { value: WorkOrderPriority.Normal, label: 'Normal' },
    { value: WorkOrderPriority.High, label: 'High' },
    { value: WorkOrderPriority.Urgent, label: 'Urgent' },
  ];

  ngOnInit(): void { this.load(); }

  onSearch(): void { this.pageIndex.set(0); this.load(); }
  onPage(e: PageEvent): void { this.pageIndex.set(e.pageIndex); this.pageSize.set(e.pageSize); this.load(); }

  completedCount(w: WorkOrder): number {
    return w.stages.filter((s) => s.status === 'Completed').length;
  }
  nonSkippedCount(w: WorkOrder): number {
    return w.stages.filter((s) => s.status !== 'Skipped').length;
  }
  progressPercent(w: WorkOrder): number {
    const total = this.nonSkippedCount(w);
    return total > 0 ? Math.round((this.completedCount(w) / total) * 100) : 0;
  }

  private load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    const req: WorkOrderQuery = {
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      search: this.searchTerm.trim() || undefined,
      status: this.statusFilter ?? undefined,
      priority: this.priorityFilter ?? undefined,
    };
    this.service.getPaged(req).subscribe({
      next: (paged) => {
        this.items.set(paged.items);
        this.totalCount.set(paged.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to load work orders.'));
      },
    });
  }
}
