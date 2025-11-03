import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  DashboardStatistics,
  DetectionStatistics,
  SystemAnomalyReport,
  GetDetectionStatisticsInput,
  GenerateSystemReportInput,
  CategoryData,
  TimeSeriesData,
  TrendData,
} from '../models/dashboard.model';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  // Use absolute backend URL to avoid dev-server HTML fallback issues (index.html returned as <!DOCTYPE...)
  private readonly baseUrl = `${environment.apis.default.url}/api/app/statistics`;

  constructor(private http: HttpClient) {}

  // Dashboard overview statistics
  getDashboardStatistics(): Observable<DashboardStatistics> {
    // Backend method name: GetDashboardStatisticsAsync -> ABP route segment: 'dashboard-statistics'
    // Previous '/dashboard' path caused a genuine 404. Adjusted.
    // Some endpoints may occasionally return an empty body (""), which causes Angular JSON parse errors (HttpErrorResponse with status 200, ok=false).
    // Request as text and manually parse to avoid false-negative errors.
    return this.http.get(`${this.baseUrl}/dashboard-statistics`, { responseType: 'text' }).pipe(
      // Manual safe parse
      map(raw => {
        if (!raw) {
          return {} as DashboardStatistics; // fallback empty object
        }
        if (raw.startsWith('<!DOCTYPE')) {
          console.error(
            '[DashboardService] Received HTML instead of JSON (likely dev server index.html). Check proxy configuration or absolute API URL.'
          );
          return {} as DashboardStatistics;
        }
        try {
          return JSON.parse(raw) as DashboardStatistics;
        } catch (e) {
          console.warn('Failed to parse dashboard statistics JSON, returning empty object.', e);
          return {} as DashboardStatistics;
        }
      })
    );
  }

  // Detection statistics with filtering
  getDetectionStatistics(input: GetDetectionStatisticsInput): Observable<DetectionStatistics> {
    // Only send parameters the backend actually needs; avoid sending SystemTypes until multi-select support.
    let params = new HttpParams();
    if (input.startDate) params = params.set('FromDate', input.startDate.toISOString());
    if (input.endDate) params = params.set('ToDate', input.endDate.toISOString());
    if (input.oemCode) params = params.set('OemCode', input.oemCode);
    if (input.anomalyLevel !== undefined)
      params = params.set('AnomalyLevels', input.anomalyLevel.toString());
    if (input.groupBy) {
      const enumValue = input.groupBy.charAt(0).toUpperCase() + input.groupBy.slice(1);
      params = params.set('GroupBy', enumValue);
    }

    return this.http
      .get(`${this.baseUrl}/detection-statistics`, { params, responseType: 'text' })
      .pipe(
        map(raw => this.safeParse(raw)),
        map(dto => this.mapDetectionStatisticsDtoToUi(dto))
      );
  }

  /**
   * Safely parse a raw text response into JSON, returning an empty object on failure.
   */
  private safeParse(raw: string): any {
    if (!raw) return {};
    if (raw.startsWith('<!DOCTYPE')) {
      console.error('[DashboardService] Received HTML instead of JSON.');
      return {};
    }
    try {
      return JSON.parse(raw);
    } catch (e) {
      console.warn('JSON parse error. Returning empty object.', e);
      return {};
    }
  }

  /**
   * Map backend DetectionStatisticsDto shape to simplified front-end DetectionStatistics interface.
   * Backend fields reference enum keys; convert dictionaries into CategoryData arrays with percentages.
   */
  private mapDetectionStatisticsDtoToUi(dto: any): DetectionStatistics {
    if (!dto || Object.keys(dto).length === 0) return {} as DetectionStatistics;

    const total = dto.totalDetections || 0;

    const anomalyLevelDistribution: CategoryData[] = this.mapDictionaryToCategories(
      dto.detectionsByAnomalyLevel,
      total,
      level => this.enumKeyToLabel(level)
    );

    const systemTypeDistribution: CategoryData[] = this.mapDictionaryToCategories(
      dto.detectionsBySystemType,
      total,
      system => system
    );

    const resolutionStatusDistribution: CategoryData[] = this.mapDictionaryToCategories(
      {
        Resolved: dto.totalResolvedDetections,
        FalsePositive: dto.totalFalsePositives,
        Open: dto.totalOpenDetections,
      },
      total,
      s => s
    );

    const dailyDetections: TimeSeriesData[] = (dto.dailyStatistics || []).map((d: any) => ({
      date: new Date(d.date),
      value: d.totalDetections,
    }));

    const detectionTrend: TrendData = this.mapTrend(dto.detectionTrend, '前期間比');
    const resolutionTrend: TrendData = this.mapTrend(dto.resolutionTrend, '前期間比');

    return {
      dailyDetections,
      weeklyDetections: [],
      monthlyDetections: [],
      anomalyLevelDistribution,
      systemTypeDistribution,
      oemDistribution: [],
      detectionTypeDistribution: [],
      resolutionStatusDistribution,
      detectionTrend,
      resolutionTrend,
    } as DetectionStatistics;
  }

  private mapDictionaryToCategories(
    dict: Record<string, number> | undefined,
    total: number,
    labelTransform: (key: string) => string
  ): CategoryData[] {
    if (!dict) return [];
    return Object.entries(dict)
      .filter(([, value]) => value > 0)
      .map(([key, value]) => ({
        category: labelTransform(key),
        value,
        percentage: total > 0 ? Math.round((value / total) * 100) : 0,
      }));
  }

  private enumKeyToLabel(key: string): string {
    // Provide Japanese labels if desired; fallback to key.
    const map: Record<string, string> = {
      Info: '情報',
      Warning: '警告',
      Error: 'エラー',
      Critical: 'クリティカル',
      Fatal: '致命的',
    };
    return map[key] || key;
  }

  private mapTrend(value: number | undefined, period: string): TrendData {
    if (value === undefined || value === null) {
      return { direction: 'stable', percentage: 0, period };
    }
    const direction = value > 0 ? 'up' : value < 0 ? 'down' : 'stable';
    return { direction, percentage: Math.round(Math.abs(value)), period };
  }

  // System-specific anomaly reports
  getSystemAnomalyReport(
    systemType: string,
    input?: GetDetectionStatisticsInput
  ): Observable<SystemAnomalyReport> {
    let params = new HttpParams();

    if (input?.startDate) params = params.set('startDate', input.startDate.toISOString());
    if (input?.endDate) params = params.set('endDate', input.endDate.toISOString());
    if (input?.oemCode) params = params.set('oemCode', input.oemCode);

    return this.http.get<SystemAnomalyReport>(`${this.baseUrl}/system-report/${systemType}`, {
      params,
    });
  }

  // Generate comprehensive system report
  generateSystemReport(input: GenerateSystemReportInput): Observable<Blob> {
    // Backend method: GenerateSystemReportAsync -> 'generate-system-report'
    return this.http.post(`${this.baseUrl}/generate-system-report`, input, {
      responseType: 'blob',
    });
  }

  // Real-time statistics (for live updates)
  getRealTimeStatistics(): Observable<any> {
    // Backend method: GetRealTimeStatisticsAsync -> 'real-time-statistics'
    return this.http.get<any>(`${this.baseUrl}/real-time-statistics`);
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
  exportDashboardData(format: 'pdf' | 'excel' | 'csv' | 'json'): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/export-dashboard-data?format=${format}`, {
      responseType: 'blob',
    });
  }

  // Custom query for advanced analytics
  executeCustomQuery(query: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/custom-query`, query);
  }
}
