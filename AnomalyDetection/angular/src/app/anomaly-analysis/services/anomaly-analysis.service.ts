import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AnomalyPatternAnalysisDto,
  ThresholdRecommendationResultDto,
  DetectionAccuracyMetricsDto,
  AnomalyAnalysisRequestDto,
  ThresholdRecommendationRequestDto,
  DetectionAccuracyRequestDto,
} from '../models/anomaly-analysis.models';

@Injectable({
  providedIn: 'root',
})
export class AnomalyAnalysisService {
  private readonly baseUrl = '/api/app/anomaly-analysis';

  constructor(private http: HttpClient) {}

  /**
   * 異常パターン分析を実行する
   */
  analyzeAnomalyPattern(request: AnomalyAnalysisRequestDto): Observable<AnomalyPatternAnalysisDto> {
    return this.http.post<AnomalyPatternAnalysisDto>(
      `${this.baseUrl}/analyze-anomaly-pattern`,
      request
    );
  }

  /**
   * 異常パターン分析を実行する（簡易版）
   */
  analyzeAnomalyPatternSimple(
    canSignalId: string,
    startDate: Date,
    endDate: Date
  ): Observable<AnomalyPatternAnalysisDto> {
    return this.http.get<AnomalyPatternAnalysisDto>(`${this.baseUrl}/analyze-anomaly-pattern`, {
      params: {
        canSignalId,
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString(),
      },
    });
  }

  /**
   * 閾値最適化推奨を取得する
   */
  getThresholdRecommendations(
    request: ThresholdRecommendationRequestDto
  ): Observable<ThresholdRecommendationResultDto> {
    return this.http.post<ThresholdRecommendationResultDto>(
      `${this.baseUrl}/threshold-recommendations`,
      request
    );
  }

  /**
   * 閾値最適化推奨を取得する（簡易版）
   */
  getThresholdRecommendationsSimple(
    detectionLogicId: string,
    startDate: Date,
    endDate: Date
  ): Observable<ThresholdRecommendationResultDto> {
    return this.http.get<ThresholdRecommendationResultDto>(
      `${this.baseUrl}/threshold-recommendations`,
      {
        params: {
          detectionLogicId,
          startDate: startDate.toISOString(),
          endDate: endDate.toISOString(),
        },
      }
    );
  }

  /**
   * ML-based統計的最適化による高度な閾値推奨を取得する
   */
  getAdvancedThresholdRecommendations(
    request: ThresholdRecommendationRequestDto
  ): Observable<ThresholdRecommendationResultDto> {
    return this.http.post<ThresholdRecommendationResultDto>(
      `${this.baseUrl}/advanced-threshold-recommendations`,
      request
    );
  }

  /**
   * ML-based統計的最適化による高度な閾値推奨を取得する（簡易版）
   */
  getAdvancedThresholdRecommendationsSimple(
    detectionLogicId: string,
    startDate: Date,
    endDate: Date
  ): Observable<ThresholdRecommendationResultDto> {
    return this.http.get<ThresholdRecommendationResultDto>(
      `${this.baseUrl}/advanced-threshold-recommendations`,
      {
        params: {
          detectionLogicId,
          startDate: startDate.toISOString(),
          endDate: endDate.toISOString(),
        },
      }
    );
  }

  /**
   * 検出精度評価メトリクスを取得する
   */
  getDetectionAccuracyMetrics(
    request: DetectionAccuracyRequestDto
  ): Observable<DetectionAccuracyMetricsDto> {
    return this.http.post<DetectionAccuracyMetricsDto>(
      `${this.baseUrl}/detection-accuracy-metrics`,
      request
    );
  }

  /**
   * 検出精度評価メトリクスを取得する（簡易版）
   */
  getDetectionAccuracyMetricsSimple(
    detectionLogicId: string,
    startDate: Date,
    endDate: Date
  ): Observable<DetectionAccuracyMetricsDto> {
    return this.http.get<DetectionAccuracyMetricsDto>(
      `${this.baseUrl}/detection-accuracy-metrics`,
      {
        params: {
          detectionLogicId,
          startDate: startDate.toISOString(),
          endDate: endDate.toISOString(),
        },
      }
    );
  }
}
