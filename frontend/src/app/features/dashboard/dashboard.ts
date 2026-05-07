import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { extractErrorMessage } from '../../core/api/error-utils';
import { AuthService } from '../../core/auth/auth.service';
import { ReportsService } from '../reports/reports.service';
import {
  InventorySummaryReport,
  ProductionLoadReport,
  SalesOrderPipelineReport,
  StagesByStage,
} from '../reports/reports.types';

interface Kpi {
  icon: string;
  label: string;
  value: string;
  hint: string;
  color: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [
    DecimalPipe,
    DatePipe,
    RouterLink,
    MatCardModule,
    MatIconModule,
    MatChipsModule,
    MatTableModule,
    MatProgressBarModule,
    MatButtonModule,
    MatDividerModule,
    MatTooltipModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h1>Dashboard</h1>
        <p class="subtitle">
          Welcome back, <strong>{{ auth.user()?.fullName }}</strong>
          <span class="muted"> · live snapshot from inventory + sales + production</span>
        </p>
      </div>
      <button mat-icon-button (click)="reload()" matTooltip="Refresh">
        <mat-icon>refresh</mat-icon>
      </button>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate" /> }

    @if (errorMessage(); as msg) {
      <div class="error">
        <mat-icon>error_outline</mat-icon>
        <span>{{ msg }}</span>
      </div>
    }

    <!-- ── KPI cards ───────────────────────────────────────────────── -->
    <section class="kpis">
      @for (kpi of kpis(); track kpi.label) {
        <mat-card class="kpi-card" [style.borderLeftColor]="kpi.color">
          <mat-card-content>
            <div class="kpi-row">
              <mat-icon [style.color]="kpi.color">{{ kpi.icon }}</mat-icon>
              <div class="kpi-text">
                <div class="kpi-label">{{ kpi.label }}</div>
                <div class="kpi-value">{{ kpi.value }}</div>
                <div class="kpi-hint">{{ kpi.hint }}</div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      }
    </section>

    <!-- ── Inventory ───────────────────────────────────────────────── -->
    @if (inventory(); as inv) {
      <section class="section">
        <div class="section-header">
          <h2><mat-icon>inventory_2</mat-icon> Inventory</h2>
          <a mat-button routerLink="/inventory">View all →</a>
        </div>
        <div class="grid">
          <mat-card>
            <mat-card-header>
              <mat-card-title>Raw Materials by Material</mat-card-title>
              <mat-card-subtitle>{{ inv.rawMaterialsByMaterial.length }} active material(s)</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @if (inv.rawMaterialsByMaterial.length === 0) {
                <div class="empty">No raw materials in stock yet.</div>
              }
              @for (m of inv.rawMaterialsByMaterial; track m.materialCode) {
                <div class="row">
                  <div class="row-main">
                    <strong>{{ m.materialName }}</strong>
                    <small class="muted">{{ m.materialCode }} · {{ m.lotCount }} lot(s)</small>
                  </div>
                  <div class="row-num">
                    <strong>{{ m.totalQuantity | number: '1.0-4' }}</strong>
                    <small class="muted">{{ m.unit }}</small>
                    @if (m.totalPureWeight !== null) {
                      <div class="pure">pure {{ m.totalPureWeight | number: '1.0-4' }}g</div>
                    }
                  </div>
                </div>
              }
            </mat-card-content>
          </mat-card>

          <mat-card>
            <mat-card-header>
              <mat-card-title>Stock Status Breakdown</mat-card-title>
              <mat-card-subtitle>Counts by InStock / Reserved / etc.</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <div class="status-grid">
                <div class="status-col">
                  <div class="col-label">Raw Material</div>
                  @for (s of inv.rawMaterialsByStatus; track s.status) {
                    <div class="status-line">
                      <mat-chip [class]="'status-' + s.status.toLowerCase()">{{ s.status }}</mat-chip>
                      <span class="status-count">{{ s.count }}</span>
                    </div>
                  }
                  @if (inv.rawMaterialsByStatus.length === 0) { <div class="empty">—</div> }
                </div>
                <div class="status-col">
                  <div class="col-label">Stone Items</div>
                  @for (s of inv.stoneItemsByStatus; track s.status) {
                    <div class="status-line">
                      <mat-chip [class]="'status-' + s.status.toLowerCase()">{{ s.status }}</mat-chip>
                      <span class="status-count">{{ s.count }}</span>
                    </div>
                  }
                  @if (inv.stoneItemsByStatus.length === 0) { <div class="empty">—</div> }
                </div>
                <div class="status-col">
                  <div class="col-label">Stone Parcels</div>
                  @for (s of inv.stoneParcelsByStatus; track s.status) {
                    <div class="status-line">
                      <mat-chip [class]="'status-' + s.status.toLowerCase()">{{ s.status }}</mat-chip>
                      <span class="status-count">{{ s.count }}</span>
                    </div>
                  }
                  @if (inv.stoneParcelsByStatus.length === 0) { <div class="empty">—</div> }
                </div>
              </div>
            </mat-card-content>
          </mat-card>
        </div>
      </section>
    }

    <!-- ── Sales Orders ────────────────────────────────────────────── -->
    @if (sales(); as so) {
      <section class="section">
        <div class="section-header">
          <h2><mat-icon>receipt_long</mat-icon> Sales Pipeline</h2>
          <a mat-button routerLink="/sales-orders">View all →</a>
        </div>
        <div class="grid">
          <mat-card>
            <mat-card-header>
              <mat-card-title>Orders by Status</mat-card-title>
              <mat-card-subtitle>{{ so.totalOrders }} total · {{ so.openOrders }} open</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @if (so.byStatus.length === 0) { <div class="empty">No orders yet.</div> }
              @for (s of so.byStatus; track s.status) {
                <div class="row">
                  <div class="row-main">
                    <mat-chip [class]="'order-status order-status-' + s.status.toLowerCase()">{{ s.status }}</mat-chip>
                    <small class="muted">{{ s.count }} order(s)</small>
                  </div>
                  <div class="row-num">
                    <strong>{{ s.totalAmount | number: '1.2-2' }}</strong>
                    <small class="muted">{{ s.currency || 'mixed' }}</small>
                  </div>
                </div>
                <!-- bar -->
                <div class="bar-track">
                  <div class="bar-fill"
                       [style.width.%]="percent(s.count, so.totalOrders)"
                       [class]="'order-status-' + s.status.toLowerCase()"></div>
                </div>
              }
            </mat-card-content>
          </mat-card>

          <mat-card>
            <mat-card-header>
              <mat-card-title>Revenue by Currency</mat-card-title>
              <mat-card-subtitle>Cross-currency totals</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @if (so.revenueByCurrency.length === 0) { <div class="empty">No revenue yet.</div> }
              @for (c of so.revenueByCurrency; track c.currency) {
                <div class="row big">
                  <div class="row-main">
                    <strong class="currency">{{ c.currency }}</strong>
                    <small class="muted">{{ c.orderCount }} order(s)</small>
                  </div>
                  <div class="row-num">
                    <strong class="big-num">{{ c.totalRevenue | number: '1.2-2' }}</strong>
                  </div>
                </div>
              }
            </mat-card-content>
          </mat-card>

          @if (so.topCustomers.length > 0) {
            <mat-card class="span-2">
              <mat-card-header>
                <mat-card-title>Top Customers</mat-card-title>
                <mat-card-subtitle>By order count</mat-card-subtitle>
              </mat-card-header>
              <mat-card-content>
                <table mat-table [dataSource]="so.topCustomers" class="mini-table">
                  <ng-container matColumnDef="code">
                    <th mat-header-cell *matHeaderCellDef>Code</th>
                    <td mat-cell *matCellDef="let c">{{ c.customerCode }}</td>
                  </ng-container>
                  <ng-container matColumnDef="name">
                    <th mat-header-cell *matHeaderCellDef>Customer</th>
                    <td mat-cell *matCellDef="let c">{{ c.customerName }}</td>
                  </ng-container>
                  <ng-container matColumnDef="orders">
                    <th mat-header-cell *matHeaderCellDef class="num">Orders</th>
                    <td mat-cell *matCellDef="let c" class="num">{{ c.orderCount }}</td>
                  </ng-container>
                  <ng-container matColumnDef="revenue">
                    <th mat-header-cell *matHeaderCellDef class="num">Revenue</th>
                    <td mat-cell *matCellDef="let c" class="num">
                      <strong>{{ c.currency }} {{ c.totalRevenue | number: '1.2-2' }}</strong>
                    </td>
                  </ng-container>
                  <tr mat-header-row *matHeaderRowDef="['code','name','orders','revenue']"></tr>
                  <tr mat-row *matRowDef="let row; columns: ['code','name','orders','revenue'];"></tr>
                </table>
              </mat-card-content>
            </mat-card>
          }

          @if (so.overdue.length > 0) {
            <mat-card class="span-2 alert-card">
              <mat-card-header>
                <mat-card-title>
                  <mat-icon class="alert-icon">warning</mat-icon>
                  Overdue Orders ({{ so.overdue.length }})
                </mat-card-title>
                <mat-card-subtitle>Past requested delivery date and not Delivered/Cancelled</mat-card-subtitle>
              </mat-card-header>
              <mat-card-content>
                <table mat-table [dataSource]="so.overdue" class="mini-table">
                  <ng-container matColumnDef="order">
                    <th mat-header-cell *matHeaderCellDef>Order</th>
                    <td mat-cell *matCellDef="let o">{{ o.orderNumber }}</td>
                  </ng-container>
                  <ng-container matColumnDef="customer">
                    <th mat-header-cell *matHeaderCellDef>Customer</th>
                    <td mat-cell *matCellDef="let o">{{ o.customerName }}</td>
                  </ng-container>
                  <ng-container matColumnDef="status">
                    <th mat-header-cell *matHeaderCellDef>Status</th>
                    <td mat-cell *matCellDef="let o">
                      <mat-chip [class]="'order-status order-status-' + o.status.toLowerCase()">{{ o.status }}</mat-chip>
                    </td>
                  </ng-container>
                  <ng-container matColumnDef="due">
                    <th mat-header-cell *matHeaderCellDef>Was Due</th>
                    <td mat-cell *matCellDef="let o">{{ o.requestedDeliveryDate | date: 'mediumDate' }}</td>
                  </ng-container>
                  <ng-container matColumnDef="late">
                    <th mat-header-cell *matHeaderCellDef class="num">Days Late</th>
                    <td mat-cell *matCellDef="let o" class="num overdue-days">{{ o.daysOverdue }}</td>
                  </ng-container>
                  <tr mat-header-row *matHeaderRowDef="['order','customer','status','due','late']"></tr>
                  <tr mat-row *matRowDef="let row; columns: ['order','customer','status','due','late'];"></tr>
                </table>
              </mat-card-content>
            </mat-card>
          }
        </div>
      </section>
    }

    <!-- ── Production ──────────────────────────────────────────────── -->
    @if (production(); as prod) {
      <section class="section">
        <div class="section-header">
          <h2><mat-icon>precision_manufacturing</mat-icon> Production Floor</h2>
          <a mat-button disabled>(WO UI coming next)</a>
        </div>
        <div class="grid">
          <mat-card>
            <mat-card-header>
              <mat-card-title>Stages on the Floor</mat-card-title>
              <mat-card-subtitle>Active WOs · 9-stage breakdown</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @if (prod.stagesInProgress.length === 0) { <div class="empty">No active work orders.</div> }
              @for (s of prod.stagesInProgress; track s.stage) {
                <div class="stage-row">
                  <div class="stage-name">{{ stagePretty(s.stage) }}</div>
                  <div class="stage-bars">
                    @if (s.completedCount > 0) {
                      <div class="seg done" [style.flex]="s.completedCount">
                        <span>✓ {{ s.completedCount }}</span>
                      </div>
                    }
                    @if (s.inProgressCount > 0) {
                      <div class="seg active" [style.flex]="s.inProgressCount">
                        <span>▶ {{ s.inProgressCount }}</span>
                      </div>
                    }
                    @if (s.pendingCount > 0) {
                      <div class="seg pending" [style.flex]="s.pendingCount">
                        <span>· {{ s.pendingCount }}</span>
                      </div>
                    }
                    @if (totalForStage(s) === 0) {
                      <div class="seg empty-seg" style="flex:1">—</div>
                    }
                  </div>
                  <div class="stage-loss" [matTooltip]="'Lifetime loss across all WOs at this stage'">
                    @if (s.totalLossGrams !== null) {
                      <small>−{{ s.totalLossGrams | number: '1.0-4' }}g</small>
                    } @else { <small class="muted">—</small> }
                  </div>
                </div>
              }
            </mat-card-content>
          </mat-card>

          <mat-card>
            <mat-card-header>
              <mat-card-title>Worker Workload</mat-card-title>
              <mat-card-subtitle>Open stages assigned per worker</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @if (prod.workerLoad.length === 0) {
                <div class="empty">No worker assignments yet — assign in Work Order stages.</div>
              }
              @for (w of prod.workerLoad; track w.workerId) {
                <div class="row">
                  <div class="row-main">
                    <strong>{{ w.fullName }}</strong>
                    <small class="muted">{{ w.employeeCode }} · {{ w.position }}</small>
                  </div>
                  <div class="row-num">
                    <span class="badge active">▶ {{ w.inProgressStages }}</span>
                    <span class="badge pending">· {{ w.pendingStages }}</span>
                  </div>
                </div>
              }
            </mat-card-content>
          </mat-card>

          <mat-card class="span-2 production-summary">
            <mat-card-content>
              <div class="metrics-row">
                <div class="metric">
                  <div class="metric-label">This Month — Completed</div>
                  <div class="metric-value">{{ prod.completedThisMonth }}</div>
                </div>
                <mat-divider [vertical]="true" />
                <div class="metric">
                  <div class="metric-label">This Month — Cancelled</div>
                  <div class="metric-value">{{ prod.cancelledThisMonth }}</div>
                </div>
                <mat-divider [vertical]="true" />
                <div class="metric">
                  <div class="metric-label">This Month — Total Loss</div>
                  <div class="metric-value loss">{{ prod.totalLossGramsThisMonth | number: '1.0-4' }} g</div>
                </div>
              </div>
            </mat-card-content>
          </mat-card>
        </div>
      </section>
    }
  `,
  styles: [`
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 16px;
    }
    .page-header h1 { margin: 0 0 4px; font-weight: 500; }
    .subtitle { margin: 0; color: #455a64; font-size: 13px; }
    .muted { color: #607d8b; }
    .empty { color: #9e9e9e; font-size: 13px; padding: 8px 0; text-align: center; }
    .error {
      display: flex; gap: 8px; align-items: center;
      padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin: 12px 0;
    }

    /* KPIs */
    .kpis {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 16px;
      margin: 16px 0 24px;
    }
    .kpi-card { border-left: 4px solid #ccc; }
    .kpi-row { display: flex; gap: 12px; align-items: center; }
    .kpi-row mat-icon { font-size: 36px; height: 36px; width: 36px; }
    .kpi-text { flex: 1; }
    .kpi-label { font-size: 12px; color: #607d8b; text-transform: uppercase; letter-spacing: 0.5px; }
    .kpi-value { font-size: 22px; font-weight: 500; line-height: 1.2; }
    .kpi-hint { font-size: 11px; color: #9e9e9e; }

    /* Section */
    .section { margin-bottom: 32px; }
    .section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 12px;
    }
    .section-header h2 {
      display: flex; align-items: center; gap: 8px;
      margin: 0; font-size: 18px; font-weight: 500; color: #1a237e;
    }
    .grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }
    .span-2 { grid-column: span 2; }

    /* Row inside cards */
    .row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 10px 0;
      gap: 12px;
    }
    .row.big { padding: 14px 0; }
    .row + .row { border-top: 1px solid #eceff1; }
    .row-main { display: flex; flex-direction: column; gap: 2px; }
    .row-main strong { font-size: 14px; }
    .row-main small { font-size: 12px; }
    .row-num { text-align: right; }
    .row-num strong { font-size: 14px; }
    .row-num .pure { font-size: 11px; color: #43a047; margin-top: 2px; }
    .big-num { font-size: 22px; font-weight: 500; }
    .currency { font-size: 18px; color: #1a237e; }

    /* Bars */
    .bar-track { height: 4px; background: #f5f5f5; border-radius: 2px; overflow: hidden; margin-bottom: 8px; }
    .bar-fill { height: 100%; transition: width .3s; }
    .bar-fill.order-status-draft        { background: #607d8b; }
    .bar-fill.order-status-confirmed    { background: #1976d2; }
    .bar-fill.order-status-inproduction { background: #f57c00; }
    .bar-fill.order-status-qc           { background: #6a1b9a; }
    .bar-fill.order-status-packed       { background: #00838f; }
    .bar-fill.order-status-shipped      { background: #283593; }
    .bar-fill.order-status-delivered    { background: #43a047; }
    .bar-fill.order-status-cancelled    { background: #c62828; }

    /* Status grid */
    .status-grid {
      display: grid;
      grid-template-columns: 1fr 1fr 1fr;
      gap: 12px;
    }
    .col-label {
      font-size: 12px; color: #607d8b; text-transform: uppercase;
      letter-spacing: 0.5px; margin-bottom: 8px; font-weight: 500;
    }
    .status-line { display: flex; justify-content: space-between; align-items: center; padding: 4px 0; }
    .status-count { font-weight: 600; }

    /* Stages */
    .stage-row {
      display: grid;
      grid-template-columns: 130px 1fr 80px;
      gap: 12px;
      align-items: center;
      padding: 8px 0;
    }
    .stage-name { font-size: 13px; font-weight: 500; }
    .stage-bars { display: flex; gap: 2px; height: 22px; border-radius: 3px; overflow: hidden; background: #f5f5f5; }
    .seg {
      display: flex; align-items: center; justify-content: center;
      font-size: 11px; color: #fff; font-weight: 500; min-width: 24px;
    }
    .seg.done { background: #43a047; }
    .seg.active { background: #1976d2; }
    .seg.pending { background: #b0bec5; color: #455a64; }
    .seg.empty-seg { background: #f5f5f5; color: #9e9e9e; font-weight: normal; }
    .stage-loss { text-align: right; color: #c62828; font-size: 11px; }
    .stage-loss small.muted { color: #9e9e9e; }

    /* Workers */
    .badge {
      display: inline-block; padding: 2px 8px; border-radius: 10px;
      font-size: 11px; font-weight: 600; margin-left: 4px;
    }
    .badge.active { background: #e3f2fd; color: #0d47a1; }
    .badge.pending { background: #eceff1; color: #455a64; }

    /* Production summary */
    .production-summary { margin-top: 8px; }
    .metrics-row { display: flex; justify-content: space-around; align-items: center; padding: 8px 0; }
    .metric { text-align: center; }
    .metric-label { font-size: 12px; color: #607d8b; text-transform: uppercase; }
    .metric-value { font-size: 28px; font-weight: 500; color: #1a237e; }
    .metric-value.loss { color: #c62828; }

    /* Tables */
    .mini-table { width: 100%; }
    .num { text-align: right; }
    .overdue-days { color: #c62828; font-weight: 600; }

    /* Order status chips */
    mat-chip.order-status { font-size: 11px; height: 22px; }
    mat-chip.order-status-draft        { background: #eceff1; color: #455a64; }
    mat-chip.order-status-confirmed    { background: #e3f2fd; color: #0d47a1; }
    mat-chip.order-status-inproduction { background: #fff3e0; color: #e65100; }
    mat-chip.order-status-qc           { background: #f3e5f5; color: #6a1b9a; }
    mat-chip.order-status-packed       { background: #e0f7fa; color: #006064; }
    mat-chip.order-status-shipped      { background: #e8eaf6; color: #1a237e; }
    mat-chip.order-status-delivered    { background: #c8e6c9; color: #1b5e20; }
    mat-chip.order-status-cancelled    { background: #ffcdd2; color: #b71c1c; }

    /* Stock status chips */
    mat-chip.status-instock { background: #c8e6c9; color: #1b5e20; }
    mat-chip.status-reserved { background: #fff3e0; color: #e65100; }
    mat-chip.status-inproduction { background: #e3f2fd; color: #0d47a1; }
    mat-chip.status-sold { background: #eceff1; color: #455a64; }
    mat-chip.status-writtenoff { background: #ffcdd2; color: #b71c1c; }

    /* Alert card */
    .alert-card { border-left: 4px solid #c62828; }
    .alert-icon { color: #c62828; vertical-align: middle; }

    @media (max-width: 900px) {
      .grid { grid-template-columns: 1fr; }
      .span-2 { grid-column: auto; }
      .status-grid { grid-template-columns: 1fr; }
      .stage-row { grid-template-columns: 100px 1fr 60px; }
    }
  `],
})
export class Dashboard implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly reports = inject(ReportsService);

  readonly inventory = signal<InventorySummaryReport | null>(null);
  readonly sales = signal<SalesOrderPipelineReport | null>(null);
  readonly production = signal<ProductionLoadReport | null>(null);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly kpis = computed<Kpi[]>(() => {
    const inv = this.inventory();
    const so = this.sales();
    const prod = this.production();
    if (!inv || !so || !prod) return [];

    return [
      {
        icon: 'monetization_on',
        label: 'Pure Gold In Stock',
        value: `${inv.totals.totalPureGoldGrams.toLocaleString(undefined, { maximumFractionDigits: 4 })} g`,
        hint: `${inv.totals.rawMaterialLotCount} raw lot(s) total`,
        color: '#f9a825',
      },
      {
        icon: 'diamond',
        label: 'Stones In Stock',
        value: `${inv.totals.totalStoneCarat.toLocaleString(undefined, { maximumFractionDigits: 2 })} ct`,
        hint: `${inv.totals.totalStoneCount} stone(s) · ${inv.totals.stoneItemCount} individual + ${inv.totals.stoneParcelCount} parcel(s)`,
        color: '#1e88e5',
      },
      {
        icon: 'receipt_long',
        label: 'Open Sales Orders',
        value: `${so.openOrders}`,
        hint: `${so.totalOrders} total · ${so.overdue.length} overdue`,
        color: so.overdue.length > 0 ? '#c62828' : '#43a047',
      },
      {
        icon: 'precision_manufacturing',
        label: 'Active Work Orders',
        value: `${prod.activeWorkOrders}`,
        hint: `${prod.completedThisMonth} completed this month · ${prod.totalLossGramsThisMonth.toFixed(4)}g loss`,
        color: '#6a1b9a',
      },
    ];
  });

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    forkJoin({
      inv: this.reports.inventorySummary(),
      so: this.reports.salesOrderPipeline(),
      prod: this.reports.productionLoad(),
    }).subscribe({
      next: ({ inv, so, prod }) => {
        this.inventory.set(inv);
        this.sales.set(so);
        this.production.set(prod);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to load reports.'));
      },
    });
  }

  percent(part: number, total: number): number {
    return total > 0 ? Math.round((part / total) * 100) : 0;
  }

  totalForStage(s: StagesByStage): number {
    return s.completedCount + s.inProgressCount + s.pendingCount;
  }

  /** WaxModel → "Wax Model", FilingCleaning → "Filing/Cleaning", FinalQc → "Final QC" */
  stagePretty(stage: string): string {
    return ({
      WaxModel: 'Wax Model',
      Casting: 'Casting',
      FilingCleaning: 'Filing/Cleaning',
      StoneSetting: 'Stone Setting',
      Polishing: 'Polishing',
      Plating: 'Plating',
      Assembly: 'Assembly',
      FinalQc: 'Final QC',
      Packaging: 'Packaging',
    } as Record<string, string>)[stage] ?? stage;
  }
}
