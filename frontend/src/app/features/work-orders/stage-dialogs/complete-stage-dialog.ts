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
import { STAGE_PRETTY, WorkOrderStage } from '../work-order.types';

export interface CompleteStageDialogData {
  stage: WorkOrderStage;
}

export interface CompleteStageDialogResult {
  weightOutGrams: number | null;
  actualHours: number;
  notes: string | null;
}

@Component({
  selector: 'app-complete-stage-dialog',
  imports: [
    ReactiveFormsModule, DecimalPipe,
    MatButtonModule, MatIconModule,
    MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose,
    MatFormFieldModule, MatInputModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2 mat-dialog-title>Complete "{{ stagePretty(data.stage.stage) }}"</h2>
    <form [formGroup]="form" (ngSubmit)="confirm()">
      <mat-dialog-content>
        @if (data.stage.weightInGrams !== null) {
          <div class="weight-in-banner">
            <mat-icon>scale</mat-icon>
            <span>Weight In was <strong>{{ data.stage.weightInGrams | number: '1.0-4' }}g</strong></span>
          </div>
        }

        <mat-form-field appearance="outline" class="full">
          <mat-label>Weight Out (grams)</mat-label>
          <input matInput type="number" min="0" step="0.0001" formControlName="weightOutGrams" />
          <mat-hint>Gross weight EXITING this stage. Used to compute loss.</mat-hint>
        </mat-form-field>

        @if (loss() !== null) {
          <div class="loss-display" [class.warn]="loss()! < 0">
            <mat-icon>{{ loss()! < 0 ? 'warning' : 'arrow_forward' }}</mat-icon>
            <span>
              Loss: <strong>{{ loss() | number: '1.0-4' }}g</strong>
              @if (loss()! < 0) { <small>(negative — gain detected)</small> }
            </span>
          </div>
        }

        <mat-form-field appearance="outline" class="full">
          <mat-label>Actual Hours</mat-label>
          <input matInput type="number" min="0" step="0.25" formControlName="actualHours" />
          <mat-hint>Estimate was {{ data.stage.estimatedHours }}h.</mat-hint>
          @if (form.controls.actualHours.hasError('required')) {
            <mat-error>Actual hours is required</mat-error>
          }
          @if (form.controls.actualHours.hasError('min')) {
            <mat-error>Must be ≥ 0</mat-error>
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
          Complete Stage
        </button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .full { width: 100%; min-width: 380px; }
    .weight-in-banner, .loss-display {
      display: flex; gap: 8px; align-items: center;
      padding: 8px 12px; border-radius: 4px; font-size: 13px; margin-bottom: 12px;
    }
    .weight-in-banner { background: #e3f2fd; color: #0d47a1; }
    .loss-display { background: #fff3e0; color: #e65100; }
    .loss-display.warn { background: #ffebee; color: #c62828; }
    mat-dialog-content { padding-top: 8px !important; }
  `],
})
export class CompleteStageDialog {
  protected readonly data = inject<CompleteStageDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CompleteStageDialog, CompleteStageDialogResult | undefined>);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    weightOutGrams: [null as number | null, [Validators.min(0)]],
    actualHours: [this.data.stage.estimatedHours, [Validators.required, Validators.min(0)]],
    notes: [''],
  });

  // Live loss preview
  readonly loss = signal<number | null>(null);

  constructor() {
    this.form.controls.weightOutGrams.valueChanges.subscribe((wo) => {
      const wi = this.data.stage.weightInGrams;
      if (wi !== null && wo !== null && wo !== undefined) {
        this.loss.set(Number((wi - wo).toFixed(4)));
      } else {
        this.loss.set(null);
      }
    });
  }

  stagePretty(s: string): string { return STAGE_PRETTY[s] ?? s; }

  confirm(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.dialogRef.close({
      weightOutGrams: v.weightOutGrams,
      actualHours: v.actualHours,
      notes: v.notes?.trim() || null,
    });
  }
}
