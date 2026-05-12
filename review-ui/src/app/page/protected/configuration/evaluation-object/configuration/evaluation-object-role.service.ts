import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EvaluationObjectRoleDto {
  id: string;
  evaluationObjectCode: string;
  userId: string;
  userName: string;
}

export interface CreateEvaluationObjectRoleRequest {
  userId: string;
}

export interface UpdateEvaluationObjectRoleRequest {
  userId: string;
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

export interface TreeNodeDto {
  id: string;
  code: string;
  name: string;
  children: TreeNodeDto[];
  individuals: IndividualDto[];
}

export interface IndividualDto {
  id: string;
  name: string;
  code: string;
  selectedObjectIds: string[];
}

export interface EvaluationObjectHeaderDto {
  code: string;
  name: string;
}

export interface EvaluationObjectRoleTreeResponse {
  data: TreeNodeDto;
  headers: EvaluationObjectHeaderDto[];
}

@Injectable({
  providedIn: 'root'
})
export class EvaluationObjectRoleService {
  private apiUrl = '/api/page/configuration/evaluation-object';

  constructor(private http: HttpClient) { }

  getAll(evaluationObjectCode: string, search?: string, page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<EvaluationObjectRoleDto>> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }
    params = params.set('page', page.toString());
    params = params.set('pageSize', pageSize.toString());
    
    // If no code, call the all roles endpoint
    if (!evaluationObjectCode) {
      return this.http.get<PaginatedResponse<EvaluationObjectRoleDto>>(
        '/api/page/configuration/evaluation-object-role', 
        { params }
      );
    }
    
    return this.http.get<PaginatedResponse<EvaluationObjectRoleDto>>(
      `${this.apiUrl}/${evaluationObjectCode}/role`, 
      { params }
    );
  }

  getById(evaluationObjectCode: string, id: string): Observable<EvaluationObjectRoleDto> {
    return this.http.get<EvaluationObjectRoleDto>(
      `${this.apiUrl}/${evaluationObjectCode}/role/${id}`
    );
  }

  create(evaluationObjectCode: string, request: CreateEvaluationObjectRoleRequest): Observable<EvaluationObjectRoleDto> {
    return this.http.post<EvaluationObjectRoleDto>(
      `${this.apiUrl}/${evaluationObjectCode}/role`, 
      request
    );
  }

  update(evaluationObjectCode: string, id: string, request: UpdateEvaluationObjectRoleRequest): Observable<EvaluationObjectRoleDto> {
    return this.http.put<EvaluationObjectRoleDto>(
      `${this.apiUrl}/${evaluationObjectCode}/role/${id}`, 
      request
    );
  }

  delete(evaluationObjectCode: string, id: string): Observable<any> {
    return this.http.delete(
      `${this.apiUrl}/${evaluationObjectCode}/role/${id}`
    );
  }

  getAllAsTree(): Observable<EvaluationObjectRoleTreeResponse> {
    return this.http.get<EvaluationObjectRoleTreeResponse>(
      '/api/page/configuration/evaluation-object-role'
    );
  }

  batchSave(requests: any[]): Observable<any> {
    return this.http.post(
      '/api/page/configuration/evaluation-object-role/batch',
      requests
    );
  }

  search(query: string): Observable<EvaluationObjectRoleTreeResponse> {
    return this.http.get<EvaluationObjectRoleTreeResponse>(
      '/api/page/configuration/evaluation-object-role/search',
      { params: { q: query } }
    );
  }
}
