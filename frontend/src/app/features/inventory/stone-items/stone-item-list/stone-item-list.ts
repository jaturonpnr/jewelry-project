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
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { extractErrorMessage } from '../../../../core/api/error-utils';
import { InventoryService } from '../../inventory.service';
import { StoneItem } from '../../inventory.types';

@Component({
  selector: 'app-stone-item-list',
  imports: [
    FormsModule,
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
        <mat-label>Search by item code or certificate #</mat-label>
        <input
          matInput
          [(ngModel)]="searchTerm"
          (keyup.enter)="onSearch()"
          placeholder="STN-2026-0001 or GIA-1234567"
        />
        <button matSuffix mat-icon-button (click)="onSearch()" aria-label="Search">
          <mat-icon>search</mat-icon>
        </button>
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
        <ng-container matColumnDef="code">
          <th mat-header-cell *matHeaderCellDef>Item Code</th>
          <td mat-cell *matCellDef="let s">{{ s.itemCode }}</td>
        </ng-container>

        <ng-container matColumnDef="material">
          <th mat-header-cell *matHeaderCellDef>Stone</th>
          <td mat-cell *matCellDef="let s">{{ s.materialTypeName }}</td>
        </ng-container>

        <ng-container matColumnDef="carat">
          <th mat-header-cell *matHeaderCellDef class="num">Carat</th>
          <td mat-cell *matCellDef="let s" class="num">
            <strong>{{ s.caratWeight | number: '1.2-4' }}</strong> ct
          </td>
        </ng-container>

        <ng-container matColumnDef="quality">
          <th mat-header-cell *matHeaderCellDef>Quality</th>
          <td mat-cell *matCellDef="let s">
            <div class="quality-cell">
              @if (s.color) { <span class="grade">{{ s.color }}</span> }
              @if (s.clarity) { <span class="grade">{{ s.clarity }}</span> }
              @if (s.cut) { <small class="muted">{{ s.cut }}</small> }
            </div>
          </td>
        </ng-container>

        <ng-container matColumnDef="shape">
          <th mat-header-cell *matHeaderCellDef>Shape</th>
          <td mat-cell *matCellDef="let s">{{ s.shape }}</td>
        </ng-container>

        <ng-container matColumnDef="cert">
          <th mat-header-cell *matHeaderCellDef>Certificate</th>
          <td mat-cell *matCellDef="let s">
            @if (s.certAuthority !== 'None') {
              <div class="cert-cell">
                <mat-chip class="cert">{{ s.certAuthority }}</mat-chip>
                @if (s.certificateNumber) {
                  <small class="muted">{{ s.certificateNumber }}</small>
                }
              </div>
            } @else { — }
          </td>
        </ng-container>

        <ng-container matColumnDef="origin">
          <th mat-header-cell *matHeaderCellDef>Origin</th>
          <td mat-cell *matCellDef="let s">{{ s.origin || '—' }}</td>
        </ng-container>

        <ng-container matColumnDef="cost">
          <th mat-header-cell *matHeaderCellDef class="num">Unit Cost</th>
          <td mat-cell *matCellDef="let s" class="num">
            {{ s.costCurrency }} {{ s.unitCost | number: '1.2-2' }}
          </td>
        </ng-container>

        <ng-container matColumnDef="received">
          <th mat-header-cell *matHeaderCellDef>Received</th>
          <td mat-cell *matCellDef="let s">{{ s.receivedDate | date: 'mediumDate' }}</td>
        </ng-container>

        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let s">
            <mat-chip [class]="'status-' + s.status.toLowerCase()">{{ s.status }}</mat-chip>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        <tr class="empty-row" *matNoDataRow>
          <td [attr.colspan]="displayedColumns.length">
            @if (!loading()) { No individual stones in stock yet. }
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
    .toolbar-row { margin-bottom: 16px; }
    .search { width: 100%; max-width: 480px; }
    .table-container { background: #fff; border-radius: 8px; overflow: hidden; }
    table { width: 100%; }
    .num { text-align: right; }
    .quality-cell { display: flex; gap: 6px; align-items: center; }
    .grade { font-family: monospace; font-weight: 600; padding: 2px 6px; background: #fff8e1; color: #6d4c00; border-radius: 3px; font-size: 11px; }
    .cert-cell { display: flex; flex-direction: column; gap: 2px; }
    .muted { color: #607d8b; font-size: 12px; }
    .empty-row td { text-align: center; padding: 24px; color: #607d8b; }
    .error {
      display: flex; gap: 8px; align-items: center;
      padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin-bottom: 12px;
    }
    mat-chip.cert { background: #e8eaf6; color: #1a237e; font-weight: 500; height: 20px; font-size: 11px; }
    mat-chip.status-instock    { background: #c8e6c9; color: #1b5e20; }
    mat-chip.status-reserved   { background: #fff3e0; color: #e65100; }
    mat-chip.status-inproduction { background: #e3f2fd; color: #0d47a1; }
    mat-chip.status-sold       { background: #eceff1; color: #455a64; }
    mat-chip.status-writtenoff { background: #ffcdd2; color: #b71c1c; }
  `],
})
export class StoneItemList implements OnInit {
  private readonly service = inject(InventoryService);

  readonly displayedColumns = ['code', 'material', 'carat', 'quality', 'shape', 'cert', 'origin', 'cost', 'received', 'status'];

  readonly items = signal<StoneItem[]>([]);
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

  private load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.service.getStoneItems({
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
        this.errorMessage.set(extractErrorMessage(err, 'Failed to load stone items.'));
      },
    });
  }
}
