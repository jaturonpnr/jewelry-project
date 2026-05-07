import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedRequest, PagedResult } from '../../core/api/api-response';
import { CreateCustomerDto, Customer, UpdateCustomerDto } from './customer.types';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/customers`;

  getPaged(req: PagedRequest = {}): Observable<PagedResult<Customer>> {
    let params = new HttpParams();
    if (req.page) params = params.set('page', req.page);
    if (req.pageSize) params = params.set('pageSize', req.pageSize);
    if (req.sort) params = params.set('sort', req.sort);
    if (req.order) params = params.set('order', req.order);
    if (req.search) params = params.set('search', req.search);

    return this.http
      .get<ApiResponse<PagedResult<Customer>>>(this.url, { params })
      .pipe(map((res) => res.data!));
  }

  getById(id: string): Observable<Customer> {
    return this.http
      .get<ApiResponse<Customer>>(`${this.url}/${id}`)
      .pipe(map((res) => res.data!));
  }

  create(dto: CreateCustomerDto): Observable<Customer> {
    return this.http
      .post<ApiResponse<Customer>>(this.url, dto)
      .pipe(map((res) => res.data!));
  }

  update(id: string, dto: UpdateCustomerDto): Observable<Customer> {
    return this.http
      .put<ApiResponse<Customer>>(`${this.url}/${id}`, dto)
      .pipe(map((res) => res.data!));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
