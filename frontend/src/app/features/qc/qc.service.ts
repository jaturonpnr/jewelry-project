import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { PagedResult } from '../../core/api/api-response';
import { environment } from '../../../environments/environment';
import {
  CreateQcInspectionDto,
  QcInspectionResponse,
  QcInspectionSummary,
  QcInspectionTypeValue,
  QcResultValue,
} from './qc.types';

@Injectable({ providedIn: 'root' })
export class QcService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/qc-inspections`;

  getPaged(
    page: number, pageSize: number,
    inspectionType?: QcInspectionTypeValue,
    result?: QcResultValue,
    workOrderId?: string,
    fromDate?: string,
    toDate?: string,
  ): Observable<PagedResult<QcInspectionSummary>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (inspectionType != null) params = params.set('inspectionType', inspectionType);
    if (result != null) params = params.set('result', result);
    if (workOrderId) params = params.set('workOrderId', workOrderId);
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<{ data: PagedResult<QcInspectionSummary> }>(this.base, { params })
      .pipe(map(r => r.data));
  }

  getById(id: string): Observable<QcInspectionResponse> {
    return this.http.get<{ data: QcInspectionResponse }>(`${this.base}/${id}`)
      .pipe(map(r => r.data));
  }

  create(dto: CreateQcInspectionDto): Observable<string> {
    return this.http.post<{ data: string }>(this.base, dto).pipe(map(r => r.data));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
