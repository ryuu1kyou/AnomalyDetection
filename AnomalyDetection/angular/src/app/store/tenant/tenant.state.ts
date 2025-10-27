import { TenantSwitchDto, ExtendedTenantDto } from '../../shared/models/tenant.model';

export interface TenantState {
  // Current tenant context
  currentTenant: TenantSwitchDto | null;
  availableTenants: TenantSwitchDto[];
  
  // Tenant management
  tenants: ExtendedTenantDto[];
  totalCount: number;
  
  // UI state
  loading: boolean;
  error: any;
  
  // Flags
  currentTenantLoaded: boolean;
  availableTenantsLoaded: boolean;
  tenantsLoaded: boolean;
}

export const initialTenantState: TenantState = {
  currentTenant: null,
  availableTenants: [],
  tenants: [],
  totalCount: 0,
  loading: false,
  error: null,
  currentTenantLoaded: false,
  availableTenantsLoaded: false,
  tenantsLoaded: false
};