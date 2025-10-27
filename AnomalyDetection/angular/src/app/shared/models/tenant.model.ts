export interface OemMasterDto {
  id: string;
  oemCode: string;
  oemName: string;
  companyName: string;
  country: string;
  contactEmail?: string;
  contactPhone?: string;
  isActive: boolean;
  establishedDate?: string;
  description?: string;
  features: OemFeatureDto[];
  creationTime: string;
  lastModificationTime?: string;
}

export interface CreateOemMasterDto {
  oemCode: string;
  oemName: string;
  companyName: string;
  country: string;
  contactEmail?: string;
  contactPhone?: string;
  establishedDate?: string;
  description?: string;
}

export interface UpdateOemMasterDto {
  companyName: string;
  country: string;
  contactEmail?: string;
  contactPhone?: string;
  description?: string;
}

export interface OemFeatureDto {
  featureName: string;
  featureValue: string;
  isEnabled: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface ExtendedTenantDto {
  id: string;
  name: string;
  oemCode: string;
  oemName: string;
  oemMasterId?: string;
  isActive: boolean;
  activationDate?: string;
  expirationDate?: string;
  description?: string;
  features: TenantFeatureDto[];
  isExpired: boolean;
  isValidForUse: boolean;
  creationTime: string;
  lastModificationTime?: string;
}

export interface CreateExtendedTenantDto {
  name: string;
  oemCode: string;
  oemName: string;
  oemMasterId?: string;
  databaseConnectionString?: string;
  description?: string;
  expirationDate?: string;
}

export interface UpdateExtendedTenantDto {
  name: string;
  description?: string;
  expirationDate?: string;
}

export interface TenantFeatureDto {
  featureName: string;
  featureValue: string;
  isEnabled: boolean;
  createdAt: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface TenantSwitchDto {
  tenantId?: string;
  tenantName: string;
  oemCode: string;
}

export interface PagedResultDto<T> {
  totalCount: number;
  items: T[];
}

export interface PagedAndSortedResultRequestDto {
  skipCount: number;
  maxResultCount: number;
  sorting?: string;
}

export interface ListResultDto<T> {
  items: T[];
}