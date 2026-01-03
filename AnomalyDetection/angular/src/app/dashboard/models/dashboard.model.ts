export interface DashboardStatistics {
  // Overview Statistics
  totalProjects: number;
  activeProjects: number;
  completedProjects: number;
  totalCanSignals: number;
  totalDetectionLogics: number;
  totalAnomalies: number;
  resolvedAnomalies: number;
  
  // Recent Activity
  recentAnomalies: number;
  recentProjects: number;
  
  // Performance Metrics
  averageDetectionTime: number;
  detectionAccuracy: number;
  falsePositiveRate: number;
  
  // System Health
  systemUptime: number;
  activeConnections: number;
  processingQueue: number;
  detectionStatistics?: DetectionStatistics;
}

export interface DetectionStatistics {
  // Time-based statistics
  dailyDetections: TimeSeriesData[];
  weeklyDetections: TimeSeriesData[];
  monthlyDetections: TimeSeriesData[];
  
  // Anomaly level distribution
  anomalyLevelDistribution: CategoryData[];
  
  // System type distribution
  systemTypeDistribution: CategoryData[];
  
  // OEM distribution
  oemDistribution: CategoryData[];
  
  // Detection type distribution
  detectionTypeDistribution: CategoryData[];
  
  // Resolution status distribution
  resolutionStatusDistribution: CategoryData[];
  
  // Trend analysis
  detectionTrend: TrendData;
  resolutionTrend: TrendData;
}

export interface SystemAnomalyReport {
  systemType: string;
  totalAnomalies: number;
  resolvedAnomalies: number;
  averageResolutionTime: number;
  criticalAnomalies: number;
  recentAnomalies: number;
  topSignals: SignalAnomalyData[];
  trendData: TimeSeriesData[];
}

export interface TimeSeriesData {
  date: string;
  count: number;
  value?: number;
  label?: string;
}

export interface CategoryData {
  category: string;
  value: number;
  percentage: number;
  color?: string;
}

export interface TrendData {
  direction: 'up' | 'down' | 'stable';
  percentage: number;
  period: string;
}

export interface SignalAnomalyData {
  signalName: string;
  canId: string;
  anomalyCount: number;
  lastDetection: Date;
  severity: string;
}

export interface GetDetectionStatisticsInput {
  startDate?: Date;
  endDate?: Date;
  systemType?: string;
  oemCode?: string;
  anomalyLevel?: number;
  groupBy?: 'day' | 'week' | 'month';
}

export interface GenerateSystemReportInput {
  systemType?: string;
  startDate?: Date;
  endDate?: Date;
  includeDetails?: boolean;
  format?: 'pdf' | 'excel' | 'csv';
}

export interface ChartConfiguration {
  type: 'line' | 'bar' | 'pie' | 'doughnut' | 'area';
  data: any;
  options: any;
}

export interface DashboardWidget {
  id: string;
  title: string;
  type: 'chart' | 'metric' | 'table' | 'list';
  size: 'small' | 'medium' | 'large';
  position: { x: number; y: number };
  configuration: any;
  refreshInterval?: number;
}

export interface MetricCard {
  title: string;
  value: number | string;
  unit?: string;
  trend?: TrendData;
  icon?: string;
  color?: string;
  description?: string;
}