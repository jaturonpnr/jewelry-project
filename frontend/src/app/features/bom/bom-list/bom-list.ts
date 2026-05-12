import { CurrencyPipe, DatePipe } from '@angular/common';
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
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../../core/api/error-utils';
import { BomService } from '../bom.service';
import { BomTemplateSummary } from '../bom.types';

@Component({
  selector: 'app-bom-list',
  imports: [
    FormsModule, RouterLink, DatePipe, CurrencyPipe,
    MatTableModule, MatPaginatorModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatIconModule, MatButtonModule,
    MatChipsModule, MatProgressBarModule, MatTooltipModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h1>BOM Templates</h1>
        <p class="subtitle">Bill of Materials — material, stone &amp; labour cost per design</p>
      </div>
      <button mat-flat-button color="primary" routerLink="/bom/new">
        <mat-icon>add</mat-icon> New BOM
      </button>
    </div>

    <div class="toolbar">
      <mat-form-field appearance="outline" class="search">
        <mat-label>Search design code or name</mat-label>
        <input matInput [(ngModel)]="searchTerm" (keyup.enter)="onSearch()" placeholder="RNG-18K-D001" />
        <button matSuffix mat-icon-button (click)="onSearch()"><mat-icon>search</mat-icon></button>
      </mat-form-field>

      <mat-form-field appearance="outline" class="filter">
        <mat-label>Status</mat-label>
        <mat-select [(ngModel)]="activeFilter" (selectionChange)="onSearch()">
          <mat-option [value]="null">All</mat-option>
          <mat-option [value]="true">Active</mat-option>
          <mat-option [value]="false">Inactive</mat-option>
        </mat-select>
      </mat-form-field>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate" /> }
    @if (errorMessage(); as msg) {
      <div class="error"><mat-icon>error_outline</mat-icon><span>{{ msg }}</span></div>
    }

    <div class="table-container mat-elevation-z2">
      <table mat-table [dataSource]="items()">

        <ng-container matColumnDef="designCode">
          <th mat-header-cell *matHeaderCellDef>Design Code</th>
          <td mat-cell *matCellDef="let b">
            <div class="code">{{ b.designCode }}</div>
            <small class="muted">{{ b.designName }}</small>
          </td>
        </ng-container>

        <ng-container matColumnDef="lines">
          <th mat-header-cell *matHeaderCellDef>Lines</th>
          <td mat-cell *matCellDef="let b">
            <span class="badge" [matTooltip]="b.materialLineCount + ' material lines'">
              <mat-icon class="sm">category</mat-icon> {{ b.materialLineCount }}
            </span>
            <span class="badge" [matTooltip]="b.stoneLineCount + ' stone lines'">
              <mat-icon class="sm">diamond</mat-icon> {{ b.stoneLineCount }}
            </span>
            <span class="badge" [matTooltip]="b.laborLineCount + ' labour lines'">
              <mat-icon class="sm">build</mat-icon> {{ b.laborLineCount }}
            </span>
          </td>
        </ng-container>

        <ng-container matColumnDef="totalCost">
          <th mat-header-cell *matHeaderCellDef class="num">Cost / Piece</th>
          <td mat-cell *matCellDef="let b" class="num cost">
            {{ b.totalCostPerPieceThb | currency:'THB':'symbol-narrow':'1.2-2' }}
          </td>
        </ng-container>

        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let b">
            <mat-chip [class]="b.isActive ? 'chip-active' : 'chip-inactive'">
              {{ b.isActive ? 'Active' : 'Inactive' }}
            </mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="createdAt">
          <th mat-header-cell *matHeaderCellDef>Created</th>
          <td mat-cell *matCellDef="let b"><small class="muted">{{ b.createdAt | date:'mediumDate' }}</small></td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let b">
            <button mat-icon-button [routerLink]="['/bom', b.id]" matTooltip="View / Edit">
              <mat-icon>edit</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"
            class="clickable" [routerLink]="['/bom', row.id]"></tr>
        <tr class="empty-row" *matNoDataRow>
          <td [attr.colspan]="displayedColumns.length">
            @if (!loading()) { No BOM templates found. <a routerLink="/bom/new">Create one</a>. }
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
    .toolbar { display: flex; gap: 16px; margin-bottom: 16px; }
    .search { flex: 1; max-width: 400px; }
    .filter { width: 160px; }
    .table-container { background: #fff; border-radius: 8px; overflow: hidden; }
    table { width: 100%; }
    .num { text-align: right; }
    .code { font-weight: 500; font-family: monospace; }
    .muted { color: #607d8b; font-size: 12px; }
    .cost { font-weight: 600; color: #1a237e; }
    .clickable { cursor: pointer; }
    .clickable:hover { background: #f5f5f5; }
    .empty-row td { text-align: center; padding: 24px; color: #607d8b; }
    .error { display: flex; gap: 8px; align-items: center; padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin-bottom: 12px; }
    .badge { display: inline-flex; align-items: center; gap: 2px; margin-right: 8px; font-size: 12px; color: #546e7a; }
    .sm { font-size: 14px; }
    mat-chip.chip-active   { background: #c8e6c9; color: #1b5e20; }
    mat-chip.chip-inactive { background: #eceff1; color: #546e7a; }
  `],
})
export class BomList implements OnInit {
  private readonly svc = inject(BomService);

  readonly displayedColumns = ['designCode', 'lines', 'totalCost', 'status', 'createdAt', 'actions'];
  readonly items = signal<BomTemplateSummary[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(20);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  searchTerm = '';
  activeFilter: boolean | null = null;

  ngOnInit(): void { this.load(); }

  onSearch(): void { this.pageIndex.set(0); this.load(); }
  onPage(e: PageEvent): void { this.pageIndex.set(e.pageIndex); this.pageSize.set(e.pageSize); this.load(); }

  private load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.svc.getPaged(
      this.pageIndex() + 1, this.pageSize(),
      this.searchTerm.trim() || undefined,
      this.activeFilter ?? undefined,
    ).subscribe({
      next: (r) => { this.items.set(r.items as BomTemplateSummary[]); this.totalCount.set(r.totalCount); this.loading.set(false); },
      error: (err) => { this.loading.set(false); this.errorMessage.set(extractErrorMessage(err, 'Failed to load BOM templates.')); },
    });
  }
}
