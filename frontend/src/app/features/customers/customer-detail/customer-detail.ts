import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../../core/api/error-utils';
import { CustomerService } from '../customer.service';
import { Customer } from '../customer.types';

@Component({
  selector: 'app-customer-detail',
  imports: [
    RouterLink,
    DecimalPipe,
    DatePipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    MatProgressBarModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <button mat-icon-button routerLink="/customers" aria-label="Back">
        <mat-icon>arrow_back</mat-icon>
      </button>
      <div class="header-info">
        @if (customer(); as c) {
          <h1>{{ c.companyName }}</h1>
          <p class="subtitle">{{ c.code }} · {{ c.type }}</p>
        } @else {
          <h1>Loading…</h1>
        }
      </div>

      @if (customer(); as c) {
        <div class="header-actions">
          <button mat-stroked-button [routerLink]="['/customers', c.id, 'edit']">
            <mat-icon>edit</mat-icon> Edit
          </button>
          <button mat-stroked-button color="warn" (click)="onDelete(c)" [disabled]="deleting()">
            <mat-icon>delete</mat-icon> Delete
          </button>
        </div>
      }
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

    @if (customer(); as c) {
      <div class="grid">
        <mat-card>
          <mat-card-header>
            <mat-card-title>Contact</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <dl>
              <dt>Contact Person</dt><dd>{{ c.contactPerson || '—' }}</dd>
              <dt>Email</dt><dd>{{ c.email || '—' }}</dd>
              <dt>Phone</dt><dd>{{ c.phone || '—' }}</dd>
              <dt>Tax ID</dt><dd>{{ c.taxId || '—' }}</dd>
              <dt>Status</dt>
              <dd>
                @if (c.isActive) {
                  <mat-chip class="active">Active</mat-chip>
                } @else {
                  <mat-chip class="inactive">Inactive</mat-chip>
                }
              </dd>
            </dl>
          </mat-card-content>
        </mat-card>

        <mat-card>
          <mat-card-header>
            <mat-card-title>Commercial</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <dl>
              <dt>Type</dt><dd>{{ c.type }}</dd>
              <dt>Currency</dt><dd>{{ c.defaultCurrency }}</dd>
              <dt>Payment Terms</dt><dd>{{ c.paymentTerms }}</dd>
              <dt>Credit Limit</dt>
              <dd>{{ c.defaultCurrency }} {{ c.creditLimit | number: '1.2-2' }}</dd>
              <dt>Created</dt><dd>{{ c.createdAt | date: 'medium' }}</dd>
            </dl>
          </mat-card-content>
        </mat-card>

        <mat-card class="span-2">
          <mat-card-header>
            <mat-card-title>Address</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p class="address">
              {{ c.address.line1 }}
              @if (c.address.line2) {<br />{{ c.address.line2 }}}
              <br />{{ c.address.city }}{{ c.address.state ? ', ' + c.address.state : '' }}
              {{ c.address.postalCode }}<br />
              {{ c.address.country || '—' }}
            </p>
          </mat-card-content>
        </mat-card>

        @if (c.notes) {
          <mat-card class="span-2">
            <mat-card-header>
              <mat-card-title>Notes</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <p class="notes">{{ c.notes }}</p>
            </mat-card-content>
          </mat-card>
        }
      </div>
    }
  `,
  styles: [`
    .page-header {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 16px;
    }
    .header-info { flex: 1; }
    .header-info h1 { margin: 0 0 4px; font-weight: 500; }
    .subtitle { margin: 0; color: #607d8b; font-size: 13px; }
    .header-actions { display: flex; gap: 8px; }

    .grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }
    .span-2 { grid-column: span 2; }

    dl {
      display: grid;
      grid-template-columns: 140px 1fr;
      gap: 8px 16px;
      margin: 0;
    }
    dt { color: #607d8b; font-size: 13px; }
    dd { margin: 0; font-size: 14px; }

    .address { white-space: pre-wrap; line-height: 1.5; }
    .notes { white-space: pre-wrap; }

    mat-chip.active { background: #c8e6c9; color: #1b5e20; }
    mat-chip.inactive { background: #eeeeee; color: #616161; }

    .error {
      display: flex;
      gap: 8px;
      align-items: center;
      padding: 12px;
      background: #ffebee;
      color: #c62828;
      border-radius: 4px;
      margin: 12px 0;
    }

    @media (max-width: 800px) {
      .grid { grid-template-columns: 1fr; }
      .span-2 { grid-column: auto; }
    }
  `],
})
export class CustomerDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(CustomerService);
  private readonly snack = inject(MatSnackBar);

  readonly customer = signal<Customer | null>(null);
  readonly loading = signal(false);
  readonly deleting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (c) => {
        this.customer.set(c);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to load customer.'));
      },
    });
  }

  onDelete(c: Customer): void {
    if (!confirm(`Delete customer "${c.companyName}"? This soft-deletes the record.`)) return;
    this.deleting.set(true);
    this.service.delete(c.id).subscribe({
      next: () => {
        this.snack.open('Customer deleted', 'Close', { duration: 3000 });
        this.router.navigate(['/customers']);
      },
      error: (err) => {
        this.deleting.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to delete customer.'));
      },
    });
  }
}
