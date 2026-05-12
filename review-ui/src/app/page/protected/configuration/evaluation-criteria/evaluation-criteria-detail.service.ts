import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EvaluationObjectDto {
  code: string;
  name: string;
}

export interface CreateEvaluationCriteriaDetailRequest {
  criteriaSetId: number;
  criteriaVirtualCode: string;
  order: number;
  weight: number;
}

export interface EvaluationCriteriaDetailDto {
  id: number;
  criteriaSetId: number;
  criteriaVirtualCode: string;
  order: number;
  weight: number;
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

export interface ClassificationDto {
  criteriaSetId: number;
  code: string;
  virtualId: string;
  name: string;
  abbreviation: string;
  minScore: number;
  maxScore: number;
  isActive: number;
}

export interface CriteriaSetDetailDto {
  id: number;
  name: string;
  applicableYears: string;
  applicableMonths: string;
  isActive: number;
  objectCodes: string[];
  criteria: CriteriaDto[];
  classifications: ClassificationDto[];
}

@Injectable({
  providedIn: 'root'
})
export class EvaluationCriteriaDetailService {
  private apiUrl = '/api/page/configuration/evaluation-criteria-detail';
  private criteriaApiUrl = '/api/page/configuration/evaluation-criteria';

  constructor(private http: HttpClient) { }

  getActiveEvaluationObjects(): Observable<EvaluationObjectDto[]> {
    return this.http.get<EvaluationObjectDto[]>(`${this.apiUrl}/active-evaluation-objects`);
  }

  create(request: CreateEvaluationCriteriaDetailRequest): Observable<EvaluationCriteriaDetailDto> {
    return this.http.post<EvaluationCriteriaDetailDto>(this.apiUrl, request);
  }

  getDetail(id: number): Observable<CriteriaSetDetailDto> {
    return this.http.get<CriteriaSetDetailDto>(`${this.apiUrl}/${id}/detail`);
  }

  updateDetail(id: number, request: any): Observable<CriteriaSetDetailDto> {
    return this.http.put<CriteriaSetDetailDto>(`${this.apiUrl}/${id}/detail`, request);
  }
}
