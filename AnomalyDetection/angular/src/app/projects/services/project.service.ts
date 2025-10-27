import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  AnomalyDetectionProject,
  ProjectMilestone,
  ProjectMember,
  GetProjectsInput,
  CreateProjectDto,
  UpdateProjectDto,
  CreateProjectMilestoneDto,
  UpdateProjectMilestoneDto,
  CreateProjectMemberDto,
  UpdateProjectMemberDto,
  ProjectOperationDto,
  ProjectStatistics,
  ProjectStatus,
  ProjectPriority
} from '../models/project.model';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private readonly baseUrl = '/api/app/anomaly-detection-project';

  constructor(private http: HttpClient) {}

  // Project CRUD operations
  getList(input: GetProjectsInput): Observable<PagedResult<AnomalyDetectionProject>> {
    let params = new HttpParams();
    
    if (input.filter) params = params.set('filter', input.filter);
    if (input.status !== undefined) params = params.set('status', input.status.toString());
    if (input.priority !== undefined) params = params.set('priority', input.priority.toString());
    if (input.oemCode) params = params.set('oemCode', input.oemCode);
    if (input.primarySystem) params = params.set('primarySystem', input.primarySystem);
    if (input.vehicleModel) params = params.set('vehicleModel', input.vehicleModel);
    if (input.startDateFrom) params = params.set('startDateFrom', input.startDateFrom.toISOString());
    if (input.startDateTo) params = params.set('startDateTo', input.startDateTo.toISOString());
    if (input.endDateFrom) params = params.set('endDateFrom', input.endDateFrom.toISOString());
    if (input.endDateTo) params = params.set('endDateTo', input.endDateTo.toISOString());
    if (input.skipCount !== undefined) params = params.set('skipCount', input.skipCount.toString());
    if (input.maxResultCount !== undefined) params = params.set('maxResultCount', input.maxResultCount.toString());
    if (input.sorting) params = params.set('sorting', input.sorting);

    return this.http.get<PagedResult<AnomalyDetectionProject>>(`${this.baseUrl}`, { params });
  }

  get(id: string): Observable<AnomalyDetectionProject> {
    return this.http.get<AnomalyDetectionProject>(`${this.baseUrl}/${id}`);
  }

  create(input: CreateProjectDto): Observable<AnomalyDetectionProject> {
    return this.http.post<AnomalyDetectionProject>(`${this.baseUrl}`, input);
  }

  update(id: string, input: UpdateProjectDto): Observable<AnomalyDetectionProject> {
    return this.http.put<AnomalyDetectionProject>(`${this.baseUrl}/${id}`, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  // Project status operations
  startProject(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/start`, {});
  }

  completeProject(id: string, notes?: string): Observable<void> {
    const body = notes ? { notes } : {};
    return this.http.post<void>(`${this.baseUrl}/${id}/complete`, body);
  }

  pauseProject(id: string, reason?: string): Observable<void> {
    const body = reason ? { reason } : {};
    return this.http.post<void>(`${this.baseUrl}/${id}/pause`, body);
  }

  cancelProject(id: string, reason?: string): Observable<void> {
    const body = reason ? { reason } : {};
    return this.http.post<void>(`${this.baseUrl}/${id}/cancel`, body);
  }

  // Milestone operations
  getMilestones(projectId: string): Observable<ProjectMilestone[]> {
    return this.http.get<ProjectMilestone[]>(`${this.baseUrl}/${projectId}/milestones`);
  }

  createMilestone(input: CreateProjectMilestoneDto): Observable<ProjectMilestone> {
    return this.http.post<ProjectMilestone>(`${this.baseUrl}/milestones`, input);
  }

  updateMilestone(id: string, input: UpdateProjectMilestoneDto): Observable<ProjectMilestone> {
    return this.http.put<ProjectMilestone>(`${this.baseUrl}/milestones/${id}`, input);
  }

  deleteMilestone(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/milestones/${id}`);
  }

  completeMilestone(id: string, notes?: string): Observable<void> {
    const body = notes ? { notes } : {};
    return this.http.post<void>(`${this.baseUrl}/milestones/${id}/complete`, body);
  }

  // Member operations
  getMembers(projectId: string): Observable<ProjectMember[]> {
    return this.http.get<ProjectMember[]>(`${this.baseUrl}/${projectId}/members`);
  }

  addMember(input: CreateProjectMemberDto): Observable<ProjectMember> {
    return this.http.post<ProjectMember>(`${this.baseUrl}/members`, input);
  }

  updateMember(id: string, input: UpdateProjectMemberDto): Observable<ProjectMember> {
    return this.http.put<ProjectMember>(`${this.baseUrl}/members/${id}`, input);
  }

  removeMember(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/members/${id}`);
  }

  // Statistics and reporting
  getStatistics(): Observable<ProjectStatistics> {
    return this.http.get<ProjectStatistics>(`${this.baseUrl}/statistics`);
  }

  getProjectProgress(id: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}/progress`);
  }

  generateProgressReport(id: string, format: string = 'pdf'): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/progress-report?format=${format}`, {
      responseType: 'blob'
    });
  }

  // Bulk operations
  bulkUpdateStatus(ids: string[], status: ProjectStatus, notes?: string): Observable<void> {
    const body = { ids, status, notes };
    return this.http.post<void>(`${this.baseUrl}/bulk-update-status`, body);
  }

  bulkDelete(ids: string[]): Observable<void> {
    const body = { ids };
    return this.http.post<void>(`${this.baseUrl}/bulk-delete`, body);
  }

  // Project operations
  executeOperation(id: string, operation: ProjectOperationDto): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/${id}/operations`, operation);
  }

  // Export operations
  export(input: GetProjectsInput, format: string = 'csv'): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/export?format=${format}`, input, {
      responseType: 'blob'
    });
  }

  // Dashboard data
  getDashboardData(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/dashboard`);
  }

  getProjectTimeline(id: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/${id}/timeline`);
  }

  // Related data
  getRelatedDetectionLogics(id: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/${id}/detection-logics`);
  }

  getRelatedCanSignals(id: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/${id}/can-signals`);
  }

  getRelatedAnomalies(id: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/${id}/anomalies`);
  }
}