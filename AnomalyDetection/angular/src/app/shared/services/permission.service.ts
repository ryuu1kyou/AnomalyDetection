import { Injectable } from '@angular/core';
import { Observable, map, distinctUntilChanged } from 'rxjs';
import { ConfigStateService, PermissionService as AbpPermissionService } from '@abp/ng.core';
import {
  PERMISSIONS,
  hasPermission,
  hasAnyPermission,
  hasAllPermissions,
} from '../constants/permissions';

@Injectable({
  providedIn: 'root',
})
export class PermissionService {
  constructor(
    private configState: ConfigStateService,
    private abpPermissionService: AbpPermissionService
  ) {}

  /**
   * Check if current user has a specific permission
   */
  hasPermission(permission: string): boolean {
    return this.abpPermissionService.getGrantedPolicy(permission);
  }

  /**
   * Check if current user has any of the specified permissions
   */
  hasAnyPermission(permissions: string[]): boolean {
    return permissions.some(permission => this.abpPermissionService.getGrantedPolicy(permission));
  }

  /**
   * Check if current user has all specified permissions
   */
  hasAllPermissions(permissions: string[]): boolean {
    return permissions.every(permission => this.abpPermissionService.getGrantedPolicy(permission));
  }

  /**
   * Observable that emits true if user has the specified permission
   */
  hasPermission$(permission: string): Observable<boolean> {
    return this.abpPermissionService.getGrantedPolicy$(permission);
  }

  /**
   * Observable that emits true if user has any of the specified permissions
   */
  hasAnyPermission$(permissions: string[]): Observable<boolean> {
    return this.configState.getAll$().pipe(
      map(() =>
        permissions.some(permission => this.abpPermissionService.getGrantedPolicy(permission))
      ),
      distinctUntilChanged()
    );
  }

  /**
   * Observable that emits true if user has all specified permissions
   */
  hasAllPermissions$(permissions: string[]): Observable<boolean> {
    return this.configState.getAll$().pipe(
      map(() =>
        permissions.every(permission => this.abpPermissionService.getGrantedPolicy(permission))
      ),
      distinctUntilChanged()
    );
  }

  /**
   * Permission constants for easy access
   */
  get permissions() {
    return PERMISSIONS;
  }

  // Convenience methods for specific module permissions
  canManageCanSignals(): boolean {
    return this.hasPermission(PERMISSIONS.CAN_SIGNALS.DEFAULT);
  }

  canCreateCanSignals(): boolean {
    return this.hasPermission(PERMISSIONS.CAN_SIGNALS.CREATE);
  }

  canManageDetectionLogics(): boolean {
    return this.hasPermission(PERMISSIONS.DETECTION_LOGICS.DEFAULT);
  }

  canExecuteDetectionLogics(): boolean {
    return this.hasPermission(PERMISSIONS.DETECTION_LOGICS.EXECUTE);
  }

  canManageProjects(): boolean {
    return this.hasPermission(PERMISSIONS.PROJECTS.DEFAULT);
  }

  canManageProjectMembers(): boolean {
    return this.hasPermission(PERMISSIONS.PROJECTS.MANAGE_MEMBERS);
  }

  canAccessOemTraceability(): boolean {
    return this.hasPermission(PERMISSIONS.OEM_TRACEABILITY.DEFAULT);
  }

  canApproveCustomizations(): boolean {
    return this.hasPermission(PERMISSIONS.OEM_TRACEABILITY.APPROVE_CUSTOMIZATION);
  }

  canAccessSimilarComparison(): boolean {
    return this.hasPermission(PERMISSIONS.SIMILAR_COMPARISON.DEFAULT);
  }

  canAccessAnomalyAnalysis(): boolean {
    return this.hasPermission(PERMISSIONS.ANOMALY_ANALYSIS.DEFAULT);
  }
}
