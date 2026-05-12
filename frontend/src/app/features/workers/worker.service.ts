import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedRequest, PagedResult } from '../../core/api/api-response';
import { Worker } from './worker.types';

@Injectable({ providedIn: 'root' })
export class WorkerService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/workers`;

  getPaged(req: PagedRequest = {}): Observable<PagedResult<Worker>> {
    let params = new HttpParams();
    if (req.page) params = params.set('page', req.page);
    if (req.pageSize) params = params.set('pageSize', req.pageSize);
    if (req.sort) params = params.set('sort', req.sort);
    if (req.order) params = params.set('order', req.order);
    if (req.search) params = params.set('search', req.search);

    return this.http
      .get<ApiResponse<PagedResult<Worker>>>(this.url, { params })
      .pipe(map((res) => res.data!));
  }

  /** Convenience: pull a large page of active workers for select dropdowns. */
  getAllActive(): Observable<Worker[]> {
    return this.getPaged({ pageSize: 100 }).pipe(
      map((p) => p.items.filter((w) => w.isActive)),
    );
  }
}
