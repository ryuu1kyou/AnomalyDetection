import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';

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
  // TODO: Uncomment when can-signals routes are implemented
  // {
  //   path: 'can-signals',
  //   loadChildren: () => import('./can-signals/can-signals.routes').then(r => r.CAN_SIGNALS_ROUTES),
  //   canActivate: [authGuard, permissionGuard],
  //   data: {
  //     requiredPolicy: 'AnomalyDetection.CanSignals'
  //   }
  // },
  // TODO: Uncomment when detection-logics routes are implemented
  // {
  //   path: 'detection-logics',
  //   loadChildren: () => import('./detection-logics/detection-logics.routes').then(r => r.DETECTION_LOGICS_ROUTES),
  //   canActivate: [authGuard, permissionGuard],
  //   data: {
  //     requiredPolicy: 'AnomalyDetection.DetectionLogics'
  //   }
  // },
  {
    path: 'detection-results',
    loadChildren: () => import('./detection-results/detection-results.routes').then(r => r.detectionResultsRoutes),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'AnomalyDetection.DetectionResults'
    }
  },
  {
    path: 'projects',
    loadChildren: () => import('./projects/projects.routes').then(r => r.projectsRoutes),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'AnomalyDetection.Projects'
    }
  },
  {
    path: 'anomaly-analysis',
    loadChildren: () => import('./anomaly-analysis/anomaly-analysis.routes').then(r => r.ANOMALY_ANALYSIS_ROUTES),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'AnomalyDetection.AnomalyAnalysis'
    }
  },
  {
    path: 'oem-traceability',
    loadChildren: () => import('./oem-traceability/oem-traceability.routes').then(r => r.OEM_TRACEABILITY_ROUTES),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'AnomalyDetection.OemTraceability'
    }
  },
  {
    path: 'similar-comparison',
    loadChildren: () => import('./similar-comparison/similar-comparison.routes').then(r => r.SIMILAR_COMPARISON_ROUTES),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'AnomalyDetection.SimilarComparison'
    }
  },
  // TODO: Uncomment when dashboard component is implemented
  // {
  //   path: 'dashboard',
  //   loadComponent: () => import('./dashboard/dashboard.component').then(c => c.DashboardComponent),
  //   canActivate: [authGuard]
  // }
];
