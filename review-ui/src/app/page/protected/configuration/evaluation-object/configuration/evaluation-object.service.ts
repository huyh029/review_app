import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EvaluationObjectDto {
  code: string;
  name: string;
  isActive: number;
}

export interface CreateEvaluationObjectRequest {
  code: string;
  name: string;
}

export interface UpdateEvaluationObjectRequest {
  name: string;
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

export interface DeleteManyEvaluationObjectRequest {
  isAll: boolean;
  includeIds?: string[];
  excludeIds?: string[];
  filter?: { search?: string };
}

@Injectable({
  providedIn: 'root'
})
export class EvaluationObjectService {
  private apiUrl = '/api/page/configuration/evaluation-object';

  constructor(private http: HttpClient) { }

  getAll(search?: string, page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<EvaluationObjectDto>> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }
    params = params.set('page', page.toString());
    params = params.set('pageSize', pageSize.toString());
    
    return this.http.get<PaginatedResponse<EvaluationObjectDto>>(this.apiUrl, { params });
  }

  getByCode(code: string): Observable<EvaluationObjectDto> {
    return this.http.get<EvaluationObjectDto>(`${this.apiUrl}/${code}`);
  }

  create(request: CreateEvaluationObjectRequest): Observable<EvaluationObjectDto> {
    return this.http.post<EvaluationObjectDto>(this.apiUrl, request);
  }

  update(code: string, request: UpdateEvaluationObjectRequest): Observable<EvaluationObjectDto> {
    return this.http.put<EvaluationObjectDto>(`${this.apiUrl}/${code}`, request);
  }

  delete(code: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${code}`);
  }

  deleteMany(request: DeleteManyEvaluationObjectRequest): Observable<any> {
    return this.http.delete(this.apiUrl, { body: request });
  }
}
