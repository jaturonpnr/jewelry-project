import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy, ChangeDetectorRef, Component,
  inject, OnInit, signal,
} from '@angular/core';
import {
  AbstractControl, FormArray, FormBuilder, FormGroup,
  ReactiveFormsModule, Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../../core/api/error-utils';
import { WorkOrderService } from '../../work-orders/work-order.service';
import { WorkerService } from '../../workers/worker.service';
import { QcService } from '../qc.service';
import {
  DEFECT_SEVERITY_LABELS,
  DEFECT_TYPE_PRESETS,
  DefectSeverityValue,
  QC_RESULT_LABELS,
  QC_TYPE_LABELS,
  QcInspectionResponse,
  QcInspectionTypeValue,
  QcResult,
  QcResultValue,
  STAGE_LABELS,
  ProductionStageValue,
} from '../qc.types';

@Component({
  selector: 'app-qc-form',
  imports: [
    ReactiveFormsModule, RouterLink, DatePipe,
    MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatDividerModule,
    MatChipsModule, MatSnackBarModule, MatTooltipModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h1>{{ isView() ? 'QC Inspection Detail' : 'New QC Inspection' }}</h1>
        <p class="subtitle">Record quality check result and defects found</p>
      </div>
      <div class="header-actions">
        <button mat-button routerLink="/qc">Back to list</button>
        @if (!isView()) {
          <button mat-flat-button color="primary" [disabled]="saving()" (click)="save()">
            <mat-icon>save</mat-icon> {{ saving() ? 'Saving…' : 'Submit Inspection' }}
          </button>
        }
      </div>
    </div>

    @if (errorMessage(); as msg) {
      <div class="error-banner"><mat-icon>error_outline</mat-icon> {{ msg }}</div>
    }

    <!-- View mode (read-only detail) -->
    @if (isView() && viewData(); as v) {
      <div class="cards-grid">
        <mat-card>
          <mat-card-header><mat-card-title>Inspection Info</mat-card-title></mat-card-header>
          <mat-card-content>
            <div class="info-grid">
              <span class="label">Type</span>
              <mat-chip [class]="'type-' + v.inspectionType">{{ typeLabel(v.inspectionType) }}</mat-chip>
              <span class="label">Date</span>
              <span>{{ v.inspectionDate | date:'mediumDate' }}</span>
              <span class="label">Inspector</span>
              <span>{{ v.inspectorName }}</span>
              <span class="label">Result</span>
              <mat-chip [class]="'result-' + v.result">{{ resultLabel(v.result) }}</mat-chip>
              @if (v.workOrderNumber) {
                <span class="label">Work Order</span>
                <a [routerLink]="['/work-orders', v.workOrderId]">{{ v.workOrderNumber }}</a>
              }
              @if (v.workOrderStageName) {
                <span class="label">Stage</span>
                <span>{{ v.workOrderStageName }}</span>
              }
              @if (v.reworkStage) {
                <span class="label">Rework Stage</span>
                <span>{{ stageLabel(v.reworkStage) }}</span>
              }
              @if (v.actualWeightGrams != null) {
                <span class="label">Weight (actual)</span>
                <span>{{ v.actualWeightGrams }}g @if (v.expectedWeightGrams) { / expected {{ v.expectedWeightGrams }}g }</span>
              }
              @if (v.notes) {
                <span class="label">Notes</span>
                <span>{{ v.notes }}</span>
              }
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card>
          <mat-card-header>
            <mat-card-title>Defects Found ({{ v.defects.length }})</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            @if (v.defects.length === 0) {
              <p class="muted">No defects recorded.</p>
            }
            @for (d of v.defects; track d.id) {
              <div class="defect-item">
                <mat-chip [class]="'sev-' + d.severity">{{ severityLabel(d.severity) }}</mat-chip>
                <span class="defect-type">{{ d.defectType }}</span>
                <span class="defect-qty">×{{ d.quantity }}</span>
                @if (d.description) { <span class="muted">— {{ d.description }}</span> }
              </div>
            }
          </mat-card-content>
        </mat-card>
      </div>
    }

    <!-- Create mode (form) -->
    @if (!isView()) {
      <form [formGroup]="form" class="cards-grid">
        <mat-card>
          <mat-card-header><mat-card-title>Inspection Details</mat-card-title></mat-card-header>
          <mat-card-content class="form-grid-2">

            <mat-form-field appearance="outline">
              <mat-label>Inspection Type *</mat-label>
              <mat-select formControlName="inspectionType" (selectionChange)="onTypeChange()">
                @for (opt of typeOptions; track opt.value) {
                  <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Inspection Date *</mat-label>
              <input matInput type="date" formControlName="inspectionDate" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Inspector Name *</mat-label>
              <input matInput formControlName="inspectorName" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Result *</mat-label>
              <mat-select formControlName="result">
                @for (opt of resultOptions; track opt.value) {
                  <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            @if (f['result'].value === QcResultRework) {
              <mat-form-field appearance="outline">
                <mat-label>Rework Stage *</mat-label>
                <mat-select formControlName="reworkStage">
                  @for (s of stageOptions; track s.value) {
                    <mat-option [value]="s.value">{{ s.label }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>
            }

            @if (f['inspectionType'].value !== QcTypeIncoming) {
              <mat-form-field appearance="outline">
                <mat-label>Work Order Number</mat-label>
                <input matInput formControlName="workOrderNumber"
                       placeholder="WO-2026-0001" (blur)="lookupWorkOrder()" />
                @if (workOrderId()) {
                  <mat-icon matSuffix color="primary">check_circle</mat-icon>
                }
              </mat-form-field>
            }

            <mat-form-field appearance="outline">
              <mat-label>Actual Weight (g)</mat-label>
              <input matInput type="number" formControlName="actualWeightGrams" step="0.0001" />
              <span matSuffix>g</span>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Expected Weight (g)</mat-label>
              <input matInput type="number" formControlName="expectedWeightGrams" step="0.0001" />
              <span matSuffix>g</span>
            </mat-form-field>

            <mat-form-field appearance="outline" class="span-2">
              <mat-label>Notes</mat-label>
              <textarea matInput formControlName="notes" rows="2"></textarea>
            </mat-form-field>
          </mat-card-content>
        </mat-card>

        <mat-card>
          <mat-card-header>
            <mat-card-title>Defects</mat-card-title>
            <span class="spacer"></span>
            <button mat-stroked-button color="primary" type="button" (click)="addDefect()">
              <mat-icon>add</mat-icon> Add Defect
            </button>
          </mat-card-header>
          <mat-card-content>
            @if (defectLines.length === 0) {
              <p class="muted">No defects — or click "Add Defect" to record one.</p>
            }
            @for (line of defectLines.controls; track $index) {
              <div class="defect-row" [formGroup]="asGroup(line)">
                <mat-form-field appearance="outline" class="w160">
                  <mat-label>Defect Type</mat-label>
                  <mat-select formControlName="defectType">
                    @for (t of defectTypeOptions; track t) {
                      <mat-option [value]="t">{{ t }}</mat-option>
                    }
                  </mat-select>
                </mat-form-field>

                <mat-form-field appearance="outline" class="w120">
                  <mat-label>Severity</mat-label>
                  <mat-select formControlName="severity">
                    @for (s of severityOptions; track s.value) {
                      <mat-option [value]="s.value">{{ s.label }}</mat-option>
                    }
                  </mat-select>
                </mat-form-field>

                <mat-form-field appearance="outline" class="w60">
                  <mat-label>Qty</mat-label>
                  <input matInput type="number" formControlName="quantity" min="1" />
                </mat-form-field>

                <mat-form-field appearance="outline" class="flex">
                  <mat-label>Description</mat-label>
                  <input matInput formControlName="description" />
                </mat-form-field>

                <button mat-icon-button color="warn" type="button" (click)="removeDefect($index)">
                  <mat-icon>delete</mat-icon>
                </button>
              </div>
            }
          </mat-card-content>
        </mat-card>
      </form>
    }
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 16px; }
    .page-header h1 { margin: 0 0 4px; font-weight: 500; }
    .subtitle { margin: 0; color: #607d8b; font-size: 13px; }
    .header-actions { display: flex; gap: 8px; }
    .error-banner { display: flex; gap: 8px; align-items: center; padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin-bottom: 16px; }
    .cards-grid { display: flex; flex-direction: column; gap: 16px; }
    mat-card-header { display: flex; align-items: center; margin-bottom: 16px; }
    .spacer { flex: 1; }
    .form-grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .span-2 { grid-column: span 2; }
    .info-grid { display: grid; grid-template-columns: 120px 1fr; gap: 8px 16px; align-items: center; }
    .label { color: #607d8b; font-size: 13px; }
    .defect-item { display: flex; align-items: center; gap: 8px; padding: 6px 0; border-bottom: 1px solid #f0f0f0; }
    .defect-type { font-weight: 500; }
    .defect-qty { background: #eceff1; padding: 2px 6px; border-radius: 4px; font-size: 12px; }
    .defect-row { display: flex; gap: 8px; align-items: flex-start; padding: 4px 0; flex-wrap: wrap; }
    .defect-row mat-form-field { margin-bottom: 0; }
    .w60 { width: 60px; }
    .w120 { width: 120px; }
    .w160 { width: 160px; }
    .flex { flex: 1; min-width: 120px; }
    .muted { color: #90a4ae; font-style: italic; }
    /* type chips */
    mat-chip.type-1 { background: #e3f2fd; color: #0d47a1; }
    mat-chip.type-2 { background: #fff3e0; color: #e65100; }
    mat-chip.type-3 { background: #f3e5f5; color: #6a1b9a; }
    /* result chips */
    mat-chip.result-1 { background: #c8e6c9; color: #1b5e20; }
    mat-chip.result-2 { background: #fff9c4; color: #f57f17; }
    mat-chip.result-3 { background: #ffcdd2; color: #b71c1c; font-weight: 600; }
    /* severity chips */
    mat-chip.sev-1 { background: #e8f5e9; color: #2e7d32; }
    mat-chip.sev-2 { background: #fff3e0; color: #e65100; }
    mat-chip.sev-3 { background: #ffcdd2; color: #b71c1c; font-weight: 700; }
  `],
})
export class QcForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(QcService);
  private readonly woSvc = inject(WorkOrderService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snack = inject(MatSnackBar);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly QcResultRework = QcResult.Rework;
  readonly QcTypeIncoming = 1 as QcInspectionTypeValue;

  readonly isView = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly viewData = signal<QcInspectionResponse | null>(null);
  readonly workOrderId = signal<string | null>(null);

  readonly typeOptions = Object.entries(QC_TYPE_LABELS).map(([v, label]) => ({
    value: Number(v) as QcInspectionTypeValue, label,
  }));
  readonly resultOptions = Object.entries(QC_RESULT_LABELS).map(([v, label]) => ({
    value: Number(v) as QcResultValue, label,
  }));
  readonly severityOptions = Object.entries(DEFECT_SEVERITY_LABELS).map(([v, label]) => ({
    value: Number(v) as DefectSeverityValue, label,
  }));
  readonly stageOptions = Object.entries(STAGE_LABELS).map(([v, label]) => ({
    value: Number(v) as ProductionStageValue, label,
  }));
  readonly defectTypeOptions = DEFECT_TYPE_PRESETS;

  typeLabel(v: QcInspectionTypeValue) { return QC_TYPE_LABELS[v]; }
  resultLabel(v: QcResultValue) { return QC_RESULT_LABELS[v]; }
  severityLabel(v: DefectSeverityValue) { return DEFECT_SEVERITY_LABELS[v]; }
  stageLabel(v: ProductionStageValue) { return STAGE_LABELS[v]; }

  readonly form = this.fb.group({
    inspectionType: [1 as QcInspectionTypeValue, Validators.required],
    inspectionDate: [new Date().toISOString().split('T')[0], Validators.required],
    inspectorName: ['', [Validators.required, Validators.maxLength(200)]],
    result: [1 as QcResultValue, Validators.required],
    reworkStage: [null as ProductionStageValue | null],
    workOrderNumber: [''],
    actualWeightGrams: [null as number | null],
    expectedWeightGrams: [null as number | null],
    notes: [''],
    defects: this.fb.array([]),
  });

  get f() { return this.form.controls; }
  get defectLines() { return this.f['defects'] as FormArray; }
  asGroup(ctrl: AbstractControl) { return ctrl as FormGroup; }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isView.set(true);
      this.svc.getById(id).subscribe({
        next: (v) => { this.viewData.set(v); this.cdr.markForCheck(); },
        error: () => this.router.navigate(['/qc']),
      });
    }
  }

  onTypeChange(): void {
    // Clear work order when switching to Incoming
    if (this.f['inspectionType'].value === this.QcTypeIncoming) {
      this.f['workOrderNumber'].setValue('');
      this.workOrderId.set(null);
    }
  }

  lookupWorkOrder(): void {
    const num = this.f['workOrderNumber'].value?.trim();
    if (!num) { this.workOrderId.set(null); return; }
    this.woSvc.getPaged({ page: 1, pageSize: 1, search: num }).subscribe({
      next: (r) => {
        const wo = r.items.find(w => w.workOrderNumber === num);
        this.workOrderId.set(wo?.id ?? null);
        this.cdr.markForCheck();
      },
      error: () => this.workOrderId.set(null),
    });
  }

  addDefect(): void {
    this.defectLines.push(this.fb.group({
      defectType: ['Scratch', Validators.required],
      severity: [1 as DefectSeverityValue, Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      description: [''],
    }));
  }
  removeDefect(i: number): void { this.defectLines.removeAt(i); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    this.errorMessage.set(null);

    const v = this.form.getRawValue();
    const dto = {
      inspectionType: v.inspectionType!,
      inspectionDate: v.inspectionDate!,
      workOrderId: this.workOrderId() ?? undefined,
      inspectorName: v.inspectorName!,
      result: v.result!,
      reworkStage: v.result === QcResult.Rework ? v.reworkStage : null,
      actualWeightGrams: v.actualWeightGrams ?? undefined,
      expectedWeightGrams: v.expectedWeightGrams ?? undefined,
      notes: v.notes || undefined,
      defects: this.defectLines.value.map((d: any) => ({
        defectType: d.defectType,
        severity: d.severity,
        quantity: d.quantity,
        description: d.description || undefined,
      })),
    };

    this.svc.create(dto).subscribe({
      next: (id) => {
        this.saving.set(false);
        this.snack.open('Inspection recorded.', 'OK', { duration: 3000 });
        this.router.navigate(['/qc', id]);
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to save inspection.'));
      },
    });
  }
}
