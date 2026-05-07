import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../../core/api/error-utils';
import { CancelOrderDialog, CancelOrderDialogData } from '../cancel-order-dialog/cancel-order-dialog';
import { SalesOrderService } from '../sales-order.service';
import {
  nextAllowedStatuses,
  SalesOrder,
  SalesOrderStatus,
  SalesOrderStatusValue,
  STATUS_ICONS,
  STATUS_LABELS,
} from '../sales-order.types';

@Component({
  selector: 'app-sales-order-detail',
  imports: [
    RouterLink,
    DatePipe,
    DecimalPipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    MatTableModule,
    MatProgressBarModule,
    MatTooltipModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <button mat-icon-button routerLink="/sales-orders" aria-label="Back">
        <mat-icon>arrow_back</mat-icon>
      </button>
      <div class="header-info">
        @if (order(); as o) {
          <h1>{{ o.orderNumber }}</h1>
          <p class="subtitle">
            {{ o.customerName }} · {{ o.customerCode }}
            <mat-chip [class]="'status-chip status-' + o.status.toLowerCase()">
              {{ statusLabel(o.status) }}
            </mat-chip>
          </p>
        } @else {
          <h1>Loading…</h1>
        }
      </div>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate" /> }

    @if (errorMessage(); as msg) {
      <div class="error">
        <mat-icon>error_outline</mat-icon>
        <span>{{ msg }}</span>
      </div>
    }

    @if (order(); as o) {
      <!-- Status timeline -->
      <mat-card class="timeline-card">
        <mat-card-content>
          <div class="timeline">
            @for (step of timeline(); track step.label) {
              <div class="step" [class.done]="step.done" [class.current]="step.current">
                <div class="dot">
                  <mat-icon>{{ step.done ? 'check' : step.icon }}</mat-icon>
                </div>
                <div class="label">{{ step.label }}</div>
                @if (step.timestamp) {
                  <div class="ts">{{ step.timestamp | date: 'short' }}</div>
                }
              </div>
            }
          </div>
        </mat-card-content>
      </mat-card>

      <!-- Status transition actions -->
      @if (allowedNextStatuses().length > 0) {
        <mat-card class="actions-card">
          <mat-card-header>
            <mat-card-title>Actions</mat-card-title>
            <mat-card-subtitle>Move this order to the next stage</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <div class="actions">
              @for (next of allowedNextStatuses(); track next) {
                @if (next === STATUS.Cancelled) {
                  <button
                    mat-stroked-button
                    color="warn"
                    [disabled]="changing()"
                    (click)="onCancel()"
                  >
                    <mat-icon>cancel</mat-icon> Cancel order
                  </button>
                } @else {
                  <button
                    mat-flat-button
                    color="primary"
                    [disabled]="changing()"
                    (click)="onTransition(next)"
                  >
                    <mat-icon>{{ icons[next] }}</mat-icon>
                    Move to {{ statusLabelFromValue(next) }}
                  </button>
                }
              }
            </div>
          </mat-card-content>
        </mat-card>
      }

      <!-- Items table -->
      <mat-card class="section">
        <mat-card-header>
          <mat-card-title>Line Items</mat-card-title>
          <mat-card-subtitle>{{ o.items.length }} item(s) · all amounts in {{ o.currency }}</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <table mat-table [dataSource]="o.items" class="items-table">
            <ng-container matColumnDef="line">
              <th mat-header-cell *matHeaderCellDef class="num">#</th>
              <td mat-cell *matCellDef="let i" class="num">{{ i.lineNumber }}</td>
            </ng-container>

            <ng-container matColumnDef="description">
              <th mat-header-cell *matHeaderCellDef>Description</th>
              <td mat-cell *matCellDef="let i">
                <div>{{ i.description }}</div>
                @if (i.designCode) { <small class="muted">Design: {{ i.designCode }}</small> }
                @if (i.notes) { <small class="muted">{{ i.notes }}</small> }
              </td>
            </ng-container>

            <ng-container matColumnDef="quantity">
              <th mat-header-cell *matHeaderCellDef class="num">Qty</th>
              <td mat-cell *matCellDef="let i" class="num">{{ i.quantity }}</td>
            </ng-container>

            <ng-container matColumnDef="unitPrice">
              <th mat-header-cell *matHeaderCellDef class="num">Unit Price</th>
              <td mat-cell *matCellDef="let i" class="num">{{ i.unitPrice | number: '1.2-2' }}</td>
            </ng-container>

            <ng-container matColumnDef="discount">
              <th mat-header-cell *matHeaderCellDef class="num">Discount</th>
              <td mat-cell *matCellDef="let i" class="num">
                @if (i.lineDiscount > 0) { -{{ i.lineDiscount | number: '1.2-2' }} } @else { — }
              </td>
            </ng-container>

            <ng-container matColumnDef="lineTotal">
              <th mat-header-cell *matHeaderCellDef class="num">Line Total</th>
              <td mat-cell *matCellDef="let i" class="num">
                <strong>{{ i.lineTotal | number: '1.2-2' }}</strong>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="itemColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: itemColumns;"></tr>
          </table>
        </mat-card-content>
      </mat-card>

      <!-- Summary cards -->
      <div class="summary-grid">
        <mat-card>
          <mat-card-header>
            <mat-card-title>Totals</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <dl class="totals">
              <dt>Subtotal</dt>
              <dd>{{ o.currency }} {{ o.subtotal | number: '1.2-2' }}</dd>
              @if (o.discountAmount > 0) {
                <dt>Discount</dt>
                <dd class="neg">-{{ o.currency }} {{ o.discountAmount | number: '1.2-2' }}</dd>
              }
              @if (o.taxAmount > 0) {
                <dt>Tax</dt>
                <dd>{{ o.currency }} {{ o.taxAmount | number: '1.2-2' }}</dd>
              }
              @if (o.shippingCost > 0) {
                <dt>Shipping</dt>
                <dd>{{ o.currency }} {{ o.shippingCost | number: '1.2-2' }}</dd>
              }
              <dt class="grand">Total</dt>
              <dd class="grand">{{ o.currency }} {{ o.totalAmount | number: '1.2-2' }}</dd>
            </dl>
          </mat-card-content>
        </mat-card>

        <mat-card>
          <mat-card-header>
            <mat-card-title>Order Info</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <dl>
              <dt>Order Date</dt><dd>{{ o.orderDate | date: 'mediumDate' }}</dd>
              <dt>Requested Delivery</dt>
              <dd>{{ o.requestedDeliveryDate ? (o.requestedDeliveryDate | date: 'mediumDate') : '—' }}</dd>
              <dt>Currency</dt><dd>{{ o.currency }}</dd>
              @if (o.exchangeRateToBase) {
                <dt>Exchange Rate</dt>
                <dd>{{ o.exchangeRateToBase | number: '1.4-4' }} (vs THB)</dd>
              }
              @if (o.trackingNumber) {
                <dt>Tracking #</dt><dd>{{ o.trackingNumber }}</dd>
              }
              @if (o.cancellationReason) {
                <dt>Cancellation Reason</dt>
                <dd class="neg">{{ o.cancellationReason }}</dd>
              }
            </dl>
          </mat-card-content>
        </mat-card>

        <mat-card class="span-2">
          <mat-card-header>
            <mat-card-title>Shipping Address</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p class="address">
              {{ o.shippingAddress.line1 || '—' }}
              @if (o.shippingAddress.line2) {<br />{{ o.shippingAddress.line2 }}}
              <br />{{ o.shippingAddress.city }}{{ o.shippingAddress.state ? ', ' + o.shippingAddress.state : '' }}
              {{ o.shippingAddress.postalCode }}<br />
              {{ o.shippingAddress.country || '—' }}
            </p>
          </mat-card-content>
        </mat-card>

        @if (o.notes) {
          <mat-card class="span-2">
            <mat-card-header>
              <mat-card-title>Notes</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <p class="notes">{{ o.notes }}</p>
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
    .subtitle { margin: 0; color: #607d8b; font-size: 13px; display: flex; align-items: center; gap: 8px; }

    .timeline-card { margin-bottom: 16px; }
    .timeline {
      display: flex;
      gap: 8px;
      align-items: flex-start;
      overflow-x: auto;
    }
    .step {
      display: flex;
      flex-direction: column;
      align-items: center;
      min-width: 96px;
      flex: 1;
      position: relative;
    }
    .step:not(:last-child)::after {
      content: '';
      position: absolute;
      top: 18px;
      left: 60%;
      width: 80%;
      height: 2px;
      background: #e0e0e0;
      z-index: 0;
    }
    .step.done:not(:last-child)::after { background: #43a047; }
    .dot {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background: #f5f5f5;
      color: #9e9e9e;
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1;
      border: 2px solid #e0e0e0;
    }
    .step.done .dot { background: #c8e6c9; color: #1b5e20; border-color: #43a047; }
    .step.current .dot { background: #1a237e; color: #fff; border-color: #1a237e; }
    .label { font-size: 12px; margin-top: 4px; color: #607d8b; text-align: center; }
    .step.done .label, .step.current .label { color: #1a237e; font-weight: 500; }
    .ts { font-size: 11px; color: #9e9e9e; }

    .actions-card { margin-bottom: 16px; }
    .actions { display: flex; gap: 8px; flex-wrap: wrap; }

    .section { margin-bottom: 16px; }

    .items-table { width: 100%; }
    .num { text-align: right; }
    .muted { color: #607d8b; display: block; font-size: 12px; }

    .summary-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }
    .span-2 { grid-column: span 2; }

    dl {
      display: grid;
      grid-template-columns: 160px 1fr;
      gap: 8px 16px;
      margin: 0;
    }
    dt { color: #607d8b; font-size: 13px; }
    dd { margin: 0; font-size: 14px; }
    dd.neg { color: #c62828; }
    dt.grand, dd.grand { font-size: 16px; font-weight: 600; padding-top: 8px; border-top: 1px solid #e0e0e0; }

    .address, .notes { white-space: pre-wrap; line-height: 1.5; }

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

    /* Status chip colors (mirror list) */
    mat-chip.status-chip { font-size: 11px; height: 22px; padding: 0 8px; }
    mat-chip.status-draft        { background: #eceff1; color: #455a64; }
    mat-chip.status-confirmed    { background: #e3f2fd; color: #0d47a1; }
    mat-chip.status-inproduction { background: #fff3e0; color: #e65100; }
    mat-chip.status-qc           { background: #f3e5f5; color: #6a1b9a; }
    mat-chip.status-packed       { background: #e0f7fa; color: #006064; }
    mat-chip.status-shipped      { background: #e8eaf6; color: #1a237e; }
    mat-chip.status-delivered    { background: #c8e6c9; color: #1b5e20; }
    mat-chip.status-cancelled    { background: #ffcdd2; color: #b71c1c; }

    @media (max-width: 800px) {
      .summary-grid { grid-template-columns: 1fr; }
      .span-2 { grid-column: auto; }
    }
  `],
})
export class SalesOrderDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(SalesOrderService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly STATUS = SalesOrderStatus;
  readonly icons = STATUS_ICONS;
  readonly itemColumns = ['line', 'description', 'quantity', 'unitPrice', 'discount', 'lineTotal'];

  readonly order = signal<SalesOrder | null>(null);
  readonly loading = signal(false);
  readonly changing = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly allowedNextStatuses = computed(() => {
    const o = this.order();
    return o ? nextAllowedStatuses(o.status) : [];
  });

  readonly timeline = computed(() => {
    const o = this.order();
    if (!o) return [];
    const happyPath: { value: SalesOrderStatusValue; label: string; icon: string; ts?: string | null }[] = [
      { value: SalesOrderStatus.Draft, label: 'Draft', icon: STATUS_ICONS[SalesOrderStatus.Draft], ts: o.createdAt },
      { value: SalesOrderStatus.Confirmed, label: 'Confirmed', icon: STATUS_ICONS[SalesOrderStatus.Confirmed], ts: o.confirmedAt },
      { value: SalesOrderStatus.InProduction, label: 'In Production', icon: STATUS_ICONS[SalesOrderStatus.InProduction] },
      { value: SalesOrderStatus.Qc, label: 'QC', icon: STATUS_ICONS[SalesOrderStatus.Qc] },
      { value: SalesOrderStatus.Packed, label: 'Packed', icon: STATUS_ICONS[SalesOrderStatus.Packed] },
      { value: SalesOrderStatus.Shipped, label: 'Shipped', icon: STATUS_ICONS[SalesOrderStatus.Shipped], ts: o.shippedAt },
      { value: SalesOrderStatus.Delivered, label: 'Delivered', icon: STATUS_ICONS[SalesOrderStatus.Delivered], ts: o.deliveredAt },
    ];

    const isCancelled = o.status === 'Cancelled';
    const currentIndex = happyPath.findIndex((s) => STATUS_LABELS[s.value] === STATUS_LABELS[statusValueOf(o.status)]);

    return happyPath.map((step, idx) => ({
      label: step.label,
      icon: step.icon,
      timestamp: step.ts,
      done: !isCancelled && idx < currentIndex,
      current: !isCancelled && idx === currentIndex,
    })).concat(
      isCancelled
        ? [{
            label: 'Cancelled',
            icon: STATUS_ICONS[SalesOrderStatus.Cancelled],
            timestamp: o.cancelledAt,
            done: true,
            current: true,
          }]
        : []
    );
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.load(id);
  }

  statusLabel(name: string): string {
    return STATUS_LABELS[statusValueOf(name as never)] ?? name;
  }
  statusLabelFromValue(value: SalesOrderStatusValue): string {
    return STATUS_LABELS[value];
  }

  onTransition(next: SalesOrderStatusValue): void {
    const o = this.order();
    if (!o) return;

    let trackingNumber: string | null = null;
    if (next === SalesOrderStatus.Shipped) {
      trackingNumber = window.prompt('Enter tracking number (optional):') || null;
    }

    this.applyStatusChange(o, next, null, trackingNumber);
  }

  onCancel(): void {
    const o = this.order();
    if (!o) return;

    const ref = this.dialog.open<CancelOrderDialog, CancelOrderDialogData, string | undefined>(
      CancelOrderDialog,
      { data: { orderNumber: o.orderNumber }, width: '480px' },
    );

    ref.afterClosed().subscribe((reason) => {
      if (reason) {
        this.applyStatusChange(o, SalesOrderStatus.Cancelled, reason, null);
      }
    });
  }

  private applyStatusChange(
    o: SalesOrder,
    next: SalesOrderStatusValue,
    reason: string | null,
    trackingNumber: string | null,
  ): void {
    this.changing.set(true);
    this.errorMessage.set(null);

    this.service.changeStatus(o.id, {
      newStatus: next,
      reason,
      trackingNumber,
      rowVersion: o.rowVersion,
    }).subscribe({
      next: (updated) => {
        this.order.set(updated);
        this.changing.set(false);
        this.snack.open(`Order moved to ${STATUS_LABELS[next]}`, 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.changing.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to change status.'));
      },
    });
  }

  private load(id: string): void {
    this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (o) => {
        this.order.set(o);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to load order.'));
      },
    });
  }
}

function statusValueOf(name: string): SalesOrderStatusValue {
  return (SalesOrderStatus as Record<string, number>)[name] as SalesOrderStatusValue;
}
