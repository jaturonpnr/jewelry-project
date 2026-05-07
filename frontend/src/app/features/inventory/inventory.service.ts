import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedRequest, PagedResult } from '../../core/api/api-response';
import {
  AdjustRawMaterialDto,
  RawMaterialItem,
  ReceiveRawMaterialDto,
  StockMovement,
  StoneItem,
  StoneParcel,
} from './inventory.types';

export interface MovementQuery extends PagedRequest {
  itemType?: number;
  itemId?: string;
  movementType?: number;
  fromDate?: string;
  toDate?: string;
}

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory`;

  // ── Raw Material ──────────────────────────────────────

  getRawMaterials(req: PagedRequest = {}): Observable<PagedResult<RawMaterialItem>> {
    return this.http
      .get<ApiResponse<PagedResult<RawMaterialItem>>>(`${this.base}/raw-materials`, {
        params: this.toParams(req),
      })
      .pipe(map((res) => res.data!));
  }

  getRawMaterial(id: string): Observable<RawMaterialItem> {
    return this.http
      .get<ApiResponse<RawMaterialItem>>(`${this.base}/raw-materials/${id}`)
      .pipe(map((res) => res.data!));
  }

  receiveRawMaterial(dto: ReceiveRawMaterialDto): Observable<RawMaterialItem> {
    return this.http
      .post<ApiResponse<RawMaterialItem>>(`${this.base}/raw-materials/receive`, dto)
      .pipe(map((res) => res.data!));
  }

  adjustRawMaterial(id: string, dto: AdjustRawMaterialDto): Observable<RawMaterialItem> {
    return this.http
      .post<ApiResponse<RawMaterialItem>>(`${this.base}/raw-materials/${id}/adjust`, dto)
      .pipe(map((res) => res.data!));
  }

  // ── Stone Item ────────────────────────────────────────

  getStoneItems(req: PagedRequest = {}): Observable<PagedResult<StoneItem>> {
    return this.http
      .get<ApiResponse<PagedResult<StoneItem>>>(`${this.base}/stone-items`, {
        params: this.toParams(req),
      })
      .pipe(map((res) => res.data!));
  }

  // ── Stone Parcel ──────────────────────────────────────

  getStoneParcels(req: PagedRequest = {}): Observable<PagedResult<StoneParcel>> {
    return this.http
      .get<ApiResponse<PagedResult<StoneParcel>>>(`${this.base}/stone-parcels`, {
        params: this.toParams(req),
      })
      .pipe(map((res) => res.data!));
  }

  // ── Stock Movement ───────────────────────────────────

  getMovements(req: MovementQuery = {}): Observable<PagedResult<StockMovement>> {
    let params = this.toParams(req);
    if (req.itemType !== undefined) params = params.set('itemType', req.itemType);
    if (req.itemId) params = params.set('itemId', req.itemId);
    if (req.movementType !== undefined) params = params.set('movementType', req.movementType);
    if (req.fromDate) params = params.set('fromDate', req.fromDate);
    if (req.toDate) params = params.set('toDate', req.toDate);

    return this.http
      .get<ApiResponse<PagedResult<StockMovement>>>(`${this.base}/movements`, { params })
      .pipe(map((res) => res.data!));
  }

  private toParams(req: PagedRequest): HttpParams {
    let params = new HttpParams();
    if (req.page) params = params.set('page', req.page);
    if (req.pageSize) params = params.set('pageSize', req.pageSize);
    if (req.sort) params = params.set('sort', req.sort);
    if (req.order) params = params.set('order', req.order);
    if (req.search) params = params.set('search', req.search);
    return params;
  }
}
