import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../../../core/api/error-utils';
import { InventoryService } from '../../inventory.service';
import { RawMaterialItem } from '../../inventory.types';
import {
  AdjustRawMaterialDialog,
  AdjustRawMaterialDialogData,
  AdjustRawMaterialDialogResult,
} from '../adjust-raw-material-dialog/adjust-raw-material-dialog';

@Component({
  selector: 'app-raw-material-list',
  imports: [
    FormsModule,
    RouterLink,
    DecimalPipe,
    DatePipe,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressBarModule,
    MatTooltipModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toolbar-row">
      <mat-form-field appearance="outline" class="search">
        <mat-label>Search by lot, code, or material name</mat-label>
        <input
          matInput
          [(ngModel)]="searchTerm"
          (keyup.enter)="onSearch()"
          placeholder="e.g. LOT-2026-001 or GOLD_18K"
        />
        <button matSuffix mat-icon-button (click)="onSearch()" aria-label="Search">
          <mat-icon>search</mat-icon>
        </button>
      </mat-form-field>

      <button mat-flat-button color="primary" routerLink="receive">
        <mat-icon>add</mat-icon> Receive Stock
      </button>
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
        <ng-container matColumnDef="material">
          <th mat-header-cell *matHeaderCellDef>Material</th>
          <td mat-cell *matCellDef="let r">
            <div class="cell">
              <span>{{ r.materialTypeName }}</span>
              <small class="muted">{{ r.materialTypeCode }}</small>
            </div>
          </td>
        </ng-container>

        <ng-container matColumnDef="lot">
          <th mat-header-cell *matHeaderCellDef>Lot</th>
          <td mat-cell *matCellDef="let r">{{ r.lotNumber }}</td>
        </ng-container>

        <ng-container matColumnDef="location">
          <th mat-header-cell *matHeaderCellDef>Location</th>
          <td mat-cell *matCellDef="let r">{{ r.location }}</td>
        </ng-container>

        <ng-container matColumnDef="quantity">
          <th mat-header-cell *matHeaderCellDef class="num">Quantity</th>
          <td mat-cell *matCellDef="let r" class="num">
            <strong>{{ r.quantity | number: '1.0-4' }}</strong>
            <small class="unit">{{ r.materialUnit }}</small>
          </td>
        </ng-container>

        <ng-container matColumnDef="pureWeight">
          <th mat-header-cell *matHeaderCellDef class="num">
            Pure Weight
            <mat-icon class="hint" matTooltip="For metals only — gross × purity">help_outline</mat-icon>
          </th>
          <td mat-cell *matCellDef="let r" class="num">
            @if (r.pureWeight !== null && r.pureWeight !== undefined) {
              {{ r.pureWeight | number: '1.0-4' }}<small class="unit">g</small>
            } @else { — }
          </td>
        </ng-container>

        <ng-container matColumnDef="unitCost">
          <th mat-header-cell *matHeaderCellDef class="num">Unit Cost</th>
          <td mat-cell *matCellDef="let r" class="num">
            {{ r.costCurrency }} {{ r.unitCost | number: '1.2-2' }}
          </td>
        </ng-container>

        <ng-container matColumnDef="received">
          <th mat-header-cell *matHeaderCellDef>Received</th>
          <td mat-cell *matCellDef="let r">{{ r.receivedDate | date: 'mediumDate' }}</td>
        </ng-container>

        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let r">
            <mat-chip [class]="'status-' + r.status.toLowerCase()">{{ r.status }}</mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef class="num">Actions</th>
          <td mat-cell *matCellDef="let r" class="num">
            <button
              mat-icon-button
              (click)="openAdjust(r)"
              matTooltip="Adjust quantity"
              [disabled]="r.status !== 'InStock'"
            >
              <mat-icon>tune</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        <tr class="empty-row" *matNoDataRow>
          <td [attr.colspan]="displayedColumns.length">
            @if (!loading()) { No raw material in stock yet. Click "Receive Stock" to add. }
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
    .toolbar-row {
      display: flex;
      gap: 16px;
      align-items: flex-start;
      margin-bottom: 16px;
    }
    .search { flex: 1; max-width: 480px; }
    .table-container { background: #fff; border-radius: 8px; overflow: hidden; }
    table { width: 100%; }
    .num { text-align: right; }
    .cell { display: flex; flex-direction: column; }
    .muted { color: #607d8b; font-size: 12px; }
    .unit { color: #9e9e9e; font-size: 11px; margin-left: 4px; text-transform: lowercase; }
    .hint { font-size: 14px; height: 14px; width: 14px; vertical-align: middle; margin-left: 2px; color: #9e9e9e; }
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
    mat-chip.status-instock { background: #c8e6c9; color: #1b5e20; }
    mat-chip.status-reserved { background: #fff3e0; color: #e65100; }
    mat-chip.status-inproduction { background: #e3f2fd; color: #0d47a1; }
    mat-chip.status-sold { background: #eceff1; color: #455a64; }
    mat-chip.status-writtenoff { background: #ffcdd2; color: #b71c1c; }
  `],
})
export class RawMaterialList implements OnInit {
  private readonly service = inject(InventoryService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = [
    'material', 'lot', 'location', 'quantity', 'pureWeight',
    'unitCost', 'received', 'status', 'actions',
  ];

  readonly items = signal<RawMaterialItem[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(20);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  searchTerm = '';

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

  openAdjust(r: RawMaterialItem): void {
    const ref = this.dialog.open<
      AdjustRawMaterialDialog,
      AdjustRawMaterialDialogData,
      AdjustRawMaterialDialogResult | undefined
    >(AdjustRawMaterialDialog, {
      width: '460px',
      data: { item: r },
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.service.adjustRawMaterial(r.id, {
        quantityDelta: result.quantityDelta,
        reason: result.reason,
        notes: result.notes ?? null,
        rowVersion: r.rowVersion,
      }).subscribe({
        next: () => {
          this.snack.open('Quantity adjusted', 'Close', { duration: 3000 });
          this.load();
        },
        error: (err) => {
          this.snack.open(extractErrorMessage(err, 'Adjustment failed'), 'Close', { duration: 5000 });
        },
      });
    });
  }

  private load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.service.getRawMaterials({
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      search: this.searchTerm.trim() || undefined,
    }).subscribe({
      next: (paged) => {
        this.items.set(paged.items);
        this.totalCount.set(paged.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to load raw materials.'));
      },
    });
  }
}
