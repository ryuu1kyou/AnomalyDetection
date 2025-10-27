import { createReducer, on } from '@ngrx/store';
import { TenantState, initialTenantState } from './tenant.state';
import * as TenantActions from './tenant.actions';

export const tenantReducer = createReducer(
  initialTenantState,

  // Current Tenant
  on(TenantActions.loadCurrentTenant, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(TenantActions.loadCurrentTenantSuccess, (state, { tenant }) => ({
    ...state,
    currentTenant: tenant,
    loading: false,
    currentTenantLoaded: true,
    error: null
  })),
  on(TenantActions.loadCurrentTenantFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
    currentTenantLoaded: false
  })),

  // Available Tenants
  on(TenantActions.loadAvailableTenants, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(TenantActions.loadAvailableTenantsSuccess, (state, { tenants }) => ({
    ...state,
    availableTenants: tenants,
    loading: false,
    availableTenantsLoaded: true,
    error: null
  })),
  on(TenantActions.loadAvailableTenantsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
    availableTenantsLoaded: false
  })),

  // Switch Tenant
  on(TenantActions.switchTenant, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(TenantActions.switchTenantSuccess, (state) => ({
    ...state,
    loading: false,
    error: null
  })),
  on(TenantActions.switchTenantFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Tenant Management
  on(TenantActions.loadTenants, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(TenantActions.loadTenantsSuccess, (state, { tenants, totalCount }) => ({
    ...state,
    tenants,
    totalCount,
    loading: false,
    tenantsLoaded: true,
    error: null
  })),
  on(TenantActions.loadTenantsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
    tenantsLoaded: false
  })),

  // Create Tenant
  on(TenantActions.createTenant, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(TenantActions.createTenantSuccess, (state, { tenant }) => ({
    ...state,
    tenants: [...state.tenants, tenant],
    totalCount: state.totalCount + 1,
    loading: false,
    error: null
  })),
  on(TenantActions.createTenantFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Update Tenant
  on(TenantActions.updateTenant, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(TenantActions.updateTenantSuccess, (state, { tenant }) => ({
    ...state,
    tenants: state.tenants.map(t => t.id === tenant.id ? tenant : t),
    loading: false,
    error: null
  })),
  on(TenantActions.updateTenantFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Delete Tenant
  on(TenantActions.deleteTenant, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(TenantActions.deleteTenantSuccess, (state, { id }) => ({
    ...state,
    tenants: state.tenants.filter(t => t.id !== id),
    totalCount: state.totalCount - 1,
    loading: false,
    error: null
  })),
  on(TenantActions.deleteTenantFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // UI Actions
  on(TenantActions.setLoading, (state, { loading }) => ({
    ...state,
    loading
  })),
  on(TenantActions.clearError, (state) => ({
    ...state,
    error: null
  }))
);