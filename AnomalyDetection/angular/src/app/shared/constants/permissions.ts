/**
 * Permission constants for the Anomaly Detection System
 * These should match the permissions defined in the backend
 */
export const PERMISSIONS = {
  // CAN Signals permissions
  CAN_SIGNALS: {
    DEFAULT: 'AnomalyDetection.CanSignals',
    CREATE: 'AnomalyDetection.CanSignals.Create',
    EDIT: 'AnomalyDetection.CanSignals.Edit',
    DELETE: 'AnomalyDetection.CanSignals.Delete',
    VIEW: 'AnomalyDetection.CanSignals.View'
  },

  // Detection Logics permissions
  DETECTION_LOGICS: {
    DEFAULT: 'AnomalyDetection.DetectionLogics',
    CREATE: 'AnomalyDetection.DetectionLogics.Create',
    EDIT: 'AnomalyDetection.DetectionLogics.Edit',
    DELETE: 'AnomalyDetection.DetectionLogics.Delete',
    VIEW: 'AnomalyDetection.DetectionLogics.View',
    EXECUTE: 'AnomalyDetection.DetectionLogics.Execute'
  },

  // Detection Results permissions
  DETECTION_RESULTS: {
    DEFAULT: 'AnomalyDetection.DetectionResults',
    VIEW: 'AnomalyDetection.DetectionResults.View',
    EDIT: 'AnomalyDetection.DetectionResults.Edit',
    DELETE: 'AnomalyDetection.DetectionResults.Delete'
  },

  // Projects permissions
  PROJECTS: {
    DEFAULT: 'AnomalyDetection.Projects',
    CREATE: 'AnomalyDetection.Projects.Create',
    EDIT: 'AnomalyDetection.Projects.Edit',
    DELETE: 'AnomalyDetection.Projects.Delete',
    VIEW: 'AnomalyDetection.Projects.View',
    MANAGE_MEMBERS: 'AnomalyDetection.Projects.ManageMembers'
  },

  // OEM Traceability permissions
  OEM_TRACEABILITY: {
    DEFAULT: 'AnomalyDetection.OemTraceability',
    CREATE_CUSTOMIZATION: 'AnomalyDetection.OemTraceability.CreateCustomization',
    APPROVE_CUSTOMIZATION: 'AnomalyDetection.OemTraceability.ApproveCustomization',
    VIEW_TRACEABILITY: 'AnomalyDetection.OemTraceability.ViewTraceability'
  },

  // Similar Comparison permissions
  SIMILAR_COMPARISON: {
    DEFAULT: 'AnomalyDetection.SimilarComparison',
    SEARCH_SIGNALS: 'AnomalyDetection.SimilarComparison.SearchSignals',
    COMPARE_DATA: 'AnomalyDetection.SimilarComparison.CompareData',
    VIEW_ANALYSIS: 'AnomalyDetection.SimilarComparison.ViewAnalysis'
  },

  // Anomaly Analysis permissions
  ANOMALY_ANALYSIS: {
    DEFAULT: 'AnomalyDetection.AnomalyAnalysis',
    ANALYZE_PATTERNS: 'AnomalyDetection.AnomalyAnalysis.AnalyzePatterns',
    GENERATE_RECOMMENDATIONS: 'AnomalyDetection.AnomalyAnalysis.GenerateRecommendations',
    VIEW_METRICS: 'AnomalyDetection.AnomalyAnalysis.ViewMetrics'
  }
} as const;

/**
 * Helper function to check if user has required permission
 */
export function hasPermission(userPermissions: string[], requiredPermission: string): boolean {
  return userPermissions.includes(requiredPermission);
}

/**
 * Helper function to check if user has any of the required permissions
 */
export function hasAnyPermission(userPermissions: string[], requiredPermissions: string[]): boolean {
  return requiredPermissions.some(permission => userPermissions.includes(permission));
}

/**
 * Helper function to check if user has all required permissions
 */
export function hasAllPermissions(userPermissions: string[], requiredPermissions: string[]): boolean {
  return requiredPermissions.every(permission => userPermissions.includes(permission));
}