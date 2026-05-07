import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../../core/api/error-utils';
import { CustomerService } from '../customer.service';
import {
  CreateCustomerDto,
  Currency,
  CustomerType,
  PaymentTerms,
  UpdateCustomerDto,
} from '../customer.types';

@Component({
  selector: 'app-customer-form',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatProgressBarModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <button mat-icon-button routerLink="/customers" aria-label="Back">
        <mat-icon>arrow_back</mat-icon>
      </button>
      <div>
        <h1>{{ isEdit() ? 'Edit Customer' : 'New Customer' }}</h1>
        <p class="subtitle">{{ isEdit() ? 'Update customer details' : 'Create a new B2B customer record' }}</p>
      </div>
    </div>

    @if (loading()) {
      <mat-progress-bar mode="indeterminate" />
    }

    <form [formGroup]="form" (ngSubmit)="submit()">
      <mat-card class="section">
        <mat-card-header>
          <mat-card-title>Basic Info</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="grid">
            <mat-form-field appearance="outline">
              <mat-label>Code (e.g. CUST-0001)</mat-label>
              <input matInput formControlName="code" [readonly]="isEdit()" />
              @if (form.controls.code.hasError('required')) {
                <mat-error>Code is required</mat-error>
              }
              @if (form.controls.code.hasError('pattern')) {
                <mat-error>Use A–Z, 0–9, hyphen or underscore only</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline" class="span-2">
              <mat-label>Company Name</mat-label>
              <input matInput formControlName="companyName" />
              @if (form.controls.companyName.hasError('required')) {
                <mat-error>Company name is required</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Contact Person</mat-label>
              <input matInput formControlName="contactPerson" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Email</mat-label>
              <input matInput type="email" formControlName="email" />
              @if (form.controls.email.hasError('email')) {
                <mat-error>Invalid email format</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Phone</mat-label>
              <input matInput formControlName="phone" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Tax ID</mat-label>
              <input matInput formControlName="taxId" />
            </mat-form-field>

            @if (isEdit()) {
              <mat-checkbox formControlName="isActive" class="span-3">Active</mat-checkbox>
            }
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="section">
        <mat-card-header>
          <mat-card-title>Commercial</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="grid">
            <mat-form-field appearance="outline">
              <mat-label>Type</mat-label>
              <mat-select formControlName="type">
                @for (opt of typeOptions; track opt.value) {
                  <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Currency</mat-label>
              <mat-select formControlName="defaultCurrency">
                @for (opt of currencyOptions; track opt.value) {
                  <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Payment Terms</mat-label>
              <mat-select formControlName="paymentTerms">
                @for (opt of paymentOptions; track opt.value) {
                  <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Credit Limit</mat-label>
              <input matInput type="number" min="0" step="0.01" formControlName="creditLimit" />
              @if (form.controls.creditLimit.hasError('min')) {
                <mat-error>Must be ≥ 0</mat-error>
              }
            </mat-form-field>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="section" formGroupName="address">
        <mat-card-header>
          <mat-card-title>Address</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="grid">
            <mat-form-field appearance="outline" class="span-3">
              <mat-label>Address Line 1</mat-label>
              <input matInput formControlName="line1" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="span-3">
              <mat-label>Address Line 2</mat-label>
              <input matInput formControlName="line2" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>City</mat-label>
              <input matInput formControlName="city" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>State / Province</mat-label>
              <input matInput formControlName="state" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Postal Code</mat-label>
              <input matInput formControlName="postalCode" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="span-3">
              <mat-label>Country</mat-label>
              <input matInput formControlName="country" />
            </mat-form-field>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="section">
        <mat-card-content>
          <mat-form-field appearance="outline" class="full">
            <mat-label>Notes</mat-label>
            <textarea matInput formControlName="notes" rows="3"></textarea>
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
        <button mat-button type="button" routerLink="/customers" [disabled]="saving()">Cancel</button>
        <button mat-flat-button color="primary" type="submit" [disabled]="saving()">
          {{ isEdit() ? 'Save Changes' : 'Create Customer' }}
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
      grid-template-columns: repeat(3, 1fr);
      gap: 16px;
      margin-top: 8px;
    }
    .span-2 { grid-column: span 2; }
    .span-3 { grid-column: span 3; }
    .full { width: 100%; }
    .actions {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      padding: 16px 0;
    }
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
    @media (max-width: 800px) {
      .grid { grid-template-columns: 1fr; }
      .span-2, .span-3 { grid-column: auto; }
    }
  `],
})
export class CustomerForm {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(CustomerService);
  private readonly snack = inject(MatSnackBar);

  readonly id = signal<string | null>(this.route.snapshot.paramMap.get('id'));
  readonly isEdit = computed(() => this.id() !== null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly typeOptions = [
    { value: CustomerType.Wholesaler, label: 'Wholesaler' },
    { value: CustomerType.Retailer, label: 'Retailer' },
    { value: CustomerType.CataloguePublisher, label: 'Catalogue Publisher' },
    { value: CustomerType.Other, label: 'Other' },
  ];
  readonly currencyOptions = Object.entries(Currency).map(([k, v]) => ({ value: v, label: k }));
  readonly paymentOptions = [
    { value: PaymentTerms.Prepaid, label: 'Prepaid' },
    { value: PaymentTerms.Net15, label: 'Net 15' },
    { value: PaymentTerms.Net30, label: 'Net 30' },
    { value: PaymentTerms.Net45, label: 'Net 45' },
    { value: PaymentTerms.Net60, label: 'Net 60' },
    { value: PaymentTerms.Net90, label: 'Net 90' },
    { value: PaymentTerms.CashOnDelivery, label: 'Cash on Delivery' },
    { value: PaymentTerms.LetterOfCredit, label: 'Letter of Credit' },
  ];

  readonly form = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.pattern(/^[A-Z0-9_-]+$/i)]],
    companyName: ['', [Validators.required]],
    contactPerson: [''],
    email: ['', [Validators.email]],
    phone: [''],
    taxId: [''],
    type: [CustomerType.Wholesaler as number],
    defaultCurrency: [Currency.USD as number],
    paymentTerms: [PaymentTerms.Net30 as number],
    creditLimit: [0, [Validators.min(0)]],
    isActive: [true],
    address: this.fb.nonNullable.group({
      line1: [''],
      line2: [''],
      city: [''],
      state: [''],
      postalCode: [''],
      country: [''],
    }),
    notes: [''],
  });

  constructor() {
    if (this.isEdit()) {
      this.loadExisting();
    }
  }

  private loadExisting(): void {
    this.loading.set(true);
    this.service.getById(this.id()!).subscribe({
      next: (c) => {
        this.form.patchValue({
          code: c.code,
          companyName: c.companyName,
          contactPerson: c.contactPerson ?? '',
          email: c.email ?? '',
          phone: c.phone ?? '',
          taxId: c.taxId ?? '',
          type: enumValueFromName(CustomerType, c.type) ?? CustomerType.Wholesaler,
          defaultCurrency: enumValueFromName(Currency, c.defaultCurrency) ?? Currency.USD,
          paymentTerms: enumValueFromName(PaymentTerms, c.paymentTerms) ?? PaymentTerms.Net30,
          creditLimit: c.creditLimit,
          isActive: c.isActive,
          address: {
            line1: c.address.line1 ?? '',
            line2: c.address.line2 ?? '',
            city: c.address.city ?? '',
            state: c.address.state ?? '',
            postalCode: c.address.postalCode ?? '',
            country: c.address.country ?? '',
          },
          notes: c.notes ?? '',
        });
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to load customer.'));
      },
    });
  }

  submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.errorMessage.set(null);

    const v = this.form.getRawValue();
    const payload = {
      code: v.code.trim().toUpperCase(),
      companyName: v.companyName.trim(),
      contactPerson: v.contactPerson?.trim() || null,
      email: v.email?.trim() || null,
      phone: v.phone?.trim() || null,
      taxId: v.taxId?.trim() || null,
      type: v.type as 1 | 2 | 3 | 99,
      defaultCurrency: v.defaultCurrency as 1,
      paymentTerms: v.paymentTerms as 1,
      creditLimit: v.creditLimit ?? 0,
      address: v.address,
      notes: v.notes?.trim() || null,
    };

    const obs = this.isEdit()
      ? this.service.update(this.id()!, { ...payload, isActive: v.isActive } as UpdateCustomerDto)
      : this.service.create(payload as CreateCustomerDto);

    obs.subscribe({
      next: (saved) => {
        this.saving.set(false);
        this.snack.open(`Customer ${this.isEdit() ? 'updated' : 'created'}`, 'Close', { duration: 3000 });
        this.router.navigate(['/customers', saved.id]);
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to save customer.'));
      },
    });
  }
}

function enumValueFromName<T extends Record<string, number>>(e: T, name: string): T[keyof T] | undefined {
  const k = Object.keys(e).find((k) => k === name);
  return k ? (e[k] as T[keyof T]) : undefined;
}
