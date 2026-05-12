import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EvaluationFlowDto {
  code: string;
  name: string;
  departmentCode: string;
  isActive: number;
}

export interface CreateEvaluationFlowRequest {
  code: string;
  name: string;
  departmentCode: string;
}

export interface UpdateEvaluationFlowRequest {
  name: string;
  departmentCode: string;
  isActive: number;
}

export interface TreeNodeRequest {
  code: string;
  name: string;
  roleCode?: string;
  objectCode?: string;
  children?: TreeNodeRequest[];
}

export interface DepartmentRequest {
  code: string;
  name: string;
}

export interface ClassificationRequest {
  code: string;
  name: string;
  minScore?: number;
  maxScore?: number;
}

export interface EvaluationFlowDetailDto {
  code: string;
  name: string;
  departments: DepartmentRequest[];
  roles: TreeNodeRequest[];
  objects: TreeNodeRequest[];
  criteria: string[];
  isActive: number;
}

export interface CreateEvaluationFlowDetailRequest {
  code: string;
  name: string;
  departments: string[];
  roles: any[];
  objects: any[];
  criteria: string[];
}

export interface UpdateEvaluationFlowDetailRequest {
  name: string;
  departments: string[];
  roles: any[];
  objects: any[];
  criteria: string[];
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

export interface CriteriaSetDto {
  id: number;
  name: string;
  applicableYears: string;
  applicableMonths: string;
  isActive: number;
}

export interface DepartmentTreeDto {
  id: string;
  code: string;
  name: string;
  checked: boolean;
  expanded: boolean;
  children: DepartmentTreeDto[] | null;
}

export interface RoleDto {
  code: string;
  name: string;
  checked: boolean;
}

export interface EvaluationObjectDto {
  code: string;
  name: string;
  checked: boolean;
}

export interface AvailableItem {
  code: string;
  name: string;
}

export interface DeleteManyEvaluationFlowRequest {
  isAll: boolean;
  includeIds?: string[];
  excludeIds?: string[];
  filter?: { search?: string };
}

@Injectable({
  providedIn: 'root'
})
export class EvaluationFlowService {
  private apiUrl = '/api/page/configuration/evaluation-flow';
  private detailApiUrl = '/api/page/configuration/evaluation-flow-detail';
  private criteriaApiUrl = '/api/page/configuration/evaluation-flow-criteria';
  private departmentApiUrl = '/api/page/configuration/evaluation-flow-department';
  private roleApiUrl = '/api/page/configuration/evaluation-flow-role';
  private objectApiUrl = '/api/page/configuration/evaluation-flow-object';

  constructor(private http: HttpClient) { }

  getAll(search?: string, page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<EvaluationFlowDto>> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }
    params = params.set('page', page.toString());
    params = params.set('pageSize', pageSize.toString());
    
    return this.http.get<PaginatedResponse<EvaluationFlowDto>>(this.apiUrl, { params });
  }

  getByCode(code: string): Observable<EvaluationFlowDto> {
    return this.http.get<EvaluationFlowDto>(`${this.apiUrl}/${code}`);
  }

  create(request: CreateEvaluationFlowRequest): Observable<EvaluationFlowDto> {
    return this.http.post<EvaluationFlowDto>(this.apiUrl, request);
  }

  update(code: string, request: UpdateEvaluationFlowRequest): Observable<EvaluationFlowDto> {
    return this.http.put<EvaluationFlowDto>(`${this.apiUrl}/${code}`, request);
  }

  delete(code: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${code}`);
  }

  deleteMany(request: DeleteManyEvaluationFlowRequest): Observable<any> {
    return this.http.delete(this.apiUrl, { body: request });
  }

  getDetail(code: string): Observable<EvaluationFlowDetailDto> {
    return this.http.get<EvaluationFlowDetailDto>(`${this.detailApiUrl}/${code}/detail`);
  }

  createDetail(request: CreateEvaluationFlowDetailRequest): Observable<EvaluationFlowDetailDto> {
    return this.http.post<EvaluationFlowDetailDto>(`${this.detailApiUrl}/detailRequest`, request);
  }

  updateDetail(code: string, request: UpdateEvaluationFlowDetailRequest): Observable<EvaluationFlowDetailDto> {
    return this.http.put<EvaluationFlowDetailDto>(`${this.detailApiUrl}/${code}/detail`, request);
  }

  getCriteriaList(): Observable<CriteriaSetDto[]> {
    return this.http.get<CriteriaSetDto[]>(this.criteriaApiUrl);
  }

  getDepartmentTree(): Observable<DepartmentTreeDto[]> {
    return this.http.get<DepartmentTreeDto[]>(`${this.departmentApiUrl}/treeRequest`);
  }

  getRoleList(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(`${this.roleApiUrl}/list`);
  }

  getObjectList(): Observable<EvaluationObjectDto[]> {
    return this.http.get<EvaluationObjectDto[]>(`${this.objectApiUrl}/list`);
  }
}
