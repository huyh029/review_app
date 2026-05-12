import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ReportRowDto {
  stt: number;
  canBo: string;
  donVi: string;
  selfScore: number;
  managerScore: number | null;
  selfClassification: string;
  managerClassification: string;
  month: number;
  year: number;
}

export interface ReportTypeOptionDto {
  code: string;
  name: string;
  applicableMonths: string;
  applicableYears: string;
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

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private apiUrl = '/api/page/reports';

  constructor(private http: HttpClient) {}

  getReport(
    reportTypeCode?: string,
    month?: number,
    year?: number,
    page: number = 1,
    pageSize: number = 10
  ): Observable<PaginatedResponse<ReportRowDto>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (reportTypeCode) params = params.set('reportTypeCode', reportTypeCode);
    if (month) params = params.set('month', month.toString());
    if (year) params = params.set('year', year.toString());
    return this.http.get<PaginatedResponse<ReportRowDto>>(this.apiUrl, { params });
  }

  getReportTypeOptions(): Observable<ReportTypeOptionDto[]> {
    return this.http.get<ReportTypeOptionDto[]>(`${this.apiUrl}/report-type-options`);
  }
}
