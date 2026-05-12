import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../../core/api/error-utils';
import { ShippingService } from '../shipping.service';
import {
  SHIPMENT_STATUS_LABELS,
  ShipmentResponse,
  ShipmentStatus,
  ShipmentStatusValue,
  UpdateShipmentStatusDto,
} from '../shipping.types';

const SHIPMENT_TRANSITIONS: Record<ShipmentStatusValue, ShipmentStatusValue[]> = {
  1: [2],
  2: [3, 5],
  3: [4, 5],
  4: [],
  5: [],
};

@Component({
  selector: 'app-shipment-detail',
  imports: [
    FormsModule, RouterLink, DatePipe, DecimalPipe,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatDividerModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatSnackBarModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h1>{{ shipment()?.shipmentNumber ?? 'Shipment Detail' }}</h1>
        <p class="subtitle">Shipment record &amp; item manifest</p>
      </div>
      <div class="header-actions">
        <button mat-button routerLink="/shipments">Back to list</button>
      </div>
    </div>

    @if (loading()) {
      <div class="loading">Loading…</div>
    }
    @if (errorMessage(); as msg) {
      <div class="error"><mat-icon>error_outline</mat-icon> {{ msg }}</div>
    }

    @if (shipment(); as s) {
      <div class="cards-grid">

        <!-- Info card -->
        <mat-card>
          <mat-card-header><mat-card-title>Shipment Info</mat-card-title></mat-card-header>
          <mat-card-content>
            <div class="info-grid">
              <span class="label">Shipment #</span><strong>{{ s.shipmentNumber }}</strong>
              <span class="label">Status</span>
              <mat-chip [class]="'status-' + s.status">{{ statusLabel(s.status) }}</mat-chip>
              <span class="label">Sales Order</span><span>{{ s.salesOrderNumber ?? '—' }}</span>
              <span class="label">Ship Date</span><span>{{ s.shipDate ? (s.shipDate | date:'mediumDate') : '—' }}</span>
              <span class="label">Est. Delivery</span><span>{{ s.estimatedDelivery ? (s.estimatedDelivery | date:'mediumDate') : '—' }}</span>
              <span class="label">Actual Delivery</span><span>{{ s.actualDelivery ? (s.actualDelivery | date:'mediumDate') : '—' }}</span>
              <span class="label">Carrier</span><span>{{ s.carrier ?? '—' }}</span>
              <span class="label">Tracking #</span><span>{{ s.trackingNumber ?? '—' }}</span>
              <span class="label">Method</span><span>{{ s.shippingMethod ?? '—' }}</span>
              <span class="label">Total Weight</span><span>{{ s.totalWeightGrams | number:'1.2-4' }} g</span>
              <span class="label">Packing Notes</span><span>{{ s.packingNotes ?? '—' }}</span>
              <span class="label">Created</span><span>{{ s.createdAt | date:'medium' }}</span>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Status transition card -->
        @if (nextStatuses().length > 0) {
          <mat-card>
            <mat-card-header><mat-card-title>Update Status</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="transition-form">
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>New Status</mat-label>
                  <mat-select [(ngModel)]="newStatus">
                    @for (ns of nextStatuses(); track ns) {
                      <mat-option [value]="ns">{{ statusLabel(ns) }}</mat-option>
                    }
                  </mat-select>
                </mat-form-field>

                @if (newStatus === 4) {
                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>Actual Delivery Date</mat-label>
                    <input matInput type="date" [(ngModel)]="actualDelivery" />
                  </mat-form-field>
                }

                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Tracking Number (optional)</mat-label>
                  <input matInput [(ngModel)]="trackingNumber" />
                </mat-form-field>

                <button mat-flat-button color="primary"
                        [disabled]="!newStatus || saving()"
                        (click)="updateStatus()">
                  <mat-icon>check</mat-icon>
                  {{ saving() ? 'Saving…' : 'Confirm Status' }}
                </button>
              </div>
            </mat-card-content>
          </mat-card>
        }
      </div>

      <!-- Items table -->
      <mat-card class="items-card">
        <mat-card-header><mat-card-title>Items ({{ s.items.length }})</mat-card-title></mat-card-header>
        <mat-card-content>
          <table mat-table [dataSource]="s.items" class="items-table">

            <ng-container matColumnDef="line">
              <th mat-header-cell *matHeaderCellDef class="num">#</th>
              <td mat-cell *matCellDef="let item; let i = index" class="num">{{ i + 1 }}</td>
            </ng-container>

            <ng-container matColumnDef="description">
              <th mat-header-cell *matHeaderCellDef>Description</th>
              <td mat-cell *matCellDef="let item">
                {{ item.description }}
                @if (item.designCode) { <small class="muted">({{ item.designCode }})</small> }
              </td>
            </ng-container>

            <ng-container matColumnDef="workOrder">
              <th mat-header-cell *matHeaderCellDef>Work Order</th>
              <td mat-cell *matCellDef="let item">{{ item.workOrderNumber ?? '—' }}</td>
            </ng-container>

            <ng-container matColumnDef="qty">
              <th mat-header-cell *matHeaderCellDef class="num">Qty</th>
              <td mat-cell *matCellDef="let item" class="num">{{ item.quantity }}</td>
            </ng-container>

            <ng-container matColumnDef="weight">
              <th mat-header-cell *matHeaderCellDef class="num">Weight (g)</th>
              <td mat-cell *matCellDef="let item" class="num">{{ item.weightGrams | number:'1.2-4' }}</td>
            </ng-container>

            <ng-container matColumnDef="value">
              <th mat-header-cell *matHeaderCellDef class="num">Unit Value (USD)</th>
              <td mat-cell *matCellDef="let item" class="num">
                {{ item.unitValueUsd != null ? (item.unitValueUsd | number:'1.2-2') : '—' }}
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="itemColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: itemColumns;"></tr>
          </table>
        </mat-card-content>
      </mat-card>
    }
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 16px; }
    .page-header h1 { margin: 0 0 4px; font-weight: 500; }
    .subtitle { margin: 0; color: #607d8b; font-size: 13px; }
    .header-actions { display: flex; gap: 8px; }
    .loading { padding: 24px; text-align: center; color: #607d8b; }
    .error { display: flex; gap: 8px; align-items: center; padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin-bottom: 12px; }
    .cards-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); gap: 16px; margin-bottom: 16px; }
    mat-card-content { padding-top: 8px !important; }
    .info-grid { display: grid; grid-template-columns: 140px 1fr; gap: 8px 12px; align-items: center; }
    .label { color: #607d8b; font-size: 13px; }
    .muted { color: #607d8b; }
    .transition-form { display: flex; flex-direction: column; gap: 12px; }
    .full-width { width: 100%; }
    .items-card { margin-top: 0; }
    .items-table { width: 100%; }
    .num { text-align: right; }
    mat-chip.status-1 { background: #e3f2fd; color: #0d47a1; }
    mat-chip.status-2 { background: #fff3e0; color: #e65100; }
    mat-chip.status-3 { background: #fff9c4; color: #f57f17; }
    mat-chip.status-4 { background: #c8e6c9; color: #1b5e20; }
    mat-chip.status-5 { background: #ffcdd2; color: #b71c1c; }
  `],
})
export class ShipmentDetail implements OnInit {
  private readonly svc = inject(ShippingService);
  private readonly route = inject(ActivatedRoute);
  private readonly snack = inject(MatSnackBar);

  readonly shipment = signal<ShipmentResponse | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly nextStatuses = signal<ShipmentStatusValue[]>([]);

  readonly itemColumns = ['line', 'description', 'workOrder', 'qty', 'weight', 'value'];

  newStatus: ShipmentStatusValue | null = null;
  actualDelivery = '';
  trackingNumber = '';

  statusLabel(v: ShipmentStatusValue) { return SHIPMENT_STATUS_LABELS[v]; }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loading.set(true);
    this.svc.getShipmentById(id).subscribe({
      next: (s) => {
        this.shipment.set(s);
        this.trackingNumber = s.trackingNumber ?? '';
        this.nextStatuses.set(SHIPMENT_TRANSITIONS[s.status] ?? []);
        this.loading.set(false);
      },
      error: (err) => { this.loading.set(false); this.errorMessage.set(extractErrorMessage(err, 'Failed to load shipment.')); },
    });
  }

  updateStatus(): void {
    if (!this.newStatus || !this.shipment()) return;
    const dto: UpdateShipmentStatusDto = {
      newStatus: this.newStatus,
      actualDelivery: this.actualDelivery || null,
      trackingNumber: this.trackingNumber || null,
    };
    this.saving.set(true);
    this.svc.updateShipmentStatus(this.shipment()!.id, dto).subscribe({
      next: () => {
        this.saving.set(false);
        this.snack.open('Status updated.', 'OK', { duration: 3000 });
        const id = this.route.snapshot.paramMap.get('id')!;
        this.svc.getShipmentById(id).subscribe((s) => {
          this.shipment.set(s);
          this.nextStatuses.set(SHIPMENT_TRANSITIONS[s.status] ?? []);
          this.newStatus = null;
        });
      },
      error: (err) => { this.saving.set(false); this.snack.open(extractErrorMessage(err, 'Update failed.'), 'OK', { duration: 5000 }); },
    });
  }
}
