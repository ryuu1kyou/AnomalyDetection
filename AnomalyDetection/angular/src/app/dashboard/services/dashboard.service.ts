import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  DashboardStatistics,
  DetectionStatistics,
  SystemAnomalyReport,
  GetDetectionStatisticsInput,
  GenerateSystemReportInput
} from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly baseUrl = '/api/app/statistics';

  constructor(private http: HttpClient) {}

  // Dashboard overview statistics
  getDashboardStatistics(): Observable<DashboardStatistics> {
    return this.http.get<DashboardStatistics>(`${this.baseUrl}/dashboard`);
  }

  // Detection statistics with filtering
  getDetectionStatistics(input: GetDetectionStatisticsInput): Observable<DetectionStatistics> {
    let params = new HttpParams();
    
    if (input.startDate) params = params.set('startDate', input.startDate.toISOString());
    if (input.endDate) params = params.set('endDate', input.endDate.toISOString());
    if (input.systemType) params = params.set('systemType', input.systemType);
    if (input.oemCode) params = params.set('oemCode', input.oemCode);
    if (input.anomalyLevel !== undefined) params = params.set('anomalyLevel', input.anomalyLevel.toString());
    if (input.groupBy) params = params.set('groupBy', input.groupBy);

    return this.http.get<DetectionStatistics>(`${this.baseUrl}/detection-statistics`, { params });
  }

  // System-specific anomaly reports
  getSystemAnomalyReport(systemType: string, input?: GetDetectionStatisticsInput): Observable<SystemAnomalyReport> {
    let params = new HttpParams();
    
    if (input?.startDate) params = params.set('startDate', input.startDate.toISOString());
    if (input?.endDate) params = params.set('endDate', input.endDate.toISOString());
    if (input?.oemCode) params = params.set('oemCode', input.oemCode);

    return this.http.get<SystemAnomalyReport>(`${this.baseUrl}/system-report/${systemType}`, { params });
  }

  // Generate comprehensive system report
  generateSystemReport(input: GenerateSystemReportInput): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/generate-report`, input, {
      responseType: 'blob'
    });
  }

  // Real-time statistics (for live updates)
  getRealTimeStatistics(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/realtime`);
  }

  // Performance metrics
  getPerformanceMetrics(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/performance`);
  }

  // System health metrics
  getSystemHealth(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/system-health`);
  }

  // Trend analysis
  getTrendAnalysis(period: 'week' | 'month' | 'quarter' | 'year'): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/trends?period=${period}`);
  }

  // Comparative analysis between OEMs
  getOemComparison(oemCodes: string[]): Observable<any> {
    const params = new HttpParams().set('oemCodes', oemCodes.join(','));
    return this.http.get<any>(`${this.baseUrl}/oem-comparison`, { params });
  }

  // Anomaly pattern analysis
  getAnomalyPatterns(systemType?: string): Observable<any> {
    let params = new HttpParams();
    if (systemType) params = params.set('systemType', systemType);
    
    return this.http.get<any>(`${this.baseUrl}/anomaly-patterns`, { params });
  }

  // Detection accuracy metrics
  getDetectionAccuracy(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/detection-accuracy`);
  }

  // Export dashboard data
  exportDashboardData(format: 'pdf' | 'excel' | 'csv'): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/export?format=${format}`, {
      responseType: 'blob'
    });
  }

  // Custom query for advanced analytics
  executeCustomQuery(query: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/custom-query`, query);
  }
}