import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AnomalyDetectionResult,
  GetDetectionResultsInput,
  MarkAsFalsePositiveDto,
  ReopenDetectionResultDto,
  ShareDetectionResultDto,
  ResolveDetectionResultDto,
  AnomalyLevel,
  ResolutionStatus,
} from '../models/detection-result.model';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root',
})
export class DetectionResultService {
  private readonly baseUrl = `${environment.apis.default.url}/api/app/anomaly-detection-result`;

  constructor(private http: HttpClient) {}

  getList(input: GetDetectionResultsInput): Observable<PagedResult<AnomalyDetectionResult>> {
    let params = new HttpParams();

    if (input.filter) params = params.set('filter', input.filter);
    if (input.detectionLogicId) params = params.set('detectionLogicId', input.detectionLogicId);
    if (input.canSignalId) params = params.set('canSignalId', input.canSignalId);
    if (input.anomalyLevel != null) params = params.set('anomalyLevel', String(input.anomalyLevel));
    if (input.resolutionStatus != null)
      params = params.set('resolutionStatus', String(input.resolutionStatus));
    if (input.sharingLevel != null) params = params.set('sharingLevel', String(input.sharingLevel));
    if (input.detectionType != null)
      params = params.set('detectionType', String(input.detectionType));
    if (input.systemType != null) params = params.set('systemType', String(input.systemType));
    if (input.detectedFrom) params = params.set('detectedFrom', input.detectedFrom.toISOString());
    if (input.detectedTo) params = params.set('detectedTo', input.detectedTo.toISOString());
    if (input.resolvedFrom) params = params.set('resolvedFrom', input.resolvedFrom.toISOString());
    if (input.resolvedTo) params = params.set('resolvedTo', input.resolvedTo.toISOString());
    if (input.minConfidenceScore != null)
      params = params.set('minConfidenceScore', String(input.minConfidenceScore));
    if (input.maxConfidenceScore != null)
      params = params.set('maxConfidenceScore', String(input.maxConfidenceScore));
    if (input.isShared != null) params = params.set('isShared', String(input.isShared));
    if (input.isHighPriority != null)
      params = params.set('isHighPriority', String(input.isHighPriority));
    if (input.maxAge != null) params = params.set('maxAge', String(input.maxAge));
    if (input.skipCount != null) params = params.set('skipCount', String(input.skipCount));
    if (input.maxResultCount != null)
      params = params.set('maxResultCount', String(input.maxResultCount));
    if (input.sorting) params = params.set('sorting', input.sorting);

    return this.http.get(`${this.baseUrl}`, { params, responseType: 'text' }).pipe(
      map(raw => {
        if (!raw) {
          return { items: [], totalCount: 0 } as PagedResult<AnomalyDetectionResult>;
        }
        if (raw.startsWith('<!DOCTYPE')) {
          console.warn(
            '[DetectionResultService] HTML received instead of JSON for list. Returning empty list.'
          );
          return { items: [], totalCount: 0 } as PagedResult<AnomalyDetectionResult>;
        }
        try {
          return JSON.parse(raw) as PagedResult<AnomalyDetectionResult>;
        } catch (e) {
          console.warn(
            '[DetectionResultService] Failed to parse detection results JSON. Returning empty list.',
            e
          );
          return { items: [], totalCount: 0 } as PagedResult<AnomalyDetectionResult>;
        }
      })
    );
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

  getSharedResults(
    input: GetDetectionResultsInput
  ): Observable<PagedResult<AnomalyDetectionResult>> {
    let params = new HttpParams();
    // Add parameters similar to getList
    return this.http.get<PagedResult<AnomalyDetectionResult>>(`${this.baseUrl}/shared`, { params });
  }

  bulkUpdateResolutionStatus(
    ids: string[],
    status: ResolutionStatus,
    notes?: string
  ): Observable<void> {
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
      responseType: 'blob',
    });
  }

  getTimeline(
    canSignalId?: string,
    detectionLogicId?: string,
    fromDate?: Date,
    toDate?: Date
  ): Observable<any[]> {
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
