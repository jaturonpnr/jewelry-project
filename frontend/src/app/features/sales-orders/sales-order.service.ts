import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedRequest, PagedResult } from '../../core/api/api-response';
import { ChangeSalesOrderStatusDto, SalesOrder } from './sales-order.types';

export interface SalesOrderQuery extends PagedRequest {
  status?: number;
  customerId?: string;
  fromDate?: string;
  toDate?: string;
}

@Injectable({ providedIn: 'root' })
export class SalesOrderService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/sales-orders`;

  getPaged(req: SalesOrderQuery = {}): Observable<PagedResult<SalesOrder>> {
    let params = new HttpParams();
    if (req.page) params = params.set('page', req.page);
    if (req.pageSize) params = params.set('pageSize', req.pageSize);
    if (req.sort) params = params.set('sort', req.sort);
    if (req.order) params = params.set('order', req.order);
    if (req.search) params = params.set('search', req.search);
    if (req.status !== undefined) params = params.set('status', req.status);
    if (req.customerId) params = params.set('customerId', req.customerId);
    if (req.fromDate) params = params.set('fromDate', req.fromDate);
    if (req.toDate) params = params.set('toDate', req.toDate);

    return this.http
      .get<ApiResponse<PagedResult<SalesOrder>>>(this.url, { params })
      .pipe(map((res) => res.data!));
  }

  getById(id: string): Observable<SalesOrder> {
    return this.http
      .get<ApiResponse<SalesOrder>>(`${this.url}/${id}`)
      .pipe(map((res) => res.data!));
  }

  changeStatus(id: string, dto: ChangeSalesOrderStatusDto): Observable<SalesOrder> {
    return this.http
      .post<ApiResponse<SalesOrder>>(`${this.url}/${id}/status`, dto)
      .pipe(map((res) => res.data!));
  }
}
