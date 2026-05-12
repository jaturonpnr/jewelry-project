import { CurrencyPipe } from '@angular/common';
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
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../../core/api/error-utils';
import { BomService } from '../bom.service';
import {
  BomTemplateResponse,
  MATERIAL_CATEGORY_LABELS,
  MaterialCategoryValue,
  ProductionStageValue,
  STAGE_LABELS,
  StoneTrackingType,
} from '../bom.types';

@Component({
  selector: 'app-bom-form',
  imports: [
    ReactiveFormsModule, RouterLink, CurrencyPipe,
    MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatCheckboxModule, MatButtonModule, MatIconModule, MatDividerModule,
    MatSnackBarModule, MatTooltipModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h1>{{ isEdit() ? 'Edit BOM: ' + (bomCode() || '') : 'New BOM Template' }}</h1>
        <p class="subtitle">Define materials, stones, and labour cost per piece</p>
      </div>
      <div class="header-actions">
        <button mat-button routerLink="/bom">Cancel</button>
        <button mat-flat-button color="primary" [disabled]="saving()" (click)="save()">
          <mat-icon>save</mat-icon> {{ saving() ? 'Saving…' : 'Save BOM' }}
        </button>
      </div>
    </div>

    @if (errorMessage(); as msg) {
      <div class="error-banner"><mat-icon>error_outline</mat-icon> {{ msg }}</div>
    }

    <form [formGroup]="form" class="form-grid">

      <!-- Header card -->
      <mat-card class="full-width">
        <mat-card-header><mat-card-title>Design Info</mat-card-title></mat-card-header>
        <mat-card-content class="row-3">
          <mat-form-field appearance="outline">
            <mat-label>Design Code *</mat-label>
            <input matInput formControlName="designCode" placeholder="RNG-18K-D001" style="text-transform:uppercase" />
            @if (f['designCode'].hasError('required') && f['designCode'].touched) {
              <mat-error>Required</mat-error>
            }
            @if (f['designCode'].hasError('pattern') && f['designCode'].touched) {
              <mat-error>Letters, digits, and hyphens only</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Design Name *</mat-label>
            <input matInput formControlName="designName" />
            @if (f['designName'].hasError('required') && f['designName'].touched) {
              <mat-error>Required</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Overhead %</mat-label>
            <input matInput type="number" formControlName="overheadPercent" min="0" max="100" />
            <span matSuffix>%</span>
          </mat-form-field>

          <mat-form-field appearance="outline" class="span-2">
            <mat-label>Description</mat-label>
            <textarea matInput formControlName="description" rows="2"></textarea>
          </mat-form-field>

          @if (isEdit()) {
            <div class="active-toggle">
              <mat-checkbox formControlName="isActive">Active (available for production)</mat-checkbox>
            </div>
          }
        </mat-card-content>
      </mat-card>

      <!-- Material Lines -->
      <mat-card class="full-width">
        <mat-card-header>
          <mat-card-title>Material Lines</mat-card-title>
          <span class="spacer"></span>
          <button mat-stroked-button color="primary" type="button" (click)="addMaterial()">
            <mat-icon>add</mat-icon> Add Material
          </button>
        </mat-card-header>
        <mat-card-content>
          @if (materialLines.length === 0) {
            <p class="empty-hint">No materials yet. Click "Add Material" to begin.</p>
          }
          @for (line of materialLines.controls; track $index) {
            <div class="line-row" [formGroup]="asGroup(line)">
              <mat-form-field appearance="outline" class="w120">
                <mat-label>Category</mat-label>
                <mat-select formControlName="category">
                  @for (cat of categoryOptions; track cat.value) {
                    <mat-option [value]="cat.value">{{ cat.label }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline" class="flex">
                <mat-label>Description</mat-label>
                <input matInput formControlName="materialDescription" placeholder="18K Yellow Gold" />
              </mat-form-field>

              <mat-form-field appearance="outline" class="w80">
                <mat-label>Karat</mat-label>
                <mat-select formControlName="karat">
                  <mat-option [value]="null">—</mat-option>
                  @for (k of karatOptions; track k.value) {
                    <mat-option [value]="k.value">{{ k.label }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline" class="w100">
                <mat-label>Qty (g)</mat-label>
                <input matInput type="number" formControlName="quantityGrams" step="0.0001" />
              </mat-form-field>

              <mat-form-field appearance="outline" class="w80">
                <mat-label>Loss %</mat-label>
                <input matInput type="number" formControlName="expectedLossPercent" step="0.1" />
                <span matSuffix>%</span>
              </mat-form-field>

              <mat-form-field appearance="outline" class="w120">
                <mat-label>Cost/g (THB)</mat-label>
                <input matInput type="number" formControlName="unitCostThbPerGram" step="0.01" />
              </mat-form-field>

              <div class="line-cost">
                {{ materialLineCost(asGroup(line)) | currency:'THB':'symbol-narrow':'1.0-0' }}
              </div>

              <button mat-icon-button color="warn" type="button" (click)="removeMaterial($index)"
                      matTooltip="Remove">
                <mat-icon>delete</mat-icon>
              </button>
            </div>
          }
        </mat-card-content>
      </mat-card>

      <!-- Stone Lines -->
      <mat-card class="full-width">
        <mat-card-header>
          <mat-card-title>Stone Lines</mat-card-title>
          <span class="spacer"></span>
          <button mat-stroked-button color="primary" type="button" (click)="addStone()">
            <mat-icon>add</mat-icon> Add Stone
          </button>
        </mat-card-header>
        <mat-card-content>
          @if (stoneLines.length === 0) {
            <p class="empty-hint">No stones in this design.</p>
          }
          @for (line of stoneLines.controls; track $index) {
            <div class="line-row" [formGroup]="asGroup(line)">
              <mat-form-field appearance="outline" class="w120">
                <mat-label>Stone Type</mat-label>
                <input matInput formControlName="stoneType" placeholder="Diamond" />
              </mat-form-field>

              <mat-form-field appearance="outline" class="w120">
                <mat-label>Shape</mat-label>
                <input matInput formControlName="stoneShape" placeholder="Round Brilliant" />
              </mat-form-field>

              <mat-form-field appearance="outline" class="flex">
                <mat-label>Size / Grade</mat-label>
                <input matInput formControlName="sizeDescription" placeholder="0.50ct RB D/VVS1 GIA" />
              </mat-form-field>

              <mat-form-field appearance="outline" class="w80">
                <mat-label>Ct/stone</mat-label>
                <input matInput type="number" formControlName="caratPerStone" step="0.0001" />
              </mat-form-field>

              <mat-form-field appearance="outline" class="w60">
                <mat-label>Qty</mat-label>
                <input matInput type="number" formControlName="quantity" />
              </mat-form-field>

              <mat-form-field appearance="outline" class="w100">
                <mat-label>Tracking</mat-label>
                <mat-select formControlName="trackingType">
                  <mat-option [value]="1">Individual</mat-option>
                  <mat-option [value]="2">Parcel</mat-option>
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline" class="w120">
                <mat-label>Cost/ct (THB)</mat-label>
                <input matInput type="number" formControlName="unitCostThbPerCarat" step="100" />
              </mat-form-field>

              <div class="line-cost">
                {{ stoneLineCost(asGroup(line)) | currency:'THB':'symbol-narrow':'1.0-0' }}
              </div>

              <button mat-icon-button color="warn" type="button" (click)="removeStone($index)"
                      matTooltip="Remove">
                <mat-icon>delete</mat-icon>
              </button>
            </div>
          }
        </mat-card-content>
      </mat-card>

      <!-- Labour Lines -->
      <mat-card class="full-width">
        <mat-card-header>
          <mat-card-title>Labour Lines</mat-card-title>
          <span class="spacer"></span>
          <button mat-stroked-button color="primary" type="button" (click)="addLabor()">
            <mat-icon>add</mat-icon> Add Stage
          </button>
        </mat-card-header>
        <mat-card-content>
          @if (laborLines.length === 0) {
            <p class="empty-hint">No labour lines yet.</p>
          }
          @for (line of laborLines.controls; track $index) {
            <div class="line-row" [formGroup]="asGroup(line)">
              <mat-form-field appearance="outline" class="w180">
                <mat-label>Stage</mat-label>
                <mat-select formControlName="stage">
                  @for (s of stageOptions; track s.value) {
                    <mat-option [value]="s.value">{{ s.label }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline" class="w100">
                <mat-label>Hours</mat-label>
                <input matInput type="number" formControlName="estimatedHours" step="0.5" />
              </mat-form-field>

              <mat-form-field appearance="outline" class="w120">
                <mat-label>Rate/hr (THB)</mat-label>
                <input matInput type="number" formControlName="hourlyRateThb" step="50" />
              </mat-form-field>

              <div class="line-cost">
                {{ laborLineCost(asGroup(line)) | currency:'THB':'symbol-narrow':'1.0-0' }}
              </div>

              <button mat-icon-button color="warn" type="button" (click)="removeLabor($index)"
                      matTooltip="Remove">
                <mat-icon>delete</mat-icon>
              </button>
            </div>
          }
        </mat-card-content>
      </mat-card>

      <!-- Cost Summary card -->
      <mat-card class="full-width cost-card">
        <mat-card-header><mat-card-title>Cost Summary (per piece)</mat-card-title></mat-card-header>
        <mat-card-content>
          <div class="summary-grid">
            <span>Material</span>
            <span class="right">{{ totalMaterialCost() | currency:'THB':'symbol-narrow':'1.2-2' }}</span>
            <span>Stone</span>
            <span class="right">{{ totalStoneCost() | currency:'THB':'symbol-narrow':'1.2-2' }}</span>
            <span>Labour</span>
            <span class="right">{{ totalLaborCost() | currency:'THB':'symbol-narrow':'1.2-2' }}</span>
            <span class="sub-line">Subtotal</span>
            <span class="right sub-line">{{ subtotal() | currency:'THB':'symbol-narrow':'1.2-2' }}</span>
            <span>Overhead ({{ f['overheadPercent'].value }}%)</span>
            <span class="right">{{ overheadCost() | currency:'THB':'symbol-narrow':'1.2-2' }}</span>
            <mat-divider class="span-2"></mat-divider>
            <span class="total-label">Total / Piece</span>
            <span class="right total-value">{{ totalCost() | currency:'THB':'symbol-narrow':'1.2-2' }}</span>
          </div>
        </mat-card-content>
      </mat-card>

    </form>
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 16px; }
    .page-header h1 { margin: 0 0 4px; font-weight: 500; }
    .subtitle { margin: 0; color: #607d8b; font-size: 13px; }
    .header-actions { display: flex; gap: 8px; }
    .error-banner { display: flex; gap: 8px; align-items: center; padding: 12px; background: #ffebee; color: #c62828; border-radius: 4px; margin-bottom: 16px; }
    .form-grid { display: flex; flex-direction: column; gap: 16px; }
    .full-width { width: 100%; }
    mat-card-header { display: flex; align-items: center; margin-bottom: 16px; }
    .spacer { flex: 1; }
    .row-3 { display: grid; grid-template-columns: 1fr 1fr 120px; gap: 12px; }
    .span-2 { grid-column: span 2; }
    .active-toggle { display: flex; align-items: center; padding: 8px 0; }
    .line-row { display: flex; gap: 8px; align-items: flex-start; padding: 4px 0; flex-wrap: wrap; }
    .line-row mat-form-field { margin-bottom: 0; }
    .flex { flex: 1; min-width: 140px; }
    .w60 { width: 60px; }
    .w80 { width: 80px; }
    .w100 { width: 100px; }
    .w120 { width: 120px; }
    .w180 { width: 180px; }
    .line-cost { min-width: 100px; text-align: right; font-weight: 600; color: #1a237e; padding-top: 16px; font-size: 13px; }
    .empty-hint { color: #90a4ae; font-style: italic; padding: 8px 0; }
    .summary-grid { display: grid; grid-template-columns: 1fr auto; gap: 6px 24px; max-width: 360px; font-size: 14px; }
    .right { text-align: right; }
    .sub-line { font-weight: 500; }
    .span-2 { grid-column: span 2; margin: 4px 0; }
    .total-label { font-size: 16px; font-weight: 700; }
    .total-value { font-size: 16px; font-weight: 700; color: #1a237e; }
    .cost-card { background: #f8f9fa; }
  `],
})
export class BomForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(BomService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snack = inject(MatSnackBar);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly isEdit = signal(false);
  readonly bomCode = signal('');
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly categoryOptions = Object.entries(MATERIAL_CATEGORY_LABELS).map(([v, label]) => ({
    value: Number(v) as MaterialCategoryValue, label,
  }));
  readonly karatOptions = [
    { value: 24, label: '24K (999)' },
    { value: 22, label: '22K (916)' },
    { value: 18, label: '18K (750)' },
    { value: 14, label: '14K (585)' },
    { value: 10, label: '10K (417)' },
    { value: 9, label: '9K (375)' },
  ];
  readonly stageOptions = Object.entries(STAGE_LABELS).map(([v, label]) => ({
    value: Number(v) as ProductionStageValue, label,
  }));

  readonly form = this.fb.group({
    designCode: ['', [Validators.required, Validators.maxLength(50), Validators.pattern(/^[A-Za-z0-9-]+$/)]],
    designName: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    overheadPercent: [15, [Validators.required, Validators.min(0), Validators.max(100)]],
    isActive: [true],
    materialLines: this.fb.array([]),
    stoneLines: this.fb.array([]),
    laborLines: this.fb.array([]),
  });

  get f() { return this.form.controls; }
  get materialLines() { return this.f['materialLines'] as FormArray; }
  get stoneLines() { return this.f['stoneLines'] as FormArray; }
  get laborLines() { return this.f['laborLines'] as FormArray; }
  asGroup(ctrl: AbstractControl) { return ctrl as FormGroup; }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEdit.set(true);
      this.svc.getById(id).subscribe({
        next: (bom) => this.patchForm(bom),
        error: () => this.router.navigate(['/bom']),
      });
    }
  }

  private patchForm(bom: BomTemplateResponse): void {
    this.bomCode.set(bom.designCode);
    this.form.patchValue({
      designCode: bom.designCode,
      designName: bom.designName,
      description: bom.description ?? '',
      overheadPercent: bom.overheadPercent,
      isActive: bom.isActive,
    });
    bom.materialLines.forEach(m => this.materialLines.push(this.buildMaterialGroup(m)));
    bom.stoneLines.forEach(s => this.stoneLines.push(this.buildStoneGroup(s)));
    bom.laborLines.forEach(l => this.laborLines.push(this.buildLaborGroup(l)));
    this.cdr.markForCheck();
  }

  addMaterial(): void {
    this.materialLines.push(this.buildMaterialGroup());
  }
  removeMaterial(i: number): void { this.materialLines.removeAt(i); }

  addStone(): void { this.stoneLines.push(this.buildStoneGroup()); }
  removeStone(i: number): void { this.stoneLines.removeAt(i); }

  addLabor(): void { this.laborLines.push(this.buildLaborGroup()); }
  removeLabor(i: number): void { this.laborLines.removeAt(i); }

  private buildMaterialGroup(m?: any): FormGroup {
    return this.fb.group({
      category: [m?.category ?? 1, Validators.required],
      materialDescription: [m?.materialDescription ?? '', Validators.required],
      karat: [m?.karat ?? null],
      purityFraction: [m?.purityFraction ?? null],
      quantityGrams: [m?.quantityGrams ?? 1, [Validators.required, Validators.min(0.0001)]],
      expectedLossPercent: [m?.expectedLossPercent ?? 0, [Validators.required, Validators.min(0), Validators.max(100)]],
      unitCostThbPerGram: [m?.unitCostThbPerGram ?? 0, [Validators.required, Validators.min(0)]],
      sortOrder: [m?.sortOrder ?? 0],
    });
  }

  private buildStoneGroup(s?: any): FormGroup {
    return this.fb.group({
      stoneType: [s?.stoneType ?? '', Validators.required],
      stoneShape: [s?.stoneShape ?? '', Validators.required],
      sizeDescription: [s?.sizeDescription ?? '', Validators.required],
      caratPerStone: [s?.caratPerStone ?? 0.5, [Validators.required, Validators.min(0.0001)]],
      quantity: [s?.quantity ?? 1, [Validators.required, Validators.min(1)]],
      trackingType: [s?.trackingType ?? StoneTrackingType.Individual, Validators.required],
      unitCostThbPerCarat: [s?.unitCostThbPerCarat ?? 0, [Validators.required, Validators.min(0)]],
      sortOrder: [s?.sortOrder ?? 0],
    });
  }

  private buildLaborGroup(l?: any): FormGroup {
    return this.fb.group({
      stage: [l?.stage ?? 1, Validators.required],
      estimatedHours: [l?.estimatedHours ?? 1, [Validators.required, Validators.min(0.1)]],
      hourlyRateThb: [l?.hourlyRateThb ?? 250, [Validators.required, Validators.min(0)]],
    });
  }

  // ── Live cost helpers ─────────────────────────────────────────────────────

  materialLineCost(g: FormGroup): number {
    const qty = Number(g.value.quantityGrams) || 0;
    const rate = Number(g.value.unitCostThbPerGram) || 0;
    return qty * rate;
  }

  stoneLineCost(g: FormGroup): number {
    const ct = Number(g.value.caratPerStone) || 0;
    const qty = Number(g.value.quantity) || 0;
    const rate = Number(g.value.unitCostThbPerCarat) || 0;
    return ct * qty * rate;
  }

  laborLineCost(g: FormGroup): number {
    const hrs = Number(g.value.estimatedHours) || 0;
    const rate = Number(g.value.hourlyRateThb) || 0;
    return hrs * rate;
  }

  totalMaterialCost(): number {
    return this.materialLines.controls.reduce((s, c) => s + this.materialLineCost(c as FormGroup), 0);
  }
  totalStoneCost(): number {
    return this.stoneLines.controls.reduce((s, c) => s + this.stoneLineCost(c as FormGroup), 0);
  }
  totalLaborCost(): number {
    return this.laborLines.controls.reduce((s, c) => s + this.laborLineCost(c as FormGroup), 0);
  }
  subtotal(): number { return this.totalMaterialCost() + this.totalStoneCost() + this.totalLaborCost(); }
  overheadCost(): number { return this.subtotal() * (Number(this.f['overheadPercent'].value) || 0) / 100; }
  totalCost(): number { return this.subtotal() + this.overheadCost(); }

  // ── Save ──────────────────────────────────────────────────────────────────

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    this.errorMessage.set(null);

    const v = this.form.getRawValue();
    const materialLines = this.materialLines.value.map((m: any, i: number) => ({
      ...m, sortOrder: i + 1,
      purityFraction: m.karat ? (m.karat / 24) : null,
    }));
    const stoneLines = this.stoneLines.value.map((s: any, i: number) => ({ ...s, sortOrder: i + 1 }));
    const laborLines = this.laborLines.value;

    const id = this.route.snapshot.paramMap.get('id');

    if (this.isEdit() && id) {
      const dto = { ...v, isActive: v.isActive!, materialLines, stoneLines, laborLines };
      this.svc.update(id, dto as any).subscribe({
        next: () => {
          this.saving.set(false);
          this.snack.open('BOM saved.', 'OK', { duration: 3000 });
          this.router.navigate(['/bom', id]);
        },
        error: (err) => { this.saving.set(false); this.errorMessage.set(extractErrorMessage(err, 'Save failed.')); },
      });
    } else {
      const dto = { ...v, materialLines, stoneLines, laborLines };
      this.svc.create(dto as any).subscribe({
        next: (newId) => {
          this.saving.set(false);
          this.snack.open('BOM created.', 'OK', { duration: 3000 });
          this.router.navigate(['/bom', newId]);
        },
        error: (err) => { this.saving.set(false); this.errorMessage.set(extractErrorMessage(err, 'Create failed.')); },
      });
    }
  }
}
