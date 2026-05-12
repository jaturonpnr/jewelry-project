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
import { QcService } from '../qc.service';
import {
  QC_RESULT_LABELS,
  QC_TYPE_LABELS,
  QcInspectionSummary,
  QcInspectionTypeValue,
  QcResultValue,
} from '../qc.types';

@Component({
  selector: 'app-qc-list',
  imports: [
    FormsModule, RouterLink, DatePipe,
    MatTableModule, MatPaginatorModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatIconModule, MatButtonModule,
    MatChipsModule, MatProgressBarModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h1>QC Inspections</h1>
        <p class="subtitle">Incoming · In-Process · Final quality checks</p>
      </div>
      <button mat-flat-button color="primary" routerLink="/qc/new">
        <mat-icon>add</mat-icon> New Inspection
      </button>
    </div>

    <div class="toolbar">
      <mat-form-field appearance="outline" class="filter">
        <mat-label>Type</mat-label>
        <mat-select [(ngModel)]="typeFilter" (selectionChange)="onSearch()">
          <mat-option [value]="null">All Types</mat-option>
          @for (opt of typeOptions; track opt.value) {
            <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
          }
        </mat-select>
      </mat-form-field>

      <mat-form-field appearance="outline" class="filter">
        <mat-label>Result</mat-label>
        <mat-select [(ngModel)]="resultFilter" (selectionChange)="onSearch()">
          <mat-option [value]="null">All Results</mat-option>
          @for (opt of resultOptions; track opt.value) {
            <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
          }
        </mat-select>
      </mat-form-field>

      <mat-form-field appearance="outline" class="filter">
        <mat-label>From Date</mat-label>
        <input matInput type="date" [(ngModel)]="fromDate" (change)="onSearch()" />
      </mat-form-field>

      <mat-form-field appearance="outline" class="filter">
        <mat-label>To Date</mat-label>
        <input matInput type="date" [(ngModel)]="toDate" (change)="onSearch()" />
      </mat-form-field>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate" /> }
    @if (errorMessage(); as msg) {
      <div class="error"><mat-icon>error_outline</mat-icon><span>{{ msg }}</span></div>
    }

    <div class="table-container mat-elevation-z2">
      <table mat-table [dataSource]="items()">

        <ng-container matColumnDef="date">
          <th mat-header-cell *matHeaderCellDef>Date</th>
          <td mat-cell *matCellDef="let q">{{ q.inspectionDate | date:'mediumDate' }}</td>
        </ng-container>

        <ng-container matColumnDef="type">
          <th mat-header-cell *matHeaderCellDef>Type</th>
          <td mat-cell *matCellDef="let q">
            <mat-chip [class]="'type-' + q.inspectionType">{{ typeLabel(q.inspectionType) }}</mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="reference">
          <th mat-header-cell *matHeaderCellDef>Reference</th>
          <td mat-cell *matCellDef="let q">
            @if (q.workOrderNumber) {
              <span class="ref">{{ q.workOrderNumber }}</span>
            }
            @if (q.rawMaterialDescription) {
              <span class="ref muted">{{ q.rawMaterialDescription }}</span>
            }
            @if (!q.workOrderNumber && !q.rawMaterialDescription) { — }
          </td>
        </ng-container>

        <ng-container matColumnDef="inspector">
          <th mat-header-cell *matHeaderCellDef>Inspector</th>
          <td mat-cell *matCellDef="let q"><small>{{ q.inspectorName }}</small></td>
        </ng-container>

        <ng-container matColumnDef="result">
          <th mat-header-cell *matHeaderCellDef>Result</th>
          <td mat-cell *matCellDef="let q">
            <mat-chip [class]="'result-' + q.result">{{ resultLabel(q.result) }}</mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="defects">
          <th mat-header-cell *matHeaderCellDef class="num">Defects</th>
          <td mat-cell *matCellDef="let q" class="num">
            @if (q.defectCount > 0) {
              <span class="defect-badge">{{ q.defectCount }}</span>
            } @else { — }
          </td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let q">
            <button mat-icon-button [routerLink]="['/qc', q.id]">
              <mat-icon>visibility</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"
            class="clickable" [routerLink]="['/qc', row.id]"></tr>
        <tr class="empty-row" *matNoDataRow>
          <td [attr.colspan]="displayedColumns.length">
            @if (!loading()) { No inspections found. }
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
    .filter { width: 160px; }
    .table-container { background: #fff; border-radius: 8px; overflow: hidden; }
    table { width: 100%; }
    .num { text-align: right; }
    .muted { color: #607d8b; }
    .ref { font-weight: 500; }
    .clickable { cursor: pointer; }
    .clickable:hover { background: #f5f5f5; }
    .empty-row td { text-align: center; padding: 24px; color: #607d8b; }
    .error { display: flex; gap: 8px; align-items: center; padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin-bottom: 12px; }
    .defect-badge { background: #ff8f00; color: #fff; border-radius: 10px; padding: 2px 8px; font-size: 12px; font-weight: 600; }
    /* type chips */
    mat-chip.type-1 { background: #e3f2fd; color: #0d47a1; }
    mat-chip.type-2 { background: #fff3e0; color: #e65100; }
    mat-chip.type-3 { background: #f3e5f5; color: #6a1b9a; }
    /* result chips */
    mat-chip.result-1 { background: #c8e6c9; color: #1b5e20; }
    mat-chip.result-2 { background: #fff9c4; color: #f57f17; }
    mat-chip.result-3 { background: #ffcdd2; color: #b71c1c; font-weight: 600; }
  `],
})
export class QcList implements OnInit {
  private readonly svc = inject(QcService);

  readonly displayedColumns = ['date', 'type', 'reference', 'inspector', 'result', 'defects', 'actions'];
  readonly items = signal<QcInspectionSummary[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(20);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  typeFilter: QcInspectionTypeValue | null = null;
  resultFilter: QcResultValue | null = null;
  fromDate = '';
  toDate = '';

  readonly typeOptions = Object.entries(QC_TYPE_LABELS).map(([v, label]) => ({
    value: Number(v) as QcInspectionTypeValue, label,
  }));
  readonly resultOptions = Object.entries(QC_RESULT_LABELS).map(([v, label]) => ({
    value: Number(v) as QcResultValue, label,
  }));

  typeLabel(v: QcInspectionTypeValue) { return QC_TYPE_LABELS[v]; }
  resultLabel(v: QcResultValue) { return QC_RESULT_LABELS[v]; }

  ngOnInit(): void { this.load(); }
  onSearch(): void { this.pageIndex.set(0); this.load(); }
  onPage(e: PageEvent): void { this.pageIndex.set(e.pageIndex); this.pageSize.set(e.pageSize); this.load(); }

  private load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.svc.getPaged(
      this.pageIndex() + 1, this.pageSize(),
      this.typeFilter ?? undefined,
      this.resultFilter ?? undefined,
      undefined,
      this.fromDate || undefined,
      this.toDate || undefined,
    ).subscribe({
      next: (r) => { this.items.set(r.items as QcInspectionSummary[]); this.totalCount.set(r.totalCount); this.loading.set(false); },
      error: (err) => { this.loading.set(false); this.errorMessage.set(extractErrorMessage(err, 'Failed to load inspections.')); },
    });
  }
}
