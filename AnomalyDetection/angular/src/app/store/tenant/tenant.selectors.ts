import { createFeatureSelector, createSelector } from '@ngrx/store';
import { TenantState } from './tenant.state';

export const selectTenantState = createFeatureSelector<TenantState>('tenant');

// Current Tenant Selectors
export const selectCurrentTenant = createSelector(
  selectTenantState,
  (state) => state.currentTenant
);

export const selectCurrentTenantLoaded = createSelector(
  selectTenantState,
  (state) => state.currentTenantLoaded
);

// Available Tenants Selectors
export const selectAvailableTenants = createSelector(
  selectTenantState,
  (state) => state.availableTenants
);

export const selectAvailableTenantsLoaded = createSelector(
  selectTenantState,
  (state) => state.availableTenantsLoaded
);

// Tenant Management Selectors
export const selectTenants = createSelector(
  selectTenantState,
  (state) => state.tenants
);

export const selectTenantsTotalCount = createSelector(
  selectTenantState,
  (state) => state.totalCount
);

export const selectTenantsLoaded = createSelector(
  selectTenantState,
  (state) => state.tenantsLoaded
);

// UI State Selectors
export const selectTenantLoading = createSelector(
  selectTenantState,
  (state) => state.loading
);

export const selectTenantError = createSelector(
  selectTenantState,
  (state) => state.error
);

// Computed Selectors
export const selectIsHostTenant = createSelector(
  selectCurrentTenant,
  (currentTenant) => !currentTenant?.tenantId
);

export const selectCurrentOemCode = createSelector(
  selectCurrentTenant,
  (currentTenant) => currentTenant?.oemCode || 'HOST'
);

export const selectActiveTenants = createSelector(
  selectTenants,
  (tenants) => tenants.filter(t => t.isActive && t.isValidForUse)
);

export const selectExpiredTenants = createSelector(
  selectTenants,
  (tenants) => tenants.filter(t => t.isExpired)
);