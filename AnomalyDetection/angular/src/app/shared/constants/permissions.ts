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

  // Detection Templates permissions (Req4)
  DETECTION_TEMPLATES: {
    DEFAULT: 'AnomalyDetection.DetectionTemplates',
    VIEW: 'AnomalyDetection.DetectionTemplates.View',
    MANAGE: 'AnomalyDetection.DetectionTemplates.Manage',
    CREATE_FROM: 'AnomalyDetection.DetectionTemplates.CreateFrom'
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
  ,

  // Safety Trace permissions (dashboard & records)
  SAFETY_TRACE: {
    DEFAULT: 'AnomalyDetection.SafetyTrace',
    AUDIT: {
      DEFAULT: 'AnomalyDetection.SafetyTrace.Audit',
      VIEW: 'AnomalyDetection.SafetyTrace.Audit.View',
      EXPORT: 'AnomalyDetection.SafetyTrace.Audit.Export'
    },
    RECORDS: {
      DEFAULT: 'AnomalyDetection.SafetyTrace.Records',
      CREATE: 'AnomalyDetection.SafetyTrace.Records.Create',
      EDIT: 'AnomalyDetection.SafetyTrace.Records.Edit',
      SUBMIT: 'AnomalyDetection.SafetyTrace.Records.Submit',
      APPROVE: 'AnomalyDetection.SafetyTrace.Records.Approve',
      REJECT: 'AnomalyDetection.SafetyTrace.Records.Reject',
      VIEW: 'AnomalyDetection.SafetyTrace.Records.View',
      EXPORT: 'AnomalyDetection.SafetyTrace.Records.Export'
    }
  },

  // CAN Specification permissions (import & diff)
  CAN_SPECIFICATION: {
    DEFAULT: 'AnomalyDetection.CanSpecification',
    IMPORT: 'AnomalyDetection.CanSpecification.Import',
    VIEW: 'AnomalyDetection.CanSpecification.View',
    DIFF: {
      DEFAULT: 'AnomalyDetection.CanSpecification.Diff',
      VIEW: 'AnomalyDetection.CanSpecification.Diff.View',
      EXPORT: 'AnomalyDetection.CanSpecification.Diff.Export'
    }
  },

  // Threshold Optimization permissions
  THRESHOLD_OPTIMIZATION: {
    DEFAULT: 'AnomalyDetection.Analysis.ThresholdOptimization',
    CALCULATE: 'AnomalyDetection.Analysis.ThresholdOptimization.Calculate',
    EXPORT: 'AnomalyDetection.Analysis.ThresholdOptimization.Export'
  },

  // Compatibility Analysis permissions
  COMPATIBILITY_ANALYSIS: {
    DEFAULT: 'AnomalyDetection.Analysis.Compatibility',
    RUN: 'AnomalyDetection.Analysis.Compatibility.Run',
    VIEW: 'AnomalyDetection.Analysis.Compatibility.View',
    EXPORT: 'AnomalyDetection.Analysis.Compatibility.Export'
  },

  // Knowledge Base permissions
  KNOWLEDGE_BASE: {
    DEFAULT: 'AnomalyDetection.KnowledgeBase',
    VIEW: 'AnomalyDetection.KnowledgeBase.View',
    MANAGE: 'AnomalyDetection.KnowledgeBase.Manage',
    STATISTICS: {
      DEFAULT: 'AnomalyDetection.KnowledgeBase.Statistics',
      VIEW: 'AnomalyDetection.KnowledgeBase.Statistics.View',
      EXPORT: 'AnomalyDetection.KnowledgeBase.Statistics.Export'
    }
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
