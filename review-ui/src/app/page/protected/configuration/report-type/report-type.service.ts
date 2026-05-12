import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ReportTypeDto {
  code: string;
  name: string;
  applicableYears: string;
  applicableMonths: string;
  criteria: string;
  isActive: number;
}

export interface CreateReportTypeRequest {
  code: string;
  name: string;
  applicableYears: string;
  applicableMonths: string;
  criteria: string;
}

export interface UpdateReportTypeRequest {
  name: string;
  applicableYears: string;
  applicableMonths: string;
  criteria: string;
  isActive: number;
}

export interface PaginationInfo {
  currentPage: number;
  totalPages: number;
  totalItems: number;
  itemsPerPage: number;
}

export interface PaginatedResponse<T> {
  data: T[];
  pagination: PaginationInfo;
}

export interface DeleteManyReportTypeRequest {
  isAll: boolean;
  includeIds?: string[];
  excludeIds?: string[];
  filter?: { search?: string };
}

@Injectable({
  providedIn: 'root'
})
export class ReportTypeService {
  private apiUrl = '/api/page/configuration/report-type';

  constructor(private http: HttpClient) { }

  getAll(search?: string, page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<ReportTypeDto>> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }
    params = params.set('page', page.toString());
    params = params.set('pageSize', pageSize.toString());
    
    return this.http.get<PaginatedResponse<ReportTypeDto>>(this.apiUrl, { params });
  }

  getByCode(code: string): Observable<ReportTypeDto> {
    return this.http.get<ReportTypeDto>(`${this.apiUrl}/${code}`);
  }

  create(request: CreateReportTypeRequest): Observable<ReportTypeDto> {
    return this.http.post<ReportTypeDto>(this.apiUrl, request);
  }

  update(code: string, request: UpdateReportTypeRequest): Observable<ReportTypeDto> {
    return this.http.put<ReportTypeDto>(`${this.apiUrl}/${code}`, request);
  }

  delete(code: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${code}`);
  }

  deleteMany(request: DeleteManyReportTypeRequest): Observable<any> {
    return this.http.delete(this.apiUrl, { body: request });
  }
}
