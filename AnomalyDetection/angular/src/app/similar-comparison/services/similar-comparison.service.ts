import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  SimilaritySearchCriteria,
  SimilarSignalResult,
  TestDataComparison,
  TestDataRecord,
  TestDataFilter,
  ComparisonVisualizationData,
  ExportFormat,
  ComparisonRecommendation
} from '../models/similar-comparison.models';

@Injectable({
  providedIn: 'root'
})
export class SimilarComparisonService {
  private apiUrl = `${environment.apis.default.url}/api/app/similar-pattern-search`;

  constructor(private http: HttpClient) {}

  searchSimilarSignals(criteria: SimilaritySearchCriteria): Observable<SimilarSignalResult[]> {
    return this.http.post<SimilarSignalResult[]>(
      `${this.apiUrl}/search-similar-signals`,
      criteria
    );
  }

  compareTestData(sourceSignalId: string, targetSignalId: string): Observable<TestDataComparison> {
    return this.http.post<TestDataComparison>(
      `${this.apiUrl}/compare-test-data`,
      { sourceSignalId, targetSignalId }
    );
  }

  getTestDataRecords(filter: TestDataFilter): Observable<TestDataRecord[]> {
    let params = new HttpParams();
    
    if (filter.canSignalId) params = params.set('canSignalId', filter.canSignalId);
    if (filter.vehiclePhase) params = params.set('vehiclePhase', filter.vehiclePhase);
    if (filter.oemCode) params = params.set('oemCode', filter.oemCode);
    if (filter.startDate) params = params.set('startDate', filter.startDate.toISOString());
    if (filter.endDate) params = params.set('endDate', filter.endDate.toISOString());
    if (filter.anomalyType) params = params.set('anomalyType', filter.anomalyType);
    if (filter.minDetectionRate !== undefined) params = params.set('minDetectionRate', filter.minDetectionRate.toString());
    if (filter.maxDetectionRate !== undefined) params = params.set('maxDetectionRate', filter.maxDetectionRate.toString());

    return this.http.get<TestDataRecord[]>(`${this.apiUrl}/test-data-records`, { params });
  }

  getSimilarSignalRecommendations(canSignalId: string): Observable<ComparisonRecommendation[]> {
    return this.http.get<ComparisonRecommendation[]>(
      `${this.apiUrl}/similar-signal-recommendations/${canSignalId}`
    );
  }

  getVisualizationData(sourceSignalId: string, targetSignalId: string): Observable<ComparisonVisualizationData> {
    return this.http.get<ComparisonVisualizationData>(
      `${this.apiUrl}/visualization-data`,
      {
        params: {
          sourceSignalId,
          targetSignalId
        }
      }
    );
  }

  exportComparisonResult(comparison: TestDataComparison, format: ExportFormat): Observable<Blob> {
    return this.http.post(
      `${this.apiUrl}/export-comparison`,
      { comparison, format },
      { responseType: 'blob' }
    );
  }

  downloadExport(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}
