import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { PagedResult } from '../../core/api/api-response';
import { environment } from '../../../environments/environment';
import {
  BomTemplateSummary, BomTemplateResponse,
  CreateBomTemplateDto, UpdateBomTemplateDto,
} from './bom.types';

@Injectable({ providedIn: 'root' })
export class BomService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/bom-templates`;

  getPaged(page: number, pageSize: number, search?: string, isActive?: boolean):
    Observable<PagedResult<BomTemplateSummary>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    if (isActive !== undefined) params = params.set('isActive', isActive);
    return this.http.get<{ data: PagedResult<BomTemplateSummary> }>(this.base, { params })
      .pipe(map(r => r.data));
  }

  getById(id: string): Observable<BomTemplateResponse> {
    return this.http.get<{ data: BomTemplateResponse }>(`${this.base}/${id}`)
      .pipe(map(r => r.data));
  }

  create(dto: CreateBomTemplateDto): Observable<string> {
    return this.http.post<{ data: string }>(this.base, dto).pipe(map(r => r.data));
  }

  update(id: string, dto: UpdateBomTemplateDto): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
