import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../../../core/api/error-utils';
import { Currency } from '../../../customers/customer.types';
import { MaterialTypeService } from '../../../material-types/material-type.service';
import { MaterialType } from '../../../material-types/material-type.types';
import { Supplier } from '../../../suppliers/supplier.types';
import { SupplierService } from '../../../suppliers/supplier.service';
import { InventoryService } from '../../inventory.service';

@Component({
  selector: 'app-receive-raw-material',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <button mat-icon-button routerLink=".." aria-label="Back">
        <mat-icon>arrow_back</mat-icon>
      </button>
      <div>
        <h1>Receive Raw Material</h1>
        <p class="subtitle">Record an incoming lot of metal, findings, or consumables</p>
      </div>
    </div>

    @if (loadingMasters()) { <mat-progress-bar mode="indeterminate" /> }

    <form [formGroup]="form" (ngSubmit)="submit()">
      <mat-card class="section">
        <mat-card-header><mat-card-title>Material</mat-card-title></mat-card-header>
        <mat-card-content>
          <div class="grid">
            <mat-form-field appearance="outline" class="span-2">
              <mat-label>Material Type</mat-label>
              <mat-select formControlName="materialTypeId">
                @for (m of materialTypes(); track m.id) {
                  <mat-option [value]="m.id">
                    {{ m.code }} — {{ m.name }}
                    <small class="opt-meta">({{ m.category }} · {{ m.unit }})</small>
                  </mat-option>
                }
              </mat-select>
              @if (form.controls.materialTypeId.hasError('required')) {
                <mat-error>Material type is required</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Lot Number</mat-label>
              <input matInput formControlName="lotNumber" placeholder="LOT-2026-001" />
              @if (form.controls.lotNumber.hasError('required')) {
                <mat-error>Lot number is required</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Location</mat-label>
              <input matInput formControlName="location" placeholder="VAULT-A" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Quantity{{ unitLabel() }}</mat-label>
              <input matInput type="number" min="0.0001" step="0.0001" formControlName="quantity" />
              @if (form.controls.quantity.hasError('min')) {
                <mat-error>Must be greater than 0</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Received Date</mat-label>
              <input matInput type="date" formControlName="receivedDate" />
            </mat-form-field>
          </div>

          @if (selectedMaterial()?.category === 'Metal' && selectedMaterial()?.purityFraction) {
            <div class="info">
              <mat-icon>info</mat-icon>
              Pure weight will be auto-computed:
              <strong>quantity × {{ selectedMaterial()!.purityFraction! }}</strong>
              ({{ selectedMaterial()!.karat }}k gold)
            </div>
          }
        </mat-card-content>
      </mat-card>

      <mat-card class="section">
        <mat-card-header><mat-card-title>Cost & Supplier</mat-card-title></mat-card-header>
        <mat-card-content>
          <div class="grid">
            <mat-form-field appearance="outline">
              <mat-label>Unit Cost</mat-label>
              <input matInput type="number" min="0" step="0.01" formControlName="unitCost" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Currency</mat-label>
              <mat-select formControlName="costCurrency">
                @for (opt of currencyOptions; track opt.value) {
                  <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline" class="span-2">
              <mat-label>Supplier (optional)</mat-label>
              <mat-select formControlName="supplierId">
                <mat-option [value]="null">— None —</mat-option>
                @for (s of suppliers(); track s.id) {
                  <mat-option [value]="s.id">{{ s.code }} — {{ s.companyName }}</mat-option>
                }
              </mat-select>
            </mat-form-field>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="section">
        <mat-card-content>
          <mat-form-field appearance="outline" class="full">
            <mat-label>Notes</mat-label>
            <textarea matInput formControlName="notes" rows="2"></textarea>
          </mat-form-field>
        </mat-card-content>
      </mat-card>

      @if (errorMessage(); as msg) {
        <div class="error">
          <mat-icon>error_outline</mat-icon>
          <span>{{ msg }}</span>
        </div>
      }

      <div class="actions">
        <button mat-button type="button" routerLink=".." [disabled]="saving()">Cancel</button>
        <button mat-flat-button color="primary" type="submit" [disabled]="saving()">
          Receive Stock
        </button>
      </div>
    </form>
  `,
  styles: [`
    .page-header {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 16px;
    }
    .page-header h1 { margin: 0 0 4px; font-weight: 500; }
    .subtitle { margin: 0; color: #607d8b; font-size: 13px; }
    .section { margin-bottom: 16px; }
    .grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
      margin-top: 8px;
    }
    .span-2 { grid-column: span 2; }
    .full { width: 100%; }
    .opt-meta { color: #9e9e9e; font-size: 11px; margin-left: 4px; }
    .info {
      display: flex;
      gap: 8px;
      align-items: center;
      padding: 8px 12px;
      background: #e3f2fd;
      color: #0d47a1;
      border-radius: 4px;
      font-size: 13px;
      margin-top: 8px;
    }
    .info strong { font-family: monospace; }
    .error {
      display: flex;
      gap: 8px;
      align-items: center;
      padding: 12px;
      background: #ffebee;
      color: #c62828;
      border-radius: 4px;
      margin: 12px 0;
    }
    .actions {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      padding: 16px 0;
    }
  `],
})
export class ReceiveRawMaterial implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly inventoryService = inject(InventoryService);
  private readonly materialTypeService = inject(MaterialTypeService);
  private readonly supplierService = inject(SupplierService);
  private readonly snack = inject(MatSnackBar);

  readonly materialTypes = signal<MaterialType[]>([]);
  readonly suppliers = signal<Supplier[]>([]);
  readonly selectedMaterial = signal<MaterialType | null>(null);
  readonly loadingMasters = signal(true);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly currencyOptions = Object.entries(Currency).map(([k, v]) => ({ value: v, label: k }));

  readonly form = this.fb.nonNullable.group({
    materialTypeId: ['', [Validators.required]],
    lotNumber: ['', [Validators.required]],
    location: ['MAIN', [Validators.required]],
    quantity: [0, [Validators.required, Validators.min(0.0001)]],
    unitCost: [0, [Validators.min(0)]],
    costCurrency: [Currency.USD as number],
    supplierId: [null as string | null],
    receivedDate: [new Date().toISOString().slice(0, 10)],
    notes: [''],
  });

  ngOnInit(): void {
    this.materialTypeService.getAllActive().subscribe({
      next: (mats) => {
        this.materialTypes.set(mats);
        this.loadingMasters.set(this.suppliers().length === 0);
      },
      error: (err) => this.errorMessage.set(extractErrorMessage(err, 'Failed to load material types.')),
    });
    this.supplierService.getPaged({ pageSize: 100 }).subscribe({
      next: (paged) => {
        this.suppliers.set(paged.items.filter((s) => s.isActive));
        this.loadingMasters.set(false);
      },
      error: () => this.loadingMasters.set(false),
    });

    this.form.controls.materialTypeId.valueChanges.subscribe((id) => {
      this.selectedMaterial.set(this.materialTypes().find((m) => m.id === id) ?? null);
    });
  }

  unitLabel(): string {
    const u = this.selectedMaterial()?.unit;
    return u ? ` (${u.toLowerCase()})` : '';
  }

  submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.errorMessage.set(null);

    const v = this.form.getRawValue();
    this.inventoryService.receiveRawMaterial({
      materialTypeId: v.materialTypeId,
      lotNumber: v.lotNumber.trim().toUpperCase(),
      location: v.location.trim().toUpperCase(),
      quantity: v.quantity,
      unitCost: v.unitCost,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      costCurrency: v.costCurrency as any,
      supplierId: v.supplierId,
      receivedDate: new Date(v.receivedDate).toISOString(),
      notes: v.notes?.trim() || null,
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.snack.open('Stock received', 'Close', { duration: 3000 });
        this.router.navigate(['/inventory/raw-materials']);
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to receive stock.'));
      },
    });
  }
}
