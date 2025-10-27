import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, catchError, switchMap, tap } from 'rxjs/operators';
import { TenantService } from '../../shared/services/tenant.service';
import * as TenantActions from './tenant.actions';

@Injectable()
export class TenantEffects {

  constructor(
    private actions$: Actions,
    private tenantService: TenantService
  ) {}

  loadCurrentTenant$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TenantActions.loadCurrentTenant),
      switchMap(() =>
        this.tenantService.getCurrentTenant().pipe(
          map(tenant => TenantActions.loadCurrentTenantSuccess({ tenant })),
          catchError(error => of(TenantActions.loadCurrentTenantFailure({ error })))
        )
      )
    )
  );

  loadAvailableTenants$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TenantActions.loadAvailableTenants),
      switchMap(() =>
        this.tenantService.getAvailableTenants().pipe(
          map(tenants => TenantActions.loadAvailableTenantsSuccess({ tenants })),
          catchError(error => of(TenantActions.loadAvailableTenantsFailure({ error })))
        )
      )
    )
  );

  switchTenant$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TenantActions.switchTenant),
      switchMap(({ tenantId }) =>
        this.tenantService.switchTenant(tenantId).pipe(
          map(() => TenantActions.switchTenantSuccess()),
          catchError(error => of(TenantActions.switchTenantFailure({ error })))
        )
      )
    )
  );

  loadTenants$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TenantActions.loadTenants),
      switchMap(({ skipCount, maxResultCount, sorting }) =>
        this.tenantService.getList({ skipCount, maxResultCount, sorting }).pipe(
          map(result => TenantActions.loadTenantsSuccess({ 
            tenants: result.items, 
            totalCount: result.totalCount 
          })),
          catchError(error => of(TenantActions.loadTenantsFailure({ error })))
        )
      )
    )
  );

  createTenant$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TenantActions.createTenant),
      switchMap(({ tenant }) =>
        this.tenantService.create(tenant).pipe(
          map(createdTenant => TenantActions.createTenantSuccess({ tenant: createdTenant })),
          catchError(error => of(TenantActions.createTenantFailure({ error })))
        )
      )
    )
  );

  updateTenant$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TenantActions.updateTenant),
      switchMap(({ id, tenant }) =>
        this.tenantService.update(id, tenant).pipe(
          map(updatedTenant => TenantActions.updateTenantSuccess({ tenant: updatedTenant })),
          catchError(error => of(TenantActions.updateTenantFailure({ error })))
        )
      )
    )
  );

  deleteTenant$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TenantActions.deleteTenant),
      switchMap(({ id }) =>
        this.tenantService.delete(id).pipe(
          map(() => TenantActions.deleteTenantSuccess({ id })),
          catchError(error => of(TenantActions.deleteTenantFailure({ error })))
        )
      )
    )
  );
}