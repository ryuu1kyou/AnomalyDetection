import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';
import { PERMISSIONS } from './shared/constants/permissions';

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(c => c.HomeComponent),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(c => c.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(c => c.createRoutes()),
  },
  {
    path: 'tenant-management',
    loadChildren: () => import('@abp/ng.tenant-management').then(c => c.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () => import('@abp/ng.setting-management').then(c => c.createRoutes()),
  },
  {
    path: 'can-signals',
    loadChildren: () => import('./can-signals/can-signals.routes').then(r => r.CAN_SIGNALS_ROUTES),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.CAN_SIGNALS.DEFAULT
    }
  },
  {
    path: 'detection-logics',
    loadChildren: () => import('./detection-logics/detection-logics.routes').then(r => r.DETECTION_LOGICS_ROUTES),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.DETECTION_LOGICS.DEFAULT
    }
  },
  {
    path: 'detection-results',
    loadChildren: () => import('./detection-results/detection-results.routes').then(r => r.detectionResultsRoutes),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.DETECTION_RESULTS.DEFAULT
    }
  },
  {
    path: 'projects',
    loadChildren: () => import('./projects/projects.routes').then(r => r.projectsRoutes),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.PROJECTS.DEFAULT
    }
  },
  {
    path: 'anomaly-analysis',
    loadChildren: () => import('./anomaly-analysis/anomaly-analysis.routes').then(r => r.ANOMALY_ANALYSIS_ROUTES),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.ANOMALY_ANALYSIS.DEFAULT
    }
  },
  {
    path: 'oem-traceability',
    loadChildren: () => import('./oem-traceability/oem-traceability.routes').then(r => r.OEM_TRACEABILITY_ROUTES),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.OEM_TRACEABILITY.DEFAULT
    }
  },
  {
    path: 'similar-comparison',
    loadChildren: () => import('./similar-comparison/similar-comparison.routes').then(r => r.SIMILAR_COMPARISON_ROUTES),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.SIMILAR_COMPARISON.DEFAULT
    }
  },
  {
    path: 'dashboard',
    loadChildren: () => import('./dashboard/dashboard.routes').then(r => r.dashboardRoutes),
    canActivate: [authGuard]
  }
];
