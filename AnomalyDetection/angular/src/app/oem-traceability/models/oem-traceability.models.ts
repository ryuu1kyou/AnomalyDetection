export interface OemTraceabilityResult {
  entityId: string;
  entityType: string;
  oemUsages: OemUsageInfo[];
  crossOemDifferences: CrossOemDifferencesAnalysis;
}

export interface OemUsageInfo {
  oemCode: string;
  usageCount: number;
  vehicles: string[];
  customizationHistory: OemCustomizationSummary[];
  approvalRecords: OemApprovalSummary[];
}

export interface OemCustomizationSummary {
  id: string;
  type: CustomizationType;
  status: CustomizationStatus;
  createdAt: Date;
  reason: string;
}

export interface OemApprovalSummary {
  id: string;
  type: ApprovalType;
  status: ApprovalStatus;
  requestedAt: Date;
  approvedAt?: Date;
  reason: string;
}

export interface CrossOemDifferencesAnalysis {
  parameterDifferences: { [key: string]: OemParameterDifference[] };
  usagePatternDifferences: UsagePatternDifference[];
  recommendations: string[];
}

export interface OemParameterDifference {
  oemCode: string;
  parameterName: string;
  originalValue: any;
  customValue: any;
  differencePercentage: number;
  differenceDescription: string;
}

export interface UsagePatternDifference {
  oemCode: string;
  patternType: string;
  description: string;
  frequency: number;
  impact: string;
}

export interface OemCustomization {
  id: string;
  entityId: string;
  entityType: string;
  oemCode: string;
  type: CustomizationType;
  customParameters: { [key: string]: any };
  originalParameters: { [key: string]: any };
  customizationReason: string;
  approvedBy?: string;
  approvedAt?: Date;
  status: CustomizationStatus;
  approvalNotes?: string;
  creationTime: Date;
}

export interface OemApproval {
  id: string;
  entityId: string;
  entityType: string;
  oemCode: string;
  type: ApprovalType;
  requestedBy: string;
  requestedAt: Date;
  approvedBy?: string;
  approvedAt?: Date;
  status: ApprovalStatus;
  approvalReason: string;
  approvalNotes?: string;
  approvalData: { [key: string]: any };
  dueDate?: Date;
  priority: number;
  isOverdue: boolean;
  isUrgent: boolean;
}

export interface CreateOemCustomization {
  entityId: string;
  entityType: string;
  oemCode: string;
  type: CustomizationType;
  customParameters: { [key: string]: any };
  originalParameters: { [key: string]: any };
  customizationReason: string;
}

export interface CreateOemApproval {
  entityId: string;
  entityType: string;
  oemCode: string;
  type: ApprovalType;
  approvalReason: string;
  approvalData: { [key: string]: any };
  dueDate?: Date;
  priority: number;
}

export enum CustomizationType {
  ParameterAdjustment = 1,
  ThresholdChange = 2,
  LogicModification = 3,
  SpecificationChange = 4,
  Other = 99
}

export enum CustomizationStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Rejected = 3,
  Obsolete = 4
}

export enum ApprovalType {
  NewEntity = 1,
  Modification = 2,
  Customization = 3,
  Inheritance = 4,
  Sharing = 5,
  Deletion = 6
}

export enum ApprovalStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Cancelled = 3
}

export interface OemTraceabilityReport {
  reportId: string;
  fileName: string;
  contentType: string;
  content: Uint8Array;
  generatedAt: Date;
  generatedBy: string;
}

export interface GenerateOemTraceabilityReport {
  entityId?: string;
  entityType?: string;
  oemCode?: string;
  startDate?: Date;
  endDate?: Date;
  includeSections: string[];
  reportFormat: string;
}