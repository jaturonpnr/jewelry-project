import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { RawMaterialItem } from '../../inventory.types';

export interface AdjustRawMaterialDialogData {
  item: RawMaterialItem;
}

export interface AdjustRawMaterialDialogResult {
  quantityDelta: number;
  reason: string;
  notes?: string | null;
}

@Component({
  selector: 'app-adjust-raw-material-dialog',
  imports: [
    ReactiveFormsModule,
    DecimalPipe,
    MatButtonModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatDialogClose,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2 mat-dialog-title>Adjust quantity</h2>
    <form [formGroup]="form" (ngSubmit)="confirm()">
      <mat-dialog-content>
        <div class="header">
          <div>
            <strong>{{ data.item.materialTypeName }}</strong>
            <small class="muted">{{ data.item.materialTypeCode }} · {{ data.item.lotNumber }} · {{ data.item.location }}</small>
          </div>
          <div class="current">
            <span class="label">Current</span>
            <span class="value">{{ data.item.quantity | number: '1.0-4' }} <small>{{ data.item.materialUnit }}</small></span>
          </div>
        </div>

        <div class="grid">
          <mat-form-field appearance="outline">
            <mat-label>Direction</mat-label>
            <mat-select formControlName="direction">
              <mat-option value="add">+ Add stock</mat-option>
              <mat-option value="remove">− Remove stock (issue)</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Amount ({{ data.item.materialUnit }})</mat-label>
            <input matInput type="number" min="0.0001" step="0.0001" formControlName="amount" />
            @if (form.controls.amount.hasError('required')) {
              <mat-error>Amount is required</mat-error>
            }
            @if (form.controls.amount.hasError('min')) {
              <mat-error>Must be greater than 0</mat-error>
            }
          </mat-form-field>
        </div>

        @if (preview() !== null) {
          <div class="preview" [class.negative]="preview()! < 0">
            <mat-icon>arrow_forward</mat-icon>
            <span>New quantity: <strong>{{ preview() | number: '1.0-4' }}</strong> {{ data.item.materialUnit }}</span>
            @if (preview()! < 0) {
              <small class="warn">Negative — will be rejected by server</small>
            }
          </div>
        }

        <mat-form-field appearance="outline" class="full">
          <mat-label>Reason</mat-label>
          <input matInput formControlName="reason" placeholder="e.g. Issued to WO-001 / Scrap recovery" />
          @if (form.controls.reason.hasError('required')) {
            <mat-error>Reason is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Notes (optional)</mat-label>
          <textarea matInput formControlName="notes" rows="2"></textarea>
        </mat-form-field>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-button mat-dialog-close type="button">Cancel</button>
        <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid">
          Apply Adjustment
        </button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 16px;
      padding: 12px 16px;
      background: #f5f5f5;
      border-radius: 4px;
      margin-bottom: 16px;
    }
    .header strong { display: block; }
    .muted { color: #607d8b; font-size: 12px; display: block; }
    .current { text-align: right; }
    .current .label { color: #607d8b; font-size: 11px; display: block; }
    .current .value { font-size: 18px; font-weight: 500; }
    .grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 12px;
    }
    .full { width: 100%; }
    .preview {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 10px 14px;
      background: #e3f2fd;
      color: #0d47a1;
      border-radius: 4px;
      margin-bottom: 12px;
      font-size: 14px;
    }
    .preview.negative { background: #ffebee; color: #c62828; }
    .warn { font-size: 11px; margin-left: 8px; }
  `],
})
export class AdjustRawMaterialDialog {
  protected readonly data = inject<AdjustRawMaterialDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<AdjustRawMaterialDialog, AdjustRawMaterialDialogResult | undefined>);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    direction: ['remove' as 'add' | 'remove'],
    amount: [0, [Validators.required, Validators.min(0.0001)]],
    reason: ['', [Validators.required, Validators.maxLength(200)]],
    notes: [''],
  });

  // Update preview reactively
  readonly preview = signal<number | null>(null);

  constructor() {
    this.form.valueChanges.subscribe(() => {
      const amt = this.form.controls.amount.value;
      if (!amt || amt <= 0) {
        this.preview.set(null);
        return;
      }
      const dir = this.form.controls.direction.value === 'add' ? 1 : -1;
      this.preview.set(this.data.item.quantity + dir * amt);
    });
  }

  confirm(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const sign = v.direction === 'add' ? 1 : -1;
    this.dialogRef.close({
      quantityDelta: sign * v.amount,
      reason: v.reason.trim(),
      notes: v.notes?.trim() || null,
    });
  }
}
