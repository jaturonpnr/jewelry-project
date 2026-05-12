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
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../../core/api/error-utils';
import { CancelWoDialog, CancelWoDialogData } from '../cancel-wo-dialog/cancel-wo-dialog';
import { CompleteStageDialog, CompleteStageDialogData, CompleteStageDialogResult } from '../stage-dialogs/complete-stage-dialog';
import { FailStageDialog, FailStageDialogData, FailStageDialogResult } from '../stage-dialogs/fail-stage-dialog';
import { SkipStageDialog, SkipStageDialogData, SkipStageDialogResult } from '../stage-dialogs/skip-stage-dialog';
import { StartStageDialog, StartStageDialogData, StartStageDialogResult } from '../stage-dialogs/start-stage-dialog';
import { WorkOrderService } from '../work-order.service';
import {
  nextAllowedStatuses, STAGE_ICONS, STAGE_PRETTY, STATUS_LABELS,
  WorkOrder, WorkOrderStage, WorkOrderStatus, WorkOrderStatusValue,
} from '../work-order.types';

@Component({
  selector: 'app-work-order-detail',
  imports: [
    RouterLink, DatePipe, DecimalPipe,
    MatCardModule, MatButtonModule, MatIconModule, MatChipsModule,
    MatDividerModule, MatProgressBarModule, MatTooltipModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <button mat-icon-button routerLink="/work-orders" aria-label="Back">
        <mat-icon>arrow_back</mat-icon>
      </button>
      <div class="header-info">
        @if (wo(); as w) {
          <h1>{{ w.workOrderNumber }}</h1>
          <p class="subtitle">
            {{ w.description }}
            <mat-chip [class]="'wo-status wo-status-' + w.status.toLowerCase()">{{ w.status }}</mat-chip>
            <mat-chip [class]="'priority-' + w.priority.toLowerCase()">{{ w.priority }}</mat-chip>
            @if (w.salesOrderNumber) {
              <small class="muted">· linked to {{ w.salesOrderNumber }}</small>
            }
          </p>
        } @else { <h1>Loading…</h1> }
      </div>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate" /> }
    @if (errorMessage(); as msg) {
      <div class="error">
        <mat-icon>error_outline</mat-icon>
        <span>{{ msg }}</span>
      </div>
    }

    @if (wo(); as w) {
      <!-- Header summary -->
      <div class="summary-grid">
        <mat-card>
          <mat-card-content>
            <div class="metric-row">
              <div class="metric">
                <div class="m-label">Quantity</div>
                <div class="m-value">{{ w.quantity }}</div>
              </div>
              <mat-divider [vertical]="true" />
              <div class="metric">
                <div class="m-label">Progress</div>
                <div class="m-value">{{ completedCount() }} / {{ activeStageCount() }}</div>
                <div class="progress-track">
                  <div class="progress-fill" [style.width.%]="progressPercent()"></div>
                </div>
              </div>
              <mat-divider [vertical]="true" />
              <div class="metric">
                <div class="m-label">Total Loss So Far</div>
                <div class="m-value loss">{{ totalLoss() | number: '1.0-4' }}<small>g</small></div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card>
          <mat-card-content>
            <dl>
              <dt>Design</dt><dd>{{ w.designCode || '—' }}</dd>
              <dt>Supervisor</dt><dd>{{ w.assignedSupervisorName || '—' }}</dd>
              <dt>Scheduled</dt>
              <dd>
                @if (w.scheduledStart) {
                  {{ w.scheduledStart | date: 'mediumDate' }}
                  → {{ w.scheduledEnd | date: 'mediumDate' }}
                } @else { —  }
              </dd>
              <dt>Released</dt><dd>{{ w.releasedAt ? (w.releasedAt | date: 'short') : '—' }}</dd>
              @if (w.completedAt) { <dt>Completed</dt><dd>{{ w.completedAt | date: 'short' }}</dd> }
              @if (w.cancellationReason) {
                <dt>Cancelled</dt>
                <dd class="neg">{{ w.cancelledAt | date: 'short' }} — {{ w.cancellationReason }}</dd>
              }
            </dl>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- WO status transition actions -->
      @if (allowedNextStatuses().length > 0) {
        <mat-card class="actions-card">
          <mat-card-header>
            <mat-card-title>Work Order Actions</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="actions">
              @for (next of allowedNextStatuses(); track next) {
                @if (next === STATUS.Cancelled) {
                  <button mat-stroked-button color="warn" [disabled]="changing()" (click)="onCancel()">
                    <mat-icon>cancel</mat-icon> Cancel work order
                  </button>
                } @else {
                  <button mat-flat-button color="primary" [disabled]="changing()" (click)="onTransition(next)">
                    <mat-icon>{{ statusIcon(next) }}</mat-icon>
                    Move to {{ statusLabel(next) }}
                  </button>
                }
              }
            </div>
          </mat-card-content>
        </mat-card>
      }

      <!-- Stages -->
      <mat-card class="stages-card">
        <mat-card-header>
          <mat-card-title>Stages ({{ w.stages.length }})</mat-card-title>
          <mat-card-subtitle>9 canonical jewelry production stages</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <div class="stages-list">
            @for (stage of w.stages; track stage.id) {
              <div class="stage-row" [class]="'stage-' + stage.status.toLowerCase()">
                <div class="stage-num">{{ stage.sequenceNumber }}</div>

                <div class="stage-icon-cell">
                  <div class="stage-icon-bg" [class]="'bg-' + stage.status.toLowerCase()">
                    <mat-icon>{{ statusIcon2(stage.status) || stageIcon(stage.stage) }}</mat-icon>
                  </div>
                </div>

                <div class="stage-main">
                  <div class="stage-name">{{ stagePretty(stage.stage) }}</div>
                  <div class="stage-meta">
                    <mat-chip [class]="'stage-status-' + stage.status.toLowerCase()">{{ stage.status }}</mat-chip>
                    @if (stage.assignedWorkerName) {
                      <small class="muted"><mat-icon class="i-small">person</mat-icon> {{ stage.assignedWorkerName }}</small>
                    }
                  </div>
                  @if (stage.notes) { <small class="notes">{{ stage.notes }}</small> }
                  @if (stage.failureReason) { <small class="notes neg">⚠ {{ stage.failureReason }}</small> }
                </div>

                <div class="stage-time">
                  @if (stage.startedAt) {
                    <small class="muted">Started: {{ stage.startedAt | date: 'short' }}</small>
                  }
                  @if (stage.completedAt) {
                    <small class="muted">Completed: {{ stage.completedAt | date: 'short' }}</small>
                  }
                  <small class="muted">
                    Est: {{ stage.estimatedHours }}h
                    @if (stage.actualHours !== null) { · Actual: {{ stage.actualHours }}h }
                  </small>
                </div>

                <div class="stage-weight">
                  @if (stage.weightInGrams !== null) {
                    <small><strong>In:</strong> {{ stage.weightInGrams | number: '1.0-4' }}g</small>
                  }
                  @if (stage.weightOutGrams !== null) {
                    <small><strong>Out:</strong> {{ stage.weightOutGrams | number: '1.0-4' }}g</small>
                  }
                  @if (stage.lossGrams !== null) {
                    <small class="loss"><strong>Loss:</strong> {{ stage.lossGrams | number: '1.0-4' }}g</small>
                  }
                </div>

                <div class="stage-actions">
                  @if (canStartStage(stage)) {
                    <button mat-flat-button color="primary" (click)="onStart(stage)" [disabled]="busy()">
                      <mat-icon>play_arrow</mat-icon> Start
                    </button>
                    <button mat-button (click)="onSkip(stage)" [disabled]="busy()">
                      <mat-icon>skip_next</mat-icon> Skip
                    </button>
                  }
                  @if (canCompleteStage(stage)) {
                    <button mat-flat-button color="primary" (click)="onComplete(stage)" [disabled]="busy()">
                      <mat-icon>check</mat-icon> Complete
                    </button>
                    <button mat-stroked-button color="warn" (click)="onFail(stage)" [disabled]="busy()">
                      <mat-icon>error</mat-icon> Fail
                    </button>
                  }
                </div>
              </div>
            }
          </div>
        </mat-card-content>
      </mat-card>

      @if (w.notes) {
        <mat-card class="notes-card">
          <mat-card-header><mat-card-title>Notes</mat-card-title></mat-card-header>
          <mat-card-content><p class="notes-text">{{ w.notes }}</p></mat-card-content>
        </mat-card>
      }
    }
  `,
  styles: [`
    .page-header {
      display: flex; align-items: center; gap: 12px; margin-bottom: 16px;
    }
    .header-info { flex: 1; }
    .header-info h1 { margin: 0 0 4px; font-weight: 500; }
    .subtitle { margin: 0; color: #607d8b; font-size: 13px; display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
    .muted { color: #607d8b; }
    .error {
      display: flex; gap: 8px; align-items: center;
      padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin: 12px 0;
    }

    .summary-grid {
      display: grid; grid-template-columns: 2fr 1fr; gap: 16px; margin-bottom: 16px;
    }
    .metric-row { display: flex; gap: 24px; align-items: center; }
    .metric { flex: 1; text-align: center; }
    .m-label { font-size: 11px; color: #607d8b; text-transform: uppercase; }
    .m-value { font-size: 28px; font-weight: 500; color: #1a237e; }
    .m-value.loss { color: #c62828; }
    .m-value small { font-size: 14px; color: #c62828; margin-left: 2px; }
    .progress-track { height: 4px; background: #eceff1; border-radius: 2px; margin-top: 6px; overflow: hidden; }
    .progress-fill { height: 100%; background: #43a047; }

    dl { display: grid; grid-template-columns: 100px 1fr; gap: 6px 12px; margin: 0; }
    dt { color: #607d8b; font-size: 12px; }
    dd { margin: 0; font-size: 13px; }
    dd.neg { color: #c62828; }

    .actions-card { margin-bottom: 16px; }
    .actions { display: flex; gap: 8px; flex-wrap: wrap; }

    .stages-card { margin-bottom: 16px; }
    .stages-list { display: flex; flex-direction: column; gap: 4px; }
    .stage-row {
      display: grid;
      grid-template-columns: 28px 48px 1fr 200px 180px 220px;
      gap: 12px;
      align-items: center;
      padding: 12px 8px;
      border-radius: 6px;
      transition: background 0.15s;
    }
    .stage-row:hover { background: #f5f5f5; }
    .stage-row.stage-completed { background: #e8f5e9; }
    .stage-row.stage-skipped { background: #f5f5f5; opacity: 0.7; }
    .stage-row.stage-failed { background: #ffebee; }
    .stage-row.stage-inprogress { background: #e3f2fd; }

    .stage-num { font-weight: 600; color: #607d8b; text-align: center; }
    .stage-icon-cell { display: flex; }
    .stage-icon-bg {
      width: 40px; height: 40px; border-radius: 50%;
      display: flex; align-items: center; justify-content: center;
      background: #eceff1;
    }
    .stage-icon-bg.bg-completed { background: #c8e6c9; color: #1b5e20; }
    .stage-icon-bg.bg-inprogress { background: #1976d2; color: #fff; }
    .stage-icon-bg.bg-skipped { background: #cfd8dc; color: #607d8b; }
    .stage-icon-bg.bg-failed { background: #ffcdd2; color: #b71c1c; }

    .stage-name { font-weight: 500; font-size: 14px; }
    .stage-meta { display: flex; gap: 8px; align-items: center; margin-top: 2px; }
    .stage-meta mat-chip { height: 20px; font-size: 10px; }
    .i-small { font-size: 14px; height: 14px; width: 14px; vertical-align: middle; }
    .notes { display: block; color: #607d8b; font-size: 11px; margin-top: 4px; }
    .notes.neg { color: #c62828; }

    .stage-time, .stage-weight { display: flex; flex-direction: column; gap: 2px; }
    .stage-time small, .stage-weight small { font-size: 11px; }
    .loss { color: #c62828; }

    .stage-actions { display: flex; gap: 4px; justify-content: flex-end; }

    .notes-card { margin-bottom: 16px; }
    .notes-text { white-space: pre-wrap; line-height: 1.5; }

    /* WO status chips */
    mat-chip.wo-status { font-size: 11px; height: 22px; }
    mat-chip.wo-status-draft      { background: #eceff1; color: #455a64; }
    mat-chip.wo-status-released   { background: #e3f2fd; color: #0d47a1; }
    mat-chip.wo-status-inprogress { background: #fff3e0; color: #e65100; }
    mat-chip.wo-status-onhold     { background: #fff8e1; color: #6d4c00; }
    mat-chip.wo-status-completed  { background: #c8e6c9; color: #1b5e20; }
    mat-chip.wo-status-cancelled  { background: #ffcdd2; color: #b71c1c; }

    /* Priority chips */
    mat-chip.priority-low      { background: #eceff1; color: #455a64; }
    mat-chip.priority-normal   { background: #e0f7fa; color: #006064; }
    mat-chip.priority-high     { background: #fff3e0; color: #e65100; }
    mat-chip.priority-urgent   { background: #ffcdd2; color: #b71c1c; font-weight: 600; }

    /* Stage status chips */
    mat-chip.stage-status-pending     { background: #eceff1; color: #455a64; }
    mat-chip.stage-status-inprogress  { background: #1976d2; color: #fff; }
    mat-chip.stage-status-completed   { background: #43a047; color: #fff; }
    mat-chip.stage-status-skipped     { background: #cfd8dc; color: #455a64; }
    mat-chip.stage-status-failed      { background: #c62828; color: #fff; }

    @media (max-width: 1200px) {
      .stage-row {
        grid-template-columns: 28px 48px 1fr;
        grid-template-rows: auto auto auto;
        gap: 8px;
      }
      .stage-time, .stage-weight, .stage-actions { grid-column: 1 / -1; }
      .summary-grid { grid-template-columns: 1fr; }
    }
  `],
})
export class WorkOrderDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(WorkOrderService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly STATUS = WorkOrderStatus;

  readonly wo = signal<WorkOrder | null>(null);
  readonly loading = signal(false);
  readonly changing = signal(false);
  readonly stageBusy = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);

  readonly busy = computed(() => this.changing() || this.stageBusy() !== null);

  readonly allowedNextStatuses = computed(() => {
    const w = this.wo();
    return w ? nextAllowedStatuses(w.status) : [];
  });

  readonly completedCount = computed(() =>
    this.wo()?.stages.filter((s) => s.status === 'Completed').length ?? 0);

  readonly activeStageCount = computed(() =>
    this.wo()?.stages.filter((s) => s.status !== 'Skipped').length ?? 0);

  readonly progressPercent = computed(() => {
    const total = this.activeStageCount();
    return total > 0 ? Math.round((this.completedCount() / total) * 100) : 0;
  });

  readonly totalLoss = computed(() => {
    const stages = this.wo()?.stages ?? [];
    return stages.reduce((sum, s) => sum + (s.lossGrams ?? 0), 0);
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.load(id);
  }

  // ── Display helpers ─────────────────────────────────────────────────
  stagePretty(s: string): string { return STAGE_PRETTY[s] ?? s; }
  stageIcon(s: string): string { return STAGE_ICONS[s] ?? 'circle'; }
  statusLabel(v: WorkOrderStatusValue): string { return STATUS_LABELS[v]; }
  statusIcon(v: WorkOrderStatusValue): string {
    return ({
      [WorkOrderStatus.Released]: 'check_circle',
      [WorkOrderStatus.InProgress]: 'play_arrow',
      [WorkOrderStatus.OnHold]: 'pause',
      [WorkOrderStatus.Completed]: 'task_alt',
      [WorkOrderStatus.Cancelled]: 'cancel',
      [WorkOrderStatus.Draft]: 'edit',
    } as Record<number, string>)[v] ?? 'circle';
  }
  statusIcon2(s: string): string | null {
    if (s === 'Completed') return 'check';
    if (s === 'Skipped') return 'remove';
    if (s === 'Failed') return 'close';
    return null;
  }

  canStartStage(s: WorkOrderStage): boolean {
    const w = this.wo();
    if (!w) return false;
    return s.status === 'Pending'
      && (w.status === 'Released' || w.status === 'InProgress');
  }
  canCompleteStage(s: WorkOrderStage): boolean {
    return s.status === 'InProgress';
  }

  // ── WO status transitions ───────────────────────────────────────────
  onTransition(next: WorkOrderStatusValue): void {
    const w = this.wo();
    if (!w) return;
    this.applyWoStatus(w, next, null);
  }

  onCancel(): void {
    const w = this.wo();
    if (!w) return;
    const ref = this.dialog.open<CancelWoDialog, CancelWoDialogData, string | undefined>(
      CancelWoDialog,
      { width: '480px', data: { workOrderNumber: w.workOrderNumber } },
    );
    ref.afterClosed().subscribe((reason) => {
      if (reason) this.applyWoStatus(w, WorkOrderStatus.Cancelled, reason);
    });
  }

  private applyWoStatus(w: WorkOrder, next: WorkOrderStatusValue, reason: string | null): void {
    this.changing.set(true);
    this.errorMessage.set(null);
    this.service.changeStatus(w.id, {
      newStatus: next, reason, rowVersion: w.rowVersion,
    }).subscribe({
      next: (updated) => {
        this.wo.set(updated);
        this.changing.set(false);
        this.snack.open(`Work order moved to ${STATUS_LABELS[next]}`, 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.changing.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to change status.'));
      },
    });
  }

  // ── Stage operations ────────────────────────────────────────────────
  onStart(stage: WorkOrderStage): void {
    const w = this.wo();
    if (!w) return;
    const ref = this.dialog.open<StartStageDialog, StartStageDialogData, StartStageDialogResult | undefined>(
      StartStageDialog, { width: '460px', data: { stage } },
    );
    ref.afterClosed().subscribe((res) => {
      if (!res) return;
      this.runStage(w.id, stage.id, () =>
        this.service.startStage(w.id, stage.id, {
          assignedWorkerId: res.assignedWorkerId,
          weightInGrams: res.weightInGrams,
          rowVersion: stage.rowVersion,
        }),
      );
    });
  }

  onComplete(stage: WorkOrderStage): void {
    const w = this.wo();
    if (!w) return;
    const ref = this.dialog.open<CompleteStageDialog, CompleteStageDialogData, CompleteStageDialogResult | undefined>(
      CompleteStageDialog, { width: '460px', data: { stage } },
    );
    ref.afterClosed().subscribe((res) => {
      if (!res) return;
      this.runStage(w.id, stage.id, () =>
        this.service.completeStage(w.id, stage.id, {
          weightOutGrams: res.weightOutGrams,
          actualHours: res.actualHours,
          notes: res.notes,
          rowVersion: stage.rowVersion,
        }),
      );
    });
  }

  onSkip(stage: WorkOrderStage): void {
    const w = this.wo();
    if (!w) return;
    const ref = this.dialog.open<SkipStageDialog, SkipStageDialogData, SkipStageDialogResult | undefined>(
      SkipStageDialog, { width: '460px', data: { stage } },
    );
    ref.afterClosed().subscribe((reason) => {
      if (!reason) return;
      this.runStage(w.id, stage.id, () =>
        this.service.skipStage(w.id, stage.id, { reason, rowVersion: stage.rowVersion }),
      );
    });
  }

  onFail(stage: WorkOrderStage): void {
    const w = this.wo();
    if (!w) return;
    const ref = this.dialog.open<FailStageDialog, FailStageDialogData, FailStageDialogResult | undefined>(
      FailStageDialog, { width: '460px', data: { stage } },
    );
    ref.afterClosed().subscribe((res) => {
      if (!res) return;
      this.runStage(w.id, stage.id, () =>
        this.service.failStage(w.id, stage.id, {
          failureReason: res.failureReason,
          weightOutGrams: res.weightOutGrams,
          rowVersion: stage.rowVersion,
        }),
      );
    });
  }

  /** Helper that wraps a stage call with the busy flag and a reload on success. */
  private runStage(woId: string, stageId: string, action: () => any): void {
    this.stageBusy.set(stageId);
    this.errorMessage.set(null);
    action().subscribe({
      next: () => {
        // Reload the whole WO so progress / loss / status update consistently.
        this.load(woId, /* keepBusy */ true);
        this.snack.open('Stage updated', 'Close', { duration: 2500 });
      },
      error: (err: unknown) => {
        this.stageBusy.set(null);
        this.errorMessage.set(extractErrorMessage(err, 'Stage operation failed.'));
      },
    });
  }

  private load(id: string, keepBusy = false): void {
    if (!keepBusy) this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (w) => {
        this.wo.set(w);
        this.loading.set(false);
        this.stageBusy.set(null);
      },
      error: (err) => {
        this.loading.set(false);
        this.stageBusy.set(null);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to load work order.'));
      },
    });
  }
}
