import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreateClassificationRequest {
  criteriaSetId: number;
  code: string;
  virtualId: string;
  name: string;
  abbreviation: string;
  minScore: number;
  maxScore: number;
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

@Injectable({
  providedIn: 'root'
})
export class ClassificationService {
  private apiUrl = '/api/page/configuration/classification';

  constructor(private http: HttpClient) { }

  create(request: CreateClassificationRequest): Observable<ClassificationDto> {
    return this.http.post<ClassificationDto>(this.apiUrl, request);
  }
}
