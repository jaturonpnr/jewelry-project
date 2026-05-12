import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { PagedResult } from '../../core/api/api-response';
import { environment } from '../../../environments/environment';
import {
  CreateInvoiceDto, CreateShipmentDto,
  InvoiceResponse, InvoiceSummary,
  InvoiceStatusValue, ShipmentResponse, ShipmentSummary,
  ShipmentStatusValue, UpdateInvoiceStatusDto, UpdateShipmentStatusDto,
} from './shipping.types';

@Injectable({ providedIn: 'root' })
export class ShippingService {
  private readonly http = inject(HttpClient);
  private readonly baseShip = `${environment.apiBaseUrl}/shipments`;
  private readonly baseInv = `${environment.apiBaseUrl}/invoices`;

  // ── Shipments ─────────────────────────────────────────────────────────────

  getShipmentsPaged(page: number, pageSize: number, status?: ShipmentStatusValue, search?: string):
    Observable<PagedResult<ShipmentSummary>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (status != null) params = params.set('status', status);
    if (search) params = params.set('search', search);
    return this.http.get<{ data: PagedResult<ShipmentSummary> }>(this.baseShip, { params })
      .pipe(map(r => r.data));
  }

  getShipmentById(id: string): Observable<ShipmentResponse> {
    return this.http.get<{ data: ShipmentResponse }>(`${this.baseShip}/${id}`)
      .pipe(map(r => r.data));
  }

  createShipment(dto: CreateShipmentDto): Observable<string> {
    return this.http.post<{ data: string }>(this.baseShip, dto).pipe(map(r => r.data));
  }

  updateShipmentStatus(id: string, dto: UpdateShipmentStatusDto): Observable<void> {
    return this.http.patch<void>(`${this.baseShip}/${id}/status`, dto);
  }

  // ── Invoices ──────────────────────────────────────────────────────────────

  getInvoicesPaged(page: number, pageSize: number, status?: InvoiceStatusValue, search?: string):
    Observable<PagedResult<InvoiceSummary>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (status != null) params = params.set('status', status);
    if (search) params = params.set('search', search);
    return this.http.get<{ data: PagedResult<InvoiceSummary> }>(this.baseInv, { params })
      .pipe(map(r => r.data));
  }

  getInvoiceById(id: string): Observable<InvoiceResponse> {
    return this.http.get<{ data: InvoiceResponse }>(`${this.baseInv}/${id}`)
      .pipe(map(r => r.data));
  }

  createInvoice(dto: CreateInvoiceDto): Observable<string> {
    return this.http.post<{ data: string }>(this.baseInv, dto).pipe(map(r => r.data));
  }

  updateInvoiceStatus(id: string, dto: UpdateInvoiceStatusDto): Observable<void> {
    return this.http.patch<void>(`${this.baseInv}/${id}/status`, dto);
  }
}
