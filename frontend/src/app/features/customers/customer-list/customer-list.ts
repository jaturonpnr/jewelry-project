import { DecimalPipe } from '@angular/common';
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
import { CustomerService } from '../customer.service';
import { Customer } from '../customer.types';

@Component({
  selector: 'app-customer-list',
  imports: [
    FormsModule,
    DecimalPipe,
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
        <h1>Customers</h1>
        <p class="subtitle">B2B customers — wholesalers, retailers, catalogue publishers</p>
      </div>
      <button mat-flat-button color="primary" disabled>
        <mat-icon>add</mat-icon> New Customer
      </button>
    </div>

    <div class="toolbar">
      <mat-form-field appearance="outline" class="search">
        <mat-label>Search by code, name, contact</mat-label>
        <input
          matInput
          [(ngModel)]="searchTerm"
          (keyup.enter)="onSearch()"
          placeholder="e.g. CUST-0001 or Goldsmith"
        />
        <button matSuffix mat-icon-button (click)="onSearch()" aria-label="Search">
          <mat-icon>search</mat-icon>
        </button>
      </mat-form-field>
    </div>

    @if (loading()) {
      <mat-progress-bar mode="indeterminate" />
    }

    @if (errorMessage(); as msg) {
      <div class="error">
        <mat-icon>error_outline</mat-icon>
        <span>{{ msg }}</span>
      </div>
    }

    <div class="table-container mat-elevation-z2">
      <table mat-table [dataSource]="customers()">
        <ng-container matColumnDef="code">
          <th mat-header-cell *matHeaderCellDef>Code</th>
          <td mat-cell *matCellDef="let c">{{ c.code }}</td>
        </ng-container>

        <ng-container matColumnDef="companyName">
          <th mat-header-cell *matHeaderCellDef>Company</th>
          <td mat-cell *matCellDef="let c">
            <div class="company-cell">
              <span>{{ c.companyName }}</span>
              @if (c.contactPerson) {
                <small class="contact">{{ c.contactPerson }}</small>
              }
            </div>
          </td>
        </ng-container>

        <ng-container matColumnDef="type">
          <th mat-header-cell *matHeaderCellDef>Type</th>
          <td mat-cell *matCellDef="let c">
            <mat-chip>{{ c.type }}</mat-chip>
          </td>
        </ng-container>

        <ng-container matColumnDef="country">
          <th mat-header-cell *matHeaderCellDef>Country</th>
          <td mat-cell *matCellDef="let c">{{ c.address.country || '—' }}</td>
        </ng-container>

        <ng-container matColumnDef="creditLimit">
          <th mat-header-cell *matHeaderCellDef class="num">Credit Limit</th>
          <td mat-cell *matCellDef="let c" class="num">
            {{ c.defaultCurrency }} {{ c.creditLimit | number:'1.2-2' }}
          </td>
        </ng-container>

        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Active</th>
          <td mat-cell *matCellDef="let c">
            @if (c.isActive) {
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
            @if (!loading()) { No customers found. }
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
    .table-container {
      background: #fff;
      border-radius: 8px;
      overflow: hidden;
    }
    table { width: 100%; }
    .num { text-align: right; }
    .company-cell { display: flex; flex-direction: column; }
    .contact { color: #607d8b; font-size: 12px; }
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
  `],
})
export class CustomerList implements OnInit {
  private readonly service = inject(CustomerService);

  readonly displayedColumns = ['code', 'companyName', 'type', 'country', 'creditLimit', 'status'];

  readonly customers = signal<Customer[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(20);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  searchTerm = '';

  ngOnInit(): void {
    this.load();
  }

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

    this.service
      .getPaged({
        page: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        search: this.searchTerm.trim() || undefined,
      })
      .subscribe({
        next: (paged) => {
          this.customers.set(paged.items);
          this.totalCount.set(paged.totalCount);
          this.loading.set(false);
        },
        error: (err) => {
          this.loading.set(false);
          this.errorMessage.set(err?.error?.errors?.[0]?.message ?? 'Failed to load customers.');
        },
      });
  }
}
