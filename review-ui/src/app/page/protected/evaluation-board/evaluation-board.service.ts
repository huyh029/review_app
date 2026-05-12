import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EvaluationListItem {
  id: string;
  fullName: string;
  department: string;
  month: number;
  year: number;
  evaluationPeriod: string;
  selfScore: number;
  managerScore: number | null;
  status: string;
}

export interface EvaluationDetail {
  id: string;
  userId: string;
  fullName: string;
  department: string;
  month: number;
  year: number;
  criteriaSetId: string;
  selfScore: number;
  managerScore: number | null;
  status: string;
  scores: EvaluationScore[];
  criteriaTree: CriteriaNode[];
  classifications: ClassificationItem[];
}

export interface EvaluationScore {
  id: string;
  virtualCode: string;
  selfScore: number;
  managerScore: number | null;
}

export interface CriteriaNode {
  virtualCode: string;
  displayCode: string;
  content: string;
  maxScore: number | null;
  scoreType: string;
  selfScore: number | null;
  managerScore: number | null;
  children: CriteriaNode[];
}

export interface ClassificationItem {
  code: string;
  name: string;
  abbreviation: string;
  minScore: number | null;
  maxScore: number | null;
}

export interface EvaluationPaginatedResponse {
  data: EvaluationListItem[];
  pagination: {
    currentPage: number;
    totalPages: number;
    totalItems: number;
    itemsPerPage: number;
    selectableCount: number;
  };
}

export interface EvaluationFilter {
  status?: string;
  month?: number;
  year?: number;
  page?: number;
  pageSize?: number;
}

export interface DeleteManyRequest {
  isAll: boolean;
  ids?: string[];
  excludeIds?: string[];
  filter?: EvaluationFilter;
}

export interface ScoreInput {
  virtualCode: string;
  selfScore: number;
}

export interface ManagerScoreInput {
  virtualCode: string;
  managerScore: number;
}

export interface CommentItem {
  id: string;
  evaluationId: string;
  userId: string;
  userName: string;
  content: string;
  replyToCommentId: string | null;
  createdAt: string;
  replies: CommentItem[];
  reactions: { id: string; userId: string; emoji: string }[];
  files: { id: string; fileName: string; filePath: string; fileType: string; fileSize: number }[];
}

export interface ManagerUser {
  id: string;
  fullName: string;
  department: string;
  position?: string;
}

export interface NewEvaluationTemplate {
  isChanged: boolean;
  isHaveCriteria: boolean;
  criteriaSetId: string;
  criteriaSetName: string;
  fullName: string;
  department: string;
  currentMonth: number;
  currentYear: number;
  warning?: string;
  criteriaTree: CriteriaNode[];
  classifications: ClassificationItem[];
}

@Injectable({ providedIn: 'root' })
export class EvaluationBoardService {
  private selfBase = '/api/page/evaluation-board/self';
  private managerBase = '/api/page/evaluation-board/manager';
  private resultBase = '/api/page/evaluation-board/result';
  private commentBase = '/api/page/evaluation-board/comments';

  constructor(private http: HttpClient) {}

  private buildParams(filter: EvaluationFilter): HttpParams {
    let params = new HttpParams()
      .set('page', filter.page ?? 1)
      .set('pageSize', filter.pageSize ?? 10);
    if (filter.status) params = params.set('status', filter.status);
    if (filter.month) params = params.set('month', filter.month);
    if (filter.year) params = params.set('year', filter.year);
    return params;
  }

  getNewTemplate(month?: number, year?: number, criteriaId?: string): Observable<NewEvaluationTemplate> {
    let params = new HttpParams();
    if (month) params = params.set('month', month);
    if (year) params = params.set('year', year);
    if (criteriaId) params = params.set('criteriaId', criteriaId);
    return this.http.get<NewEvaluationTemplate>(`${this.selfBase}/template`, { params });
  }

  // Self evaluation
  getSelfList(filter: EvaluationFilter = {}): Observable<EvaluationPaginatedResponse> {
    return this.http.get<EvaluationPaginatedResponse>(this.selfBase, { params: this.buildParams(filter) });
  }

  getSelfDetail(id: string): Observable<EvaluationDetail> {
    return this.http.get<EvaluationDetail>(`${this.selfBase}/${id}`);
  }

  createSelf(data: { month: number; year: number; criteriaSetId: string; scores: ScoreInput[] }): Observable<EvaluationDetail> {
    return this.http.post<EvaluationDetail>(this.selfBase, data);
  }

  updateSelf(id: string, scores: ScoreInput[]): Observable<EvaluationDetail> {
    return this.http.put<EvaluationDetail>(`${this.selfBase}/${id}`, { scores });
  }

