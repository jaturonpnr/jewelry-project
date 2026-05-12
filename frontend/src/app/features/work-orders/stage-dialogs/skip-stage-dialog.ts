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

export interface SkipStageDialogData { stage: WorkOrderStage; }
export type SkipStageDialogResult = string; // reason

@Component({
  selector: 'app-skip-stage-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose,
    MatFormFieldModule, MatInputModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2 mat-dialog-title>Skip "{{ stagePretty(data.stage.stage) }}"</h2>
    <form [formGroup]="form" (ngSubmit)="confirm()">
      <mat-dialog-content>
        <p class="hint">
          Skipped stages don't block work order completion. Use this for stages
          that don't apply to this WO (e.g. no plating needed).
        </p>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Reason</mat-label>
          <textarea matInput formControlName="reason" rows="2" placeholder="e.g. Customer requested no plating" cdkFocusInitial></textarea>
          @if (form.controls.reason.hasError('required') && form.controls.reason.touched) {
            <mat-error>Reason is required</mat-error>
          }
        </mat-form-field>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-button mat-dialog-close type="button">Cancel</button>
        <button mat-flat-button color="accent" type="submit" [disabled]="form.invalid">Skip Stage</button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .full { width: 100%; min-width: 380px; }
    .hint { color: #607d8b; font-size: 13px; margin: 0 0 12px; }
  `],
})
export class SkipStageDialog {
  protected readonly data = inject<SkipStageDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<SkipStageDialog, SkipStageDialogResult | undefined>);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    reason: ['', [Validators.required, Validators.maxLength(200)]],
  });

  stagePretty(s: string): string { return STAGE_PRETTY[s] ?? s; }

  confirm(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.dialogRef.close(this.form.controls.reason.value.trim());
  }
}
