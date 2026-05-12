import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
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
import { MatInputModule } from '@angular/material/input';
import { STAGE_PRETTY, WorkOrderStage } from '../work-order.types';

export interface FailStageDialogData { stage: WorkOrderStage; }
export interface FailStageDialogResult {
  failureReason: string;
  weightOutGrams: number | null;
}

@Component({
  selector: 'app-fail-stage-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose,
    MatFormFieldModule, MatInputModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2 mat-dialog-title>Mark "{{ stagePretty(data.stage.stage) }}" as Failed</h2>
    <form [formGroup]="form" (ngSubmit)="confirm()">
      <mat-dialog-content>
        <p class="warning">
          Use this when QC rejects the work and the piece needs rework or
          rejection. Other stages remain Pending.
        </p>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Failure Reason</mat-label>
          <textarea matInput formControlName="failureReason" rows="3" placeholder="e.g. Stone not seated correctly" cdkFocusInitial></textarea>
          @if (form.controls.failureReason.hasError('required') && form.controls.failureReason.touched) {
            <mat-error>Failure reason is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Weight Out (grams) — optional</mat-label>
          <input matInput type="number" min="0" step="0.0001" formControlName="weightOutGrams" />
          <mat-hint>Recover and weigh the piece if possible.</mat-hint>
        </mat-form-field>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-button mat-dialog-close type="button">Cancel</button>
        <button mat-flat-button color="warn" type="submit" [disabled]="form.invalid">Mark Failed</button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .full { width: 100%; min-width: 380px; }
    .warning { color: #c62828; font-size: 13px; margin: 0 0 12px; }
  `],
})
export class FailStageDialog {
  protected readonly data = inject<FailStageDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<FailStageDialog, FailStageDialogResult | undefined>);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    failureReason: ['', [Validators.required, Validators.maxLength(500)]],
    weightOutGrams: [null as number | null, [Validators.min(0)]],
  });

  stagePretty(s: string): string { return STAGE_PRETTY[s] ?? s; }

  confirm(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.getRawValue();
    this.dialogRef.close({
      failureReason: v.failureReason.trim(),
      weightOutGrams: v.weightOutGrams,
    });
  }
}