  submitSelf(id: string, managerId?: string): Observable<any> {
    return this.http.post(`${this.selfBase}/${id}/submit`, managerId ? { managerId } : {});
  }

  getManagers(criteriaSetId: string): Observable<ManagerUser[]> {
    const params = new HttpParams().set('criteriaSetId', criteriaSetId);
    return this.http.get<ManagerUser[]>('/api/page/evaluation-board/managers', { params });
  }

  saveAndSubmit(data: { month: number; year: number; criteriaSetId: string; scores: ScoreInput[]; managerId?: string }): Observable<EvaluationDetail> {
    return this.http.post<EvaluationDetail>(`${this.selfBase}/save-and-submit`, data);
  }

  deleteSelf(id: string): Observable<any> {
    return this.http.delete(`${this.selfBase}/${id}`);
  }

  deleteManySelf(request: DeleteManyRequest): Observable<any> {
    return this.http.delete(this.selfBase, { body: request });
  }

  recallSelf(id: string): Observable<any> {
    return this.http.post(`${this.selfBase}/${id}/recall`, {});
  }

  // Manager evaluation
  getManagerList(filter: EvaluationFilter = {}): Observable<EvaluationPaginatedResponse> {
    return this.http.get<EvaluationPaginatedResponse>(this.managerBase, { params: this.buildParams(filter) });
  }

  getManagerDetail(id: string): Observable<EvaluationDetail> {
    return this.http.get<EvaluationDetail>(`${this.managerBase}/${id}`);
  }

  reviewEvaluation(id: string): Observable<any> {
    return this.http.post(`${this.managerBase}/${id}/review`, {});
  }

  approveEvaluation(id: string, scores: ManagerScoreInput[]): Observable<EvaluationDetail> {
    return this.http.post<EvaluationDetail>(`${this.managerBase}/${id}/approve`, { scores });
  }

  updateEvaluationScores(id: string, scores: ManagerScoreInput[]): Observable<EvaluationDetail> {
    return this.http.post<EvaluationDetail>(`${this.managerBase}/${id}/update-scores`, { scores });
  }

  completeEvaluation(id: string): Observable<any> {
    return this.http.post(`${this.managerBase}/${id}/complete`, {});
  }

  // Result evaluation
  getResultList(filter: EvaluationFilter = {}): Observable<EvaluationPaginatedResponse> {
    return this.http.get<EvaluationPaginatedResponse>(this.resultBase, { params: this.buildParams(filter) });
  }

  getResultDetail(id: string): Observable<EvaluationDetail> {
    return this.http.get<EvaluationDetail>(`${this.resultBase}/${id}`);
  }

  // Comments - self detail
  getSelfComments(evaluationId: string): Observable<CommentItem[]> {
    return this.http.get<CommentItem[]>(`${this.selfBase}/detail/${evaluationId}/comments`);
  }

  addSelfComment(evaluationId: string, data: { content: string; replyToCommentId?: string }): Observable<CommentItem> {
    return this.http.post<CommentItem>(`${this.selfBase}/detail/${evaluationId}/comments`, { ...data, evaluationId });
  }

  deleteSelfComment(evaluationId: string, commentId: string): Observable<any> {
    return this.http.delete(`${this.selfBase}/detail/${evaluationId}/comments/${commentId}`);
  }

  addSelfReaction(evaluationId: string, commentId: string, emoji: string, userId: string): Observable<any> {
    return this.http.post(`${this.selfBase}/detail/${evaluationId}/comments/${commentId}/reactions`, { emoji, userId });
  }

  // Comments
  getComments(evaluationId: string): Observable<CommentItem[]> {
    return this.http.get<CommentItem[]>(`${this.commentBase}/${evaluationId}`);
  }

  addComment(data: { evaluationId: string; content: string; replyToCommentId?: string }): Observable<CommentItem> {
    return this.http.post<CommentItem>(this.commentBase, data);
  }

  deleteComment(id: string): Observable<any> {
    return this.http.delete(`${this.commentBase}/${id}`);
  }

  addReaction(commentId: string, emoji: string, userId: string): Observable<any> {
    return this.http.post(`${this.commentBase}/${commentId}/reactions`, { emoji, userId });
  }

  addCommentFile(commentId: string, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.commentBase}/${commentId}/files`, formData);
  }

  deleteReaction(commentId: string, userId: string): Observable<any> {
    return this.http.delete(`${this.commentBase}/${commentId}/reactions/${userId}`);
  }

  deleteSelfReaction(evaluationId: string, commentId: string, userId: string): Observable<any> {
    return this.http.delete(`${this.selfBase}/detail/${evaluationId}/comments/${commentId}/reactions/${userId}`);
  }
}
