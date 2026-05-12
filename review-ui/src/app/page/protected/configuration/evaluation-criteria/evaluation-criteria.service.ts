import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CriteriaSetDto {
  id: number;
  name: string;
  applicableYears: string;
  applicableMonths: string;
  isActive: number;
  objectCodes: string[];
}

export interface CreateCriteriaSetRequest {
  name: string;
  objectType: string;
  applicableYears: string;
  applicableMonths: string;
}

export interface UpdateCriteriaSetRequest {
  name: string;
  objectType: string;
  applicableYears: string;
  applicableMonths: string;
  isActive: number;
}

export interface CreateCriteriaRequest {
  criteriaSetId: number;
  virtualCode: string;
  displayCode: string;
  content: string;
  maxScore: number;
  scoreType: string;
  virtualParentCode: string;
}

export interface CreateClassificationRequest {
  criteriaSetId: number;
  code: string;
  virtualId: string;
  name: string;
  abbreviation: string;
  minScore: number;
  maxScore: number;
}

export interface CreateCriteriaSetDetailRequest {
  name: string;
  applicableYears: string;
  applicableMonths: string;
  criteria: CreateCriteriaRequest[];
  classifications: CreateClassificationRequest[];
}

export interface CriteriaSetDetailDto {
  id: number;
  name: string;
  applicableYears: string;
  applicableMonths: string;
  isActive: number;
  criteria: CriteriaDto[];
  classifications: ClassificationDto[];
}

export interface ClassificationDto {
  id: number;
  criteriaSetId: number;
  code: string;
  virtualId: string;
  name: string;
  abbreviation: string;
  minScore: number;
  maxScore: number;
  isActive: number;
}

export interface CriteriaDto {
  criteriaSetId: number;
  virtualCode: string;
  displayCode: string;
  content: string;
  maxScore: number;
  scoreType: string;
  virtualParentCode: string;
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

export interface DeleteManyCriteriaSetRequest {
  isAll: boolean;
  includeIds?: number[];
  excludeIds?: number[];
  filter?: { search?: string };
}

@Injectable({
  providedIn: 'root'
})
export class EvaluationCriteriaService {
  private apiUrl = '/api/page/configuration/evaluation-criteria';

  constructor(private http: HttpClient) { }

  getAll(search?: string, page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<CriteriaSetDto>> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }
    params = params.set('page', page.toString());
    params = params.set('pageSize', pageSize.toString());
    
    return this.http.get<PaginatedResponse<CriteriaSetDto>>(this.apiUrl, { params });
  }

  getById(id: number): Observable<CriteriaSetDto> {
    return this.http.get<CriteriaSetDto>(`${this.apiUrl}/${id}`);
  }

  getDetail(id: number): Observable<CriteriaSetDetailDto> {
    return this.http.get<CriteriaSetDetailDto>(`${this.apiUrl}/${id}/detail`);
  }

  create(request: CreateCriteriaSetRequest): Observable<CriteriaSetDto> {
    return this.http.post<CriteriaSetDto>(this.apiUrl, request);
  }

  update(id: number, request: UpdateCriteriaSetRequest): Observable<CriteriaSetDto> {
    return this.http.put<CriteriaSetDto>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  deleteMany(request: DeleteManyCriteriaSetRequest): Observable<any> {
    return this.http.delete(this.apiUrl, { body: request });
  }

  createCriteria(request: CreateCriteriaRequest): Observable<CriteriaDto> {
    return this.http.post<CriteriaDto>(`${this.apiUrl}/criteria`, request);
  }

  createCriteriaSetDetail(request: CreateCriteriaSetDetailRequest): Observable<CriteriaSetDto> {
    return this.http.post<CriteriaSetDto>(`${this.apiUrl}/batch`, request);
  }
}
