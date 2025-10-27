export enum ProjectStatus {
  Planning = 0,
  Active = 1,
  OnHold = 2,
  Completed = 3,
  Cancelled = 4
}

export enum MilestoneStatus {
  NotStarted = 0,
  InProgress = 1,
  Completed = 2,
  Delayed = 3,
  Cancelled = 4
}

export enum ProjectPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4
}

export interface AnomalyDetectionProject {
  id: string;
  tenantId?: string;
  
  // Basic Information
  projectCode: string;
  projectName: string;
  description: string;
  
  // Vehicle Information
  vehicleModel: string;
  modelYear: string;
  platform: string;
  
  // Project Details
  primarySystem: string;
  targetMarket: string;
  status: ProjectStatus;
  priority: ProjectPriority;
  
  // Dates
  startDate: Date;
  plannedEndDate: Date;
  actualEndDate?: Date;
  
  // Progress
  progressPercentage: number;
  
  // OEM Information
  oemCode: string;
  oemName: string;
  
  // Statistics
  totalDetectionLogics: number;
  totalCanSignals: number;
  totalAnomalies: number;
  resolvedAnomalies: number;
  
  // Audit fields
  creationTime: Date;
  creatorId?: string;
  lastModificationTime?: Date;
  lastModifierId?: string;
}

export interface ProjectMilestone {
  id: string;
  projectId: string;
  
  // Basic Information
  name: string;
  description: string;
  
  // Dates
  plannedDate: Date;
  actualDate?: Date;
  
  // Status
  status: MilestoneStatus;
  
  // Progress
  progressPercentage: number;
  
  // Dependencies
  dependencies: string[];
  
  // Deliverables
  deliverables: string[];
  
  // Audit fields
  creationTime: Date;
  creatorId?: string;
  lastModificationTime?: Date;
  lastModifierId?: string;
}

export interface ProjectMember {
  id: string;
  projectId: string;
  
  // User Information
  userId: string;
  userName: string;
  email: string;
  
  // Role Information
  role: string;
  responsibilities: string[];
  
  // Dates
  joinedDate: Date;
  leftDate?: Date;
  
  // Status
  isActive: boolean;
  
  // Permissions
  canEdit: boolean;
  canDelete: boolean;
  canManageMembers: boolean;
  
  // Audit fields
  creationTime: Date;
  creatorId?: string;
  lastModificationTime?: Date;
  lastModifierId?: string;
}

export interface GetProjectsInput {
  filter?: string;
  status?: ProjectStatus;
  priority?: ProjectPriority;
  oemCode?: string;
  primarySystem?: string;
  vehicleModel?: string;
  startDateFrom?: Date;
  startDateTo?: Date;
  endDateFrom?: Date;
  endDateTo?: Date;
  
  // Pagination
  skipCount?: number;
  maxResultCount?: number;
  sorting?: string;
}

export interface CreateProjectDto {
  projectCode: string;
  projectName: string;
  description: string;
  vehicleModel: string;
  modelYear: string;
  platform: string;
  primarySystem: string;
  targetMarket: string;
  status: ProjectStatus;
  priority: ProjectPriority;
  startDate: Date;
  plannedEndDate: Date;
  oemCode: string;
}

export interface UpdateProjectDto {
  projectName: string;
  description: string;
  vehicleModel: string;
  modelYear: string;
  platform: string;
  primarySystem: string;
  targetMarket: string;
  status: ProjectStatus;
  priority: ProjectPriority;
  startDate: Date;
  plannedEndDate: Date;
  actualEndDate?: Date;
  progressPercentage: number;
}

export interface CreateProjectMilestoneDto {
  projectId: string;
  name: string;
  description: string;
  plannedDate: Date;
  dependencies: string[];
  deliverables: string[];
}

export interface UpdateProjectMilestoneDto {
  name: string;
  description: string;
  plannedDate: Date;
  actualDate?: Date;
  status: MilestoneStatus;
  progressPercentage: number;
  dependencies: string[];
  deliverables: string[];
}

export interface CreateProjectMemberDto {
  projectId: string;
  userId: string;
  role: string;
  responsibilities: string[];
  canEdit: boolean;
  canDelete: boolean;
  canManageMembers: boolean;
}

export interface UpdateProjectMemberDto {
  role: string;
  responsibilities: string[];
  canEdit: boolean;
  canDelete: boolean;
  canManageMembers: boolean;
  isActive: boolean;
}

export interface ProjectOperationDto {
  operationType: string;
  operationData: Record<string, any>;
  notes?: string;
}

export interface ProjectStatistics {
  totalProjects: number;
  activeProjects: number;
  completedProjects: number;
  delayedProjects: number;
  averageCompletionTime: number;
  projectsByStatus: Record<ProjectStatus, number>;
  projectsByPriority: Record<ProjectPriority, number>;
  projectsByOem: Record<string, number>;
}