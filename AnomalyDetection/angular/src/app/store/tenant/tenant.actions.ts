import { createAction, props } from '@ngrx/store';
import { TenantSwitchDto, ExtendedTenantDto } from '../../shared/models/tenant.model';

// Current Tenant Actions
export const loadCurrentTenant = createAction('[Tenant] Load Current Tenant');
export const loadCurrentTenantSuccess = createAction(
  '[Tenant] Load Current Tenant Success',
  props<{ tenant: TenantSwitchDto }>()
);
export const loadCurrentTenantFailure = createAction(
  '[Tenant] Load Current Tenant Failure',
  props<{ error: any }>()
);

// Available Tenants Actions
export const loadAvailableTenants = createAction('[Tenant] Load Available Tenants');
export const loadAvailableTenantsSuccess = createAction(
  '[Tenant] Load Available Tenants Success',
  props<{ tenants: TenantSwitchDto[] }>()
);
export const loadAvailableTenantsFailure = createAction(
  '[Tenant] Load Available Tenants Failure',
  props<{ error: any }>()
);

// Switch Tenant Actions
export const switchTenant = createAction(
  '[Tenant] Switch Tenant',
  props<{ tenantId?: string }>()
);
export const switchTenantSuccess = createAction('[Tenant] Switch Tenant Success');
export const switchTenantFailure = createAction(
  '[Tenant] Switch Tenant Failure',
  props<{ error: any }>()
);

// Tenant Management Actions
export const loadTenants = createAction(
  '[Tenant] Load Tenants',
  props<{ skipCount: number; maxResultCount: number; sorting?: string }>()
);
export const loadTenantsSuccess = createAction(
  '[Tenant] Load Tenants Success',
  props<{ tenants: ExtendedTenantDto[]; totalCount: number }>()
);
export const loadTenantsFailure = createAction(
  '[Tenant] Load Tenants Failure',
  props<{ error: any }>()
);

export const createTenant = createAction(
  '[Tenant] Create Tenant',
  props<{ tenant: any }>()
);
export const createTenantSuccess = createAction(
  '[Tenant] Create Tenant Success',
  props<{ tenant: ExtendedTenantDto }>()
);
export const createTenantFailure = createAction(
  '[Tenant] Create Tenant Failure',
  props<{ error: any }>()
);

export const updateTenant = createAction(
  '[Tenant] Update Tenant',
  props<{ id: string; tenant: any }>()
);
export const updateTenantSuccess = createAction(
  '[Tenant] Update Tenant Success',
  props<{ tenant: ExtendedTenantDto }>()
);
export const updateTenantFailure = createAction(
  '[Tenant] Update Tenant Failure',
  props<{ error: any }>()
);

export const deleteTenant = createAction(
  '[Tenant] Delete Tenant',
  props<{ id: string }>()
);
export const deleteTenantSuccess = createAction(
  '[Tenant] Delete Tenant Success',
  props<{ id: string }>()
);
export const deleteTenantFailure = createAction(
  '[Tenant] Delete Tenant Failure',
  props<{ error: any }>()
);

// UI Actions
export const setLoading = createAction(
  '[Tenant] Set Loading',
  props<{ loading: boolean }>()
);
export const clearError = createAction('[Tenant] Clear Error');