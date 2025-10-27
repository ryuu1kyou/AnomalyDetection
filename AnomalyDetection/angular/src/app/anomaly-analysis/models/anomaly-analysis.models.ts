export interface AnomalyPatternAnalysisDto {
  canSignalId: string;
  analysisStartDate: Date;
  analysisEndDate: Date;
  totalAnomalies: number;
  anomalyTypeDistribution: { [key: string]: number };
  anomalyLevelDistribution: { [key: string]: number };
  frequencyPatterns: AnomalyFrequencyPatternDto[];
  correlations: AnomalyCorrelationDto[];
  averageDetectionDurationMs: number;
  falsePositiveRate: number;
  analysisSummary: string;
}

export interface AnomalyFrequencyPatternDto {
  patternName: string;
  timeInterval: string; // TimeSpan as string
  frequency: number;
  confidence: number;
}

export interface AnomalyCorrelationDto {
  relatedCanSignalId: string;
  relatedSignalName: string;
  correlationCoefficient: number;
  correlationType: string;
}

export interface ThresholdRecommendationResultDto {
  detectionLogicId: string;
  analysisStartDate: Date;
  analysisEndDate: Date;
  recommendations: ThresholdRecommendationDto[];
  currentMetrics: OptimizationMetricsDto;
  predictedMetrics: OptimizationMetricsDto;
  expectedImprovement: number;
  recommendationSummary: string;
}

export interface ThresholdRecommendationDto {
  parameterName: string;
  currentValue: string;
  recommendedValue: string;
  recommendationReason: string;
  priority: number;
  confidenceLevel: number;
}

export interface OptimizationMetricsDto {
  detectionRate: number;
  falsePositiveRate: number;
  falseNegativeRate: number;
  precision: number;
  recall: number;
  f1Score: number;
  averageDetectionTimeMs: number;
}

export interface DetectionAccuracyMetricsDto {
  detectionLogicId: string;
  analysisStartDate: Date;
  analysisEndDate: Date;
  totalDetections: number;
  truePositives: number;
  falsePositives: number;
  trueNegatives: number;
  falseNegatives: number;
  precision: number;
  recall: number;
  f1Score: number;
  accuracy: number;
  specificity: number;
  averageDetectionTimeMs: number;
  medianDetectionTimeMs: number;
  accuracyByType: AccuracyByAnomalyTypeDto[];
  accuracyByTime: AccuracyByTimeRangeDto[];
  performanceSummary: string;
}

export interface AccuracyByAnomalyTypeDto {
  anomalyType: string;
  truePositives: number;
  falsePositives: number;
  falseNegatives: number;
  precision: number;
  recall: number;
  f1Score: number;
}

export interface AccuracyByTimeRangeDto {
  startTime: Date;
  endTime: Date;
  truePositives: number;
  falsePositives: number;
  falseNegatives: number;
  precision: number;
  recall: number;
  f1Score: number;
}

export interface AnomalyAnalysisRequestDto {
  canSignalId: string;
  analysisStartDate: Date;
  analysisEndDate: Date;
}

export interface ThresholdRecommendationRequestDto {
  detectionLogicId: string;
  analysisStartDate: Date;
  analysisEndDate: Date;
}

export interface DetectionAccuracyRequestDto {
  detectionLogicId: string;
  analysisStartDate: Date;
  analysisEndDate: Date;
}