import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  AnomalyDetectionResult, 
  GetDetectionResultsInput,
  MarkAsFalsePositiveDto,
  ReopenDetectionResultDto,
  ShareDetectionResultDto,
  ResolveDetectionResultDto,
  AnomalyLevel,
  ResolutionStatus
} from '../models/detection-result.model';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class DetectionResultService {
  private readonly baseUrl = '/api/app/anomaly-detection-result';

  constructor(private http: HttpClient) {}

  getList(input: GetDetectionResultsInput): Observable<PagedResult<AnomalyDetectionResult>> {
    let params = new HttpParams();
    
    if (input.filter) params = params.set('filter', input.filter);
    if (input.detectionLogicId) params = params.set('detectionLogicId', input.detectionLogicId);
    if (input.canSignalId) params = params.set('canSignalId', input.canSignalId);
    if (input.anomalyLevel !== undefined) params = params.set('anomalyLevel', input.anomalyLevel.toString());
    if (input.resolutionStatus !== undefined) params = params.set('resolutionStatus', input.resolutionStatus.toString());
    if (input.sharingLevel !== undefined) params = params.set('sharingLevel', input.sharingLevel.toString());
    if (input.detectionType !== undefined) params = params.set('detectionType', input.detectionType.toString());
    if (input.systemType !== undefined) params = params.set('systemType', input.systemType.toString());
    if (input.detectedFrom) params = params.set('detectedFrom', input.detectedFrom.toISOString());
    if (input.detectedTo) params = params.set('detectedTo', input.detectedTo.toISOString());
    if (input.resolvedFrom) params = params.set('resolvedFrom', input.resolvedFrom.toISOString());
    if (input.resolvedTo) params = params.set('resolvedTo', input.resolvedTo.toISOString());
    if (input.minConfidenceScore !== undefined) params = params.set('minConfidenceScore', input.minConfidenceScore.toString());
    if (input.maxConfidenceScore !== undefined) params = params.set('maxConfidenceScore', input.maxConfidenceScore.toString());
    if (input.isShared !== undefined) params = params.set('isShared', input.isShared.toString());
    if (input.isHighPriority !== undefined) params = params.set('isHighPriority', input.isHighPriority.toString());
    if (input.maxAge !== undefined) params = params.set('maxAge', input.maxAge.toString());
    if (input.skipCount !== undefined) params = params.set('skipCount', input.skipCount.toString());
    if (input.maxResultCount !== undefined) params = params.set('maxResultCount', input.maxResultCount.toString());
    if (input.sorting) params = params.set('sorting', input.sorting);

    return this.http.get<PagedResult<AnomalyDetectionResult>>(`${this.baseUrl}`, { params });
  }

  get(id: string): Observable<AnomalyDetectionResult> {
    return this.http.get<AnomalyDetectionResult>(`${this.baseUrl}/${id}`);
  }

  getRecent(count: number = 10): Observable<AnomalyDetectionResult[]> {
    return this.http.get<AnomalyDetectionResult[]>(`${this.baseUrl}/recent?count=${count}`);
  }

  getHighPriority(): Observable<AnomalyDetectionResult[]> {
    return this.http.get<AnomalyDetectionResult[]>(`${this.baseUrl}/high-priority`);
  }

  markAsInvestigating(id: string, notes?: string): Observable<void> {
    const body = notes ? { notes } : {};
    return this.http.post<void>(`${this.baseUrl}/${id}/mark-investigating`, body);
  }

  markAsFalsePositive(id: string, input: MarkAsFalsePositiveDto): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/mark-false-positive`, input);
  }

  resolve(id: string, input: ResolveDetectionResultDto): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/resolve`, input);
  }

  reopen(id: string, input: ReopenDetectionResultDto): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/reopen`, input);
  }

  share(id: string, input: ShareDetectionResultDto): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/share`, input);
  }

  revokeSharing(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}/sharing`);
  }

  getSharedResults(input: GetDetectionResultsInput): Observable<PagedResult<AnomalyDetectionResult>> {
    let params = new HttpParams();
    // Add parameters similar to getList
    return this.http.get<PagedResult<AnomalyDetectionResult>>(`${this.baseUrl}/shared`, { params });
  }

  bulkUpdateResolutionStatus(ids: string[], status: ResolutionStatus, notes?: string): Observable<void> {
    const body = { ids, status, notes };
    return this.http.post<void>(`${this.baseUrl}/bulk-update-status`, body);
  }

  bulkMarkAsFalsePositive(ids: string[], reason: string): Observable<void> {
    const body = { ids, reason };
    return this.http.post<void>(`${this.baseUrl}/bulk-mark-false-positive`, body);
  }

  getStatistics(input: GetDetectionResultsInput): Observable<Record<string, any>> {
    return this.http.post<Record<string, any>>(`${this.baseUrl}/statistics`, input);
  }

  export(input: GetDetectionResultsInput, format: string): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/export?format=${format}`, input, { 
      responseType: 'blob' 
    });
  }

  getTimeline(canSignalId?: string, detectionLogicId?: string, fromDate?: Date, toDate?: Date): Observable<any[]> {
    let params = new HttpParams();
    if (canSignalId) params = params.set('canSignalId', canSignalId);
    if (detectionLogicId) params = params.set('detectionLogicId', detectionLogicId);
    if (fromDate) params = params.set('fromDate', fromDate.toISOString());
    if (toDate) params = params.set('toDate', toDate.toISOString());
    
    return this.http.get<any[]>(`${this.baseUrl}/timeline`, { params });
  }

  getSimilarResults(id: string, count: number = 5): Observable<AnomalyDetectionResult[]> {
    return this.http.get<AnomalyDetectionResult[]>(`${this.baseUrl}/${id}/similar?count=${count}`);
  }
}