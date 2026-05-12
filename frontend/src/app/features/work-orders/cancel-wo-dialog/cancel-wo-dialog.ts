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

export interface CancelWoDialogData { workOrderNumber: string; }

@Component({
  selector: 'app-cancel-wo-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose,
    MatFormFieldModule, MatInputModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2 mat-dialog-title>Cancel work order {{ data.workOrderNumber }}</h2>
    <form [formGroup]="form" (ngSubmit)="confirm()">
      <mat-dialog-content>
        <p class="warning">
          The work order will be set to <strong>Cancelled</strong>. In-progress
          stages remain in their current state. The action cannot be undone.
        </p>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Reason</mat-label>
          <textarea matInput formControlName="reason" rows="3" cdkFocusInitial></textarea>
          @if (form.controls.reason.hasError('required') && form.controls.reason.touched) {
            <mat-error>Cancellation reason is required</mat-error>
          }
        </mat-form-field>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-button mat-dialog-close type="button">Keep order</button>
        <button mat-flat-button color="warn" type="submit" [disabled]="form.invalid">Cancel work order</button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .full { width: 100%; min-width: 360px; }
    .warning { color: #c62828; font-size: 13px; margin: 4px 0 16px; }
  `],
})
export class CancelWoDialog {
  protected readonly data = inject<CancelWoDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CancelWoDialog, string | undefined>);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    reason: ['', [Validators.required, Validators.maxLength(500)]],
  });

  confirm(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.dialogRef.close(this.form.controls.reason.value.trim());
  }
}
