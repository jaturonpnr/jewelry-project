import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
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
import { MatSelectModule } from '@angular/material/select';
import { WorkerService } from '../../workers/worker.service';
import { Worker } from '../../workers/worker.types';
import { STAGE_PRETTY, WorkOrderStage } from '../work-order.types';

export interface StartStageDialogData {
  stage: WorkOrderStage;
}

export interface StartStageDialogResult {
  assignedWorkerId: string | null;
  weightInGrams: number | null;
}

@Component({
  selector: 'app-start-stage-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose,
    MatFormFieldModule, MatInputModule, MatSelectModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2 mat-dialog-title>Start "{{ stagePretty(data.stage.stage) }}"</h2>
    <form [formGroup]="form" (ngSubmit)="confirm()">
      <mat-dialog-content>
        <p class="hint">Stage will move to <strong>InProgress</strong> and timestamp Started At.</p>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Assigned Worker (optional)</mat-label>
          <mat-select formControlName="assignedWorkerId">
            <mat-option [value]="null">— Unassigned —</mat-option>
            @for (w of workers(); track w.id) {
              <mat-option [value]="w.id">{{ w.employeeCode }} — {{ w.fullName }} ({{ w.position }})</mat-option>
            }
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Weight In (grams)</mat-label>
          <input matInput type="number" min="0" step="0.0001" formControlName="weightInGrams" />
          <mat-hint>Capture the gross weight ENTERING this stage. Loss = WeightIn − WeightOut.</mat-hint>
        </mat-form-field>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-button mat-dialog-close type="button">Cancel</button>
        <button mat-flat-button color="primary" type="submit">Start Stage</button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .full { width: 100%; min-width: 360px; }
    .hint { color: #607d8b; font-size: 13px; margin: 0 0 12px; }
    mat-dialog-content { padding-top: 8px !important; }
  `],
})
export class StartStageDialog implements OnInit {
  protected readonly data = inject<StartStageDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<StartStageDialog, StartStageDialogResult | undefined>);
  private readonly fb = inject(FormBuilder);
  private readonly workerService = inject(WorkerService);

  readonly workers = signal<Worker[]>([]);

  readonly form = this.fb.nonNullable.group({
    assignedWorkerId: [this.data.stage.assignedWorkerId as string | null],
    weightInGrams: [null as number | null, [Validators.min(0)]],
  });

  ngOnInit(): void {
    this.workerService.getAllActive().subscribe((ws) => this.workers.set(ws));
  }

  stagePretty(s: string): string { return STAGE_PRETTY[s] ?? s; }

  confirm(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.dialogRef.close({
      assignedWorkerId: v.assignedWorkerId,
      weightInGrams: v.weightInGrams,
    });
  }
}
