export interface SimilaritySearchCriteria {
  compareCanId: boolean;
  compareSignalName: boolean;
  compareSystemType: boolean;
  comparePhysicalRange: boolean;
  minimumSimilarity: number;
  maxResults: number;
  targetCanSignalId?: string;
  targetSignalName?: string;
  targetSystemType?: number;
}

export interface SimilarSignalResult {
  canSignalId: string;
  signalName: string;
  canId: string;
  systemType: number;
  systemTypeName: string;
  similarityScore: number;
  matchedAttributes: string[];
  oemCode: string;
  vehiclePhase?: string;
  description?: string;
}

export interface TestDataComparison {
  sourceSignalId: string;
  sourceSignalName: string;
  targetSignalId: string;
  targetSignalName: string;
  thresholdDifferences: ThresholdDifference[];
  detectionConditionDifferences: string[];
  resultDifferences: string[];
  recommendations: ComparisonRecommendation[];
  overallSimilarity: number;
}

export interface ThresholdDifference {
  parameterName: string;
  sourceValue: number;
  targetValue: number;
  difference: number;
  differencePercentage: number;
  differenceType: DifferenceType;
  impactLevel: ImpactLevel;
}

export interface ComparisonRecommendation {
  type: RecommendationType;
  priority: RecommendationPriority;
  description: string;
  suggestedValue?: any;
  rationale: string;
}

export enum DifferenceType {
  Threshold = 0,
  DetectionCondition = 1,
  Result = 2,
  Parameter = 3
}

export enum ImpactLevel {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export enum RecommendationType {
  AdjustThreshold = 0,
  ModifyCondition = 1,
  ReviewResult = 2,
  UseAsIs = 3,
  RequireValidation = 4
}

export enum RecommendationPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export interface TestDataRecord {
  id: string;
  canSignalId: string;
  canSignalName: string;
  vehiclePhase: string;
  oemCode: string;
  testDate: Date;
  detectionLogicId: string;
  detectionLogicName: string;
  parameters: { [key: string]: any };
  results: TestResult[];
  anomalyCount: number;
  falsePositiveCount: number;
  detectionRate: number;
}

export interface TestResult {
  timestamp: Date;
  inputValue: number;
  detectedAnomaly: boolean;
  anomalyType?: string;
  anomalyLevel?: string;
  confidence: number;
}

export interface TestDataFilter {
  canSignalId?: string;
  vehiclePhase?: string;
  oemCode?: string;
  startDate?: Date;
  endDate?: Date;
  anomalyType?: string;
  minDetectionRate?: number;
  maxDetectionRate?: number;
}

export interface ComparisonVisualizationData {
  timeSeriesData: TimeSeriesPoint[];
  distributionData: DistributionPoint[];
  correlationData: CorrelationPoint[];
}

export interface TimeSeriesPoint {
  timestamp: Date;
  sourceValue: number;
  targetValue: number;
  sourceAnomaly: boolean;
  targetAnomaly: boolean;
}

export interface DistributionPoint {
  value: number;
  sourceFrequency: number;
  targetFrequency: number;
}

export interface CorrelationPoint {
  sourceValue: number;
  targetValue: number;
  anomalyMatch: boolean;
}

export interface ExportFormat {
  format: 'csv' | 'excel' | 'pdf';
  includeCharts: boolean;
  includeRecommendations: boolean;
}
