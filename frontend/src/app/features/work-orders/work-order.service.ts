import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedRequest, PagedResult } from '../../core/api/api-response';
import {
  ChangeWorkOrderStatusDto,
  CompleteStageDto,
  CreateWorkOrderDto,
  FailStageDto,
  SkipStageDto,
  StartStageDto,
  WorkOrder,
  WorkOrderStage,
} from './work-order.types';

export interface WorkOrderQuery extends PagedRequest {
  status?: number;
  priority?: number;
  salesOrderId?: string;
  assignedSupervisorId?: string;
}

@Injectable({ providedIn: 'root' })
export class WorkOrderService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/work-orders`;

  getPaged(req: WorkOrderQuery = {}): Observable<PagedResult<WorkOrder>> {
    let params = new HttpParams();
    if (req.page) params = params.set('page', req.page);
    if (req.pageSize) params = params.set('pageSize', req.pageSize);
    if (req.sort) params = params.set('sort', req.sort);
    if (req.order) params = params.set('order', req.order);
    if (req.search) params = params.set('search', req.search);
    if (req.status !== undefined) params = params.set('status', req.status);
    if (req.priority !== undefined) params = params.set('priority', req.priority);
    if (req.salesOrderId) params = params.set('salesOrderId', req.salesOrderId);
    if (req.assignedSupervisorId) params = params.set('assignedSupervisorId', req.assignedSupervisorId);

    return this.http
      .get<ApiResponse<PagedResult<WorkOrder>>>(this.url, { params })
      .pipe(map((res) => res.data!));
  }

  getById(id: string): Observable<WorkOrder> {
    return this.http
      .get<ApiResponse<WorkOrder>>(`${this.url}/${id}`)
      .pipe(map((res) => res.data!));
  }

  create(dto: CreateWorkOrderDto): Observable<WorkOrder> {
    return this.http
      .post<ApiResponse<WorkOrder>>(this.url, dto)
      .pipe(map((res) => res.data!));
  }

  changeStatus(id: string, dto: ChangeWorkOrderStatusDto): Observable<WorkOrder> {
    return this.http
      .post<ApiResponse<WorkOrder>>(`${this.url}/${id}/status`, dto)
      .pipe(map((res) => res.data!));
  }

  startStage(woId: string, stageId: string, dto: StartStageDto): Observable<WorkOrderStage> {
    return this.http
      .post<ApiResponse<WorkOrderStage>>(`${this.url}/${woId}/stages/${stageId}/start`, dto)
      .pipe(map((res) => res.data!));
  }

  completeStage(woId: string, stageId: string, dto: CompleteStageDto): Observable<WorkOrderStage> {
    return this.http
      .post<ApiResponse<WorkOrderStage>>(`${this.url}/${woId}/stages/${stageId}/complete`, dto)
      .pipe(map((res) => res.data!));
  }

  skipStage(woId: string, stageId: string, dto: SkipStageDto): Observable<WorkOrderStage> {
    return this.http
      .post<ApiResponse<WorkOrderStage>>(`${this.url}/${woId}/stages/${stageId}/skip`, dto)
      .pipe(map((res) => res.data!));
  }

  failStage(woId: string, stageId: string, dto: FailStageDto): Observable<WorkOrderStage> {
    return this.http
      .post<ApiResponse<WorkOrderStage>>(`${this.url}/${woId}/stages/${stageId}/fail`, dto)
      .pipe(map((res) => res.data!));
  }
}
