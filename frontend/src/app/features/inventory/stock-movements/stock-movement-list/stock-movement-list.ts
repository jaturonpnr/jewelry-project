import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { extractErrorMessage } from '../../../../core/api/error-utils';
import { InventoryService, MovementQuery } from '../../inventory.service';
import { StockMovement } from '../../inventory.types';

@Component({
  selector: 'app-stock-movement-list',
  imports: [
    FormsModule,
    DecimalPipe,
    DatePipe,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatSelectModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressBarModule,
    MatTooltipModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toolbar-row">
      <mat-form-field appearance="outline" class="filter">
        <mat-label>Item Type</mat-label>
        <mat-select [(ngModel)]="itemTypeFilter" (selectionChange)="onSearch()">
          <mat-option [value]="null">All</mat-option>
          <mat-option [value]="1">Raw Material</mat-option>
          <mat-option [value]="2">Stone Item</mat-option>
          <mat-option [value]="3">Stone Parcel</mat-option>
          <mat-option [value]="4">Finished Goods</mat-option>
        </mat-select>
      </mat-form-field>

      <mat-form-field appearance="outline" class="filter">
        <mat-label>Movement Type</mat-label>
        <mat-select [(ngModel)]="movementTypeFilter" (selectionChange)="onSearch()">
          <mat-option [value]="null">All</mat-option>
          <mat-option [value]="1">Receipt</mat-option>
          <mat-option [value]="2">Issue</mat-option>
          <mat-option [value]="3">Adjustment</mat-option>
          <mat-option [value]="4">Transfer</mat-option>
          <mat-option [value]="5">Reserve</mat-option>
          <mat-option [value]="6">Release</mat-option>
          <mat-option [value]="7">Write Off</mat-option>
          <mat-option [value]="8">Return</mat-option>
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
        <ng-container matColumnDef="time">
          <th mat-header-cell *matHeaderCellDef>When</th>
          <td mat-cell *matCellDef="let m">{{ m.performedAt | date: 'short' }}</td>
        </ng-container>

        <ng-container matColumnDef="itemType">
          <th mat-header-cell *matHeaderCellDef>Item Type</th>
          <td mat-cell *matCellDef="let m">
            <mat-chip class="item-type">{{ m.itemType }}</mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="movementType">
          <th mat-header-cell *matHeaderCellDef>Movement</th>
          <td mat-cell *matCellDef="let m">
            <mat-chip [class]="'movement movement-' + m.movementType.toLowerCase()">
              {{ m.movementType }}
            </mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="delta">
          <th mat-header-cell *matHeaderCellDef class="num">Δ</th>
          <td mat-cell *matCellDef="let m" class="num" [class.positive]="m.quantityDelta > 0" [class.negative]="m.quantityDelta < 0">
            <strong>{{ m.quantityDelta > 0 ? '+' : '' }}{{ m.quantityDelta | number: '1.0-4' }}</strong>
          </td>
        </ng-container>

        <ng-container matColumnDef="after">
          <th mat-header-cell *matHeaderCellDef class="num">After</th>
          <td mat-cell *matCellDef="let m" class="num">{{ m.quantityAfter | number: '1.0-4' }}</td>
        </ng-container>

        <ng-container matColumnDef="reason">
          <th mat-header-cell *matHeaderCellDef>Reason / Notes</th>
          <td mat-cell *matCellDef="let m">
            <div>{{ m.reason || '—' }}</div>
            @if (m.notes) { <small class="muted">{{ m.notes }}</small> }
          </td>
        </ng-container>

        <ng-container matColumnDef="ref">
          <th mat-header-cell *matHeaderCellDef>Reference</th>
          <td mat-cell *matCellDef="let m">
            @if (m.referenceType) {
              <small class="muted">{{ m.referenceType }}</small>
              @if (m.referenceId) {
                <small class="ref-id" matTooltip="{{ m.referenceId }}">
                  {{ m.referenceId.substring(0, 8) }}…
                </small>
              }
            } @else { — }
          </td>
        </ng-container>

        <ng-container matColumnDef="performedBy">
          <th mat-header-cell *matHeaderCellDef>By</th>
          <td mat-cell *matCellDef="let m">
            <small class="muted" matTooltip="{{ m.performedBy }}">
              {{ m.performedBy.substring(0, 8) }}…
            </small>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        <tr class="empty-row" *matNoDataRow>
          <td [attr.colspan]="displayedColumns.length">
            @if (!loading()) { No movements recorded yet. }
          </td>
        </tr>
      </table>

      <mat-paginator
        [length]="totalCount()"
        [pageSize]="pageSize()"
        [pageIndex]="pageIndex()"
        [pageSizeOptions]="[20, 50, 100]"
        (page)="onPage($event)"
        showFirstLastButtons
      />
    </div>
  `,
  styles: [`
    .toolbar-row { display: flex; gap: 12px; margin-bottom: 16px; }
    .filter { width: 200px; }
    .table-container { background: #fff; border-radius: 8px; overflow: hidden; }
    table { width: 100%; }
    .num { text-align: right; }
    .muted { color: #607d8b; font-size: 12px; }
    .ref-id { font-family: monospace; margin-left: 6px; }
    .positive { color: #2e7d32; }
    .negative { color: #c62828; }
    .empty-row td { text-align: center; padding: 24px; color: #607d8b; }
    .error {
      display: flex; gap: 8px; align-items: center;
      padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin-bottom: 12px;
    }
    mat-chip.item-type { background: #eceff1; color: #455a64; height: 22px; font-size: 11px; }
    mat-chip.movement { height: 22px; font-size: 11px; }
    mat-chip.movement-receipt    { background: #c8e6c9; color: #1b5e20; }
    mat-chip.movement-issue      { background: #ffcdd2; color: #b71c1c; }
    mat-chip.movement-adjustment { background: #fff8e1; color: #6d4c00; }
    mat-chip.movement-transfer   { background: #e3f2fd; color: #0d47a1; }
    mat-chip.movement-reserve    { background: #fff3e0; color: #e65100; }
    mat-chip.movement-release    { background: #f3e5f5; color: #6a1b9a; }
    mat-chip.movement-writeoff   { background: #ffebee; color: #b71c1c; }
    mat-chip.movement-return     { background: #e0f7fa; color: #006064; }
  `],
})
export class StockMovementList implements OnInit {
  private readonly service = inject(InventoryService);

  readonly displayedColumns = ['time', 'itemType', 'movementType', 'delta', 'after', 'reason', 'ref', 'performedBy'];

  readonly items = signal<StockMovement[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(50);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  itemTypeFilter: number | null = null;
  movementTypeFilter: number | null = null;

  ngOnInit(): void { this.load(); }

  onSearch(): void { this.pageIndex.set(0); this.load(); }
  onPage(e: PageEvent): void { this.pageIndex.set(e.pageIndex); this.pageSize.set(e.pageSize); this.load(); }

  private load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    const req: MovementQuery = {
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      itemType: this.itemTypeFilter ?? undefined,
      movementType: this.movementTypeFilter ?? undefined,
    };
    this.service.getMovements(req).subscribe({
      next: (paged) => {
        this.items.set(paged.items);
        this.totalCount.set(paged.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to load stock movements.'));
      },
    });
  }
}
