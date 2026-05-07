import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedRequest, PagedResult } from '../../core/api/api-response';
import { Supplier } from './supplier.types';

@Injectable({ providedIn: 'root' })
export class SupplierService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/suppliers`;

  getPaged(req: PagedRequest = {}): Observable<PagedResult<Supplier>> {
    let params = new HttpParams();
    if (req.page) params = params.set('page', req.page);
    if (req.pageSize) params = params.set('pageSize', req.pageSize);
    if (req.sort) params = params.set('sort', req.sort);
    if (req.order) params = params.set('order', req.order);
    if (req.search) params = params.set('search', req.search);

    return this.http
      .get<ApiResponse<PagedResult<Supplier>>>(this.url, { params })
      .pipe(map((res) => res.data!));
  }

  getById(id: string): Observable<Supplier> {
    return this.http
      .get<ApiResponse<Supplier>>(`${this.url}/${id}`)
      .pipe(map((res) => res.data!));
  }
}
