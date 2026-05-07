import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedRequest, PagedResult } from '../../core/api/api-response';
import { MaterialType } from './material-type.types';

@Injectable({ providedIn: 'root' })
export class MaterialTypeService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/material-types`;

  getPaged(req: PagedRequest = {}): Observable<PagedResult<MaterialType>> {
    let params = new HttpParams();
    if (req.page) params = params.set('page', req.page);
    if (req.pageSize) params = params.set('pageSize', req.pageSize);
    if (req.sort) params = params.set('sort', req.sort);
    if (req.order) params = params.set('order', req.order);
    if (req.search) params = params.set('search', req.search);

    return this.http
      .get<ApiResponse<PagedResult<MaterialType>>>(this.url, { params })
      .pipe(map((res) => res.data!));
  }

  /** Convenience: pull a large page of active types for select dropdowns. */
  getAllActive(): Observable<MaterialType[]> {
    return this.getPaged({ pageSize: 100 }).pipe(
      map((p) => p.items.filter((m) => m.isActive)),
    );
  }
}
