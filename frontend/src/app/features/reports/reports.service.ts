import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../core/api/api-response';
import {
  InventorySummaryReport,
  ProductionLoadReport,
  SalesOrderPipelineReport,
} from './reports.types';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/reports`;

  inventorySummary(): Observable<InventorySummaryReport> {
    return this.http
      .get<ApiResponse<InventorySummaryReport>>(`${this.url}/inventory-summary`)
      .pipe(map((res) => res.data!));
  }

  salesOrderPipeline(): Observable<SalesOrderPipelineReport> {
    return this.http
      .get<ApiResponse<SalesOrderPipelineReport>>(`${this.url}/sales-order-pipeline`)
      .pipe(map((res) => res.data!));
  }

  productionLoad(): Observable<ProductionLoadReport> {
    return this.http
      .get<ApiResponse<ProductionLoadReport>>(`${this.url}/production-load`)
      .pipe(map((res) => res.data!));
  }
}
