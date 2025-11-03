export enum DetectionType {
  OutOfRange = 1,
  RateOfChange = 2,
  Timeout = 3,
  Stuck = 4,
  Periodic = 5,
  Custom = 99,
}

export enum DetectionLogicStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Rejected = 3,
  Deprecated = 4,
}

export enum SharingLevel {
  Private = 0,
  OemPartner = 1,
  Industry = 2,
  Public = 3,
}

export enum AsilLevel {
  QM = 0,
  A = 1,
  B = 2,
  C = 3,
  D = 4,
}

export enum ParameterDataType {
  String = 1,
  Integer = 2,
  Double = 3,
  Boolean = 4,
  DateTime = 5,
  Json = 6,
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
  Fuel = 20,
}

export interface OemCodeDto {
  code?: string;
  name?: string;
}

export interface DetectionParameterDto {
  id: string;
  name: string;
  dataType: ParameterDataType;
  value?: string;
  defaultValue?: string;
  description?: string;
  isRequired?: boolean;
  unit?: string;
  minValue?: number | null;
  maxValue?: number | null;
  minLength?: number | null;
  maxLength?: number | null;
  pattern?: string;
  allowedValues?: string;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface CanSignalMappingDto {
  canSignalId: string;
  signalRole: string;
  isRequired: boolean;
  description?: string;
  scalingFactor?: number | null;
  offset?: number | null;
  filterExpression?: string;
  customProperties?: Record<string, unknown> | null;
  createdAt?: string;
  updatedAt?: string | null;
  signalName?: string;
  canId?: string;
  systemType?: CanSystemType;
}

export interface CanAnomalyDetectionLogicDto {
  id: string;
  name: string;
  version?: string;
  detectionType: DetectionType;
  description?: string;
  purpose?: string;
  detectionTypeName?: string;
  logicContent?: string;
  algorithm?: string;
  isExecutable?: boolean;
  asilLevel: AsilLevel;
  safetyRequirementId?: string;
  safetyGoalId?: string;
  status: DetectionLogicStatus;
  sharingLevel: SharingLevel;
  sourceLogicId?: string | null;
  vehiclePhaseId?: string | null;
  approvedAt?: string | null;
  approvedBy?: string | null;
  approvalNotes?: string | null;
  executionCount?: number;
  lastExecutedAt?: string | null;
  lastExecutionTimeMs?: number | null;
  creationTime?: string;
  lastModificationTime?: string | null;
  createdBy?: string | null;
  lastModifiedBy?: string | null;
  oemCode?: OemCodeDto | null;
  parameters?: DetectionParameterDto[];
  signalMappings?: CanSignalMappingDto[];
}

export interface GetDetectionLogicsQuery {
  skipCount?: number;
  maxResultCount?: number;
  sorting?: string;
  filter?: string;
  detectionType?: DetectionType;
  status?: DetectionLogicStatus;
  asilLevel?: AsilLevel;
  sharingLevel?: SharingLevel;
}

export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
}

const detectionTypeLabelMap: Record<number, string> = {
  [DetectionType.OutOfRange]: 'しきい値逸脱 (Out of Range)',
  [DetectionType.RateOfChange]: '変化率監視 (Rate of Change)',
  [DetectionType.Timeout]: 'タイムアウト (Timeout)',
  [DetectionType.Stuck]: '値張り付き (Stuck Value)',
  [DetectionType.Periodic]: '周期監視 (Periodic)',
  [DetectionType.Custom]: 'カスタム',
};

const detectionLogicStatusLabelMap: Record<number, string> = {
  [DetectionLogicStatus.Draft]: 'ドラフト',
  [DetectionLogicStatus.PendingApproval]: '承認待ち',
  [DetectionLogicStatus.Approved]: '承認済み',
  [DetectionLogicStatus.Rejected]: '却下',
  [DetectionLogicStatus.Deprecated]: '廃止',
};

const sharingLevelLabelMap: Record<number, string> = {
  [SharingLevel.Private]: 'テナント内共有',
  [SharingLevel.OemPartner]: 'OEM パートナー共有',
  [SharingLevel.Industry]: '業界共有',
  [SharingLevel.Public]: '公開',
};

const asilLevelLabelMap: Record<number, string> = {
  [AsilLevel.QM]: 'QM',
  [AsilLevel.A]: 'ASIL A',
  [AsilLevel.B]: 'ASIL B',
  [AsilLevel.C]: 'ASIL C',
  [AsilLevel.D]: 'ASIL D',
};

const parameterDataTypeLabelMap: Record<number, string> = {
  [ParameterDataType.String]: '文字列',
  [ParameterDataType.Integer]: '整数',
  [ParameterDataType.Double]: '数値',
  [ParameterDataType.Boolean]: '真偽値',
  [ParameterDataType.DateTime]: '日時',
  [ParameterDataType.Json]: 'JSON',
};

export function getDetectionTypeLabel(type?: DetectionType | null): string {
  if (type === undefined || type === null) {
    return '未分類';
  }
  return detectionTypeLabelMap[type] ?? `タイプ ${type}`;
}

export function getDetectionLogicStatusLabel(status?: DetectionLogicStatus | null): string {
  if (status === undefined || status === null) {
    return '状態不明';
  }
  return detectionLogicStatusLabelMap[status] ?? `状態 ${status}`;
}

export function getSharingLevelLabel(level?: SharingLevel | null): string {
  if (level === undefined || level === null) {
    return '未設定';
  }
  return sharingLevelLabelMap[level] ?? `レベル ${level}`;
}

export function getAsilLevelLabel(level?: AsilLevel | null): string {
  if (level === undefined || level === null) {
    return '未設定';
  }
  return asilLevelLabelMap[level] ?? `ASIL ${level}`;
}

export function getParameterDataTypeLabel(type?: ParameterDataType | null): string {
  if (type === undefined || type === null) {
    return '未設定';
  }
  return parameterDataTypeLabelMap[type] ?? `型 ${type}`;
}

export function getCanSystemTypeLabel(systemType?: CanSystemType | null): string {
  if (systemType === undefined || systemType === null) {
    return '未分類';
  }
  const name = CanSystemType[systemType];
  if (!name) {
    return `タイプ ${systemType}`;
  }
  return name;
}
