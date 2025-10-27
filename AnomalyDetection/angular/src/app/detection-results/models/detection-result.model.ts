export enum AnomalyLevel {
  Info = 1,
  Warning = 2,
  Error = 3,
  Critical = 4,
  Fatal = 5
}

export enum ResolutionStatus {
  Open = 1,
  Investigating = 2,
  InProgress = 3,
  Resolved = 4,
  FalsePositive = 5,
  Ignored = 6,
  Reopened = 7,
  Escalated = 8
}

export enum SharingLevel {
  Private = 0,
  OemPartner = 1,
  Industry = 2,
  Public = 3
}

export enum DetectionType {
  OutOfRange = 1,
  RateOfChange = 2,
  Timeout = 3,
  Stuck = 4,
  Pattern = 5,
  Statistical = 6,
  Custom = 7
}

export enum CanSystemType {
  Engine = 1,
  Brake = 2,
  Steering = 3,
  Transmission = 4,
  Body = 5,
  Chassis = 6,
  HVAC = 7,
  Lighting = 8,
  Infotainment = 9,
  Safety = 10,
  Powertrain = 11,
  Gateway = 12,
  Battery = 13,
  Motor = 14,
  Inverter = 15,
  Charger = 16,
  ADAS = 17,
  Suspension = 18,
  Exhaust = 19,
  Fuel = 20
}

export interface AnomalyDetectionResult {
  id: string;
  tenantId?: string;
  
  // Related Entities
  detectionLogicId: string;
  canSignalId: string;
  
  // Detection Result Basic Information
  detectedAt: Date;
  anomalyLevel: AnomalyLevel;
  confidenceScore: number;
  description: string;
  
  // Input Data
  signalValue: number;
  inputTimestamp: Date;
  additionalInputData: Record<string, any>;
  
  // Detection Details
  detectionType: DetectionType;
  triggerCondition: string;
  detectionParameters: Record<string, any>;
  executionTimeMs: number;
  
  // Resolution Status
  resolutionStatus: ResolutionStatus;
  resolvedAt?: Date;
  resolvedBy?: string;
  resolutionNotes: string;
  
  // Sharing Settings
  sharingLevel: SharingLevel;
  isShared: boolean;
  sharedAt?: Date;
  sharedBy?: string;
  
  // Related Information (for display purposes)
  detectionLogicName: string;
  signalName: string;
  canId: string;
  systemType: CanSystemType;
  resolvedByUserName: string;
  sharedByUserName: string;
  
  // Audit fields
  creationTime: Date;
  creatorId?: string;
  lastModificationTime?: Date;
  lastModifierId?: string;
}

export interface GetDetectionResultsInput {
  filter?: string;
  detectionLogicId?: string;
  canSignalId?: string;
  anomalyLevel?: AnomalyLevel;
  resolutionStatus?: ResolutionStatus;
  sharingLevel?: SharingLevel;
  detectionType?: DetectionType;
  systemType?: CanSystemType;
  detectedFrom?: Date;
  detectedTo?: Date;
  resolvedFrom?: Date;
  resolvedTo?: Date;
  minConfidenceScore?: number;
  maxConfidenceScore?: number;
  isShared?: boolean;
  isHighPriority?: boolean;
  maxAge?: number;
  
  // Pagination
  skipCount?: number;
  maxResultCount?: number;
  sorting?: string;
}

export interface MarkAsFalsePositiveDto {
  reason: string;
  notes?: string;
}

export interface ReopenDetectionResultDto {
  reason: string;
  notes?: string;
}

export interface ShareDetectionResultDto {
  sharingLevel: SharingLevel;
  shareReason?: string;
  shareNotes?: string;
  requireApproval?: boolean;
}

export interface ResolveDetectionResultDto {
  resolutionNotes: string;
  resolutionType?: string;
}