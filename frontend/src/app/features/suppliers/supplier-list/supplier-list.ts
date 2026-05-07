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
import { extractErrorMessage } from '../../../core/api/error-utils';
import { SupplierService } from '../supplier.service';
import { Supplier } from '../supplier.types';

@Component({
  selector: 'app-supplier-list',
  imports: [
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressBarModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h1>Suppliers</h1>
        <p class="subtitle">Gold, silver, gemstone, findings & equipment suppliers</p>
      </div>
      <button mat-flat-button color="primary" disabled>
        <mat-icon>add</mat-icon> New Supplier
      </button>
    </div>

    <div class="toolbar">
      <mat-form-field appearance="outline" class="search">
        <mat-label>Search by code or name</mat-label>
        <input
          matInput
          [(ngModel)]="searchTerm"
          (keyup.enter)="onSearch()"
          placeholder="e.g. SUP-DIA-001"
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
      <table mat-table [dataSource]="suppliers()">
        <ng-container matColumnDef="code">
          <th mat-header-cell *matHeaderCellDef>Code</th>
          <td mat-cell *matCellDef="let s">{{ s.code }}</td>
        </ng-container>

        <ng-container matColumnDef="companyName">
          <th mat-header-cell *matHeaderCellDef>Company</th>
          <td mat-cell *matCellDef="let s">
            <div class="company-cell">
              <span>{{ s.companyName }}</span>
              @if (s.contactPerson) { <small class="contact">{{ s.contactPerson }}</small> }
            </div>
          </td>
        </ng-container>

        <ng-container matColumnDef="type">
          <th mat-header-cell *matHeaderCellDef>Type</th>
          <td mat-cell *matCellDef="let s">
            <mat-chip [class]="'type-' + s.type.toLowerCase()">{{ s.type }}</mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="country">
          <th mat-header-cell *matHeaderCellDef>Country</th>
          <td mat-cell *matCellDef="let s">{{ s.address.country || '—' }}</td>
        </ng-container>

        <ng-container matColumnDef="certifications">
          <th mat-header-cell *matHeaderCellDef>Certifications</th>
          <td mat-cell *matCellDef="let s">
            @if (s.certifications) {
              <span class="cert">{{ s.certifications }}</span>
            } @else { — }
          </td>
        </ng-container>

        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Active</th>
          <td mat-cell *matCellDef="let s">
            @if (s.isActive) {
              <mat-icon class="active-icon">check_circle</mat-icon>
            } @else {
              <mat-icon class="inactive-icon">cancel</mat-icon>
            }
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        <tr class="empty-row" *matNoDataRow>
          <td [attr.colspan]="displayedColumns.length">
            @if (!loading()) { No suppliers found. }
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
    .toolbar { margin-bottom: 16px; }
    .search { width: 100%; max-width: 480px; }
    .table-container { background: #fff; border-radius: 8px; overflow: hidden; }
    table { width: 100%; }
    .company-cell { display: flex; flex-direction: column; }
    .contact { color: #607d8b; font-size: 12px; }
    .cert { font-size: 12px; color: #455a64; }
    .active-icon { color: #43a047; }
    .inactive-icon { color: #bdbdbd; }
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

    /* Type-specific chip colors */
    mat-chip.type-gold      { background: #fff8e1; color: #6d4c00; }
    mat-chip.type-silver    { background: #eceff1; color: #455a64; }
    mat-chip.type-preciousstone { background: #e8f5e9; color: #1b5e20; }
    mat-chip.type-findings  { background: #e3f2fd; color: #0d47a1; }
    mat-chip.type-equipment { background: #f3e5f5; color: #4a148c; }
    mat-chip.type-consumable { background: #fbe9e7; color: #bf360c; }
  `],
})
export class SupplierList implements OnInit {
  private readonly service = inject(SupplierService);

  readonly displayedColumns = ['code', 'companyName', 'type', 'country', 'certifications', 'status'];

  readonly suppliers = signal<Supplier[]>([]);
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
    this.service.getPaged({
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      search: this.searchTerm.trim() || undefined,
    }).subscribe({
      next: (paged) => {
        this.suppliers.set(paged.items);
        this.totalCount.set(paged.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to load suppliers.'));
      },
    });
  }
}
