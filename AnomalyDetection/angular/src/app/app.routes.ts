import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';
import { PERMISSIONS } from './shared/constants/permissions';

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./home/home.component')
        .then(c => {
          console.debug('[Route] home component loaded');
          return c.HomeComponent;
        })
        .catch(err => {
          console.error('[Route] home component load failed', err);
          throw err;
        }),
  },
  {
    path: 'detection-templates',
    loadChildren: () =>
      import('./detection-templates/detection-templates.routes')
        .then(r => {
          console.debug('[Route] detection-templates routes loaded');
          return r.DETECTION_TEMPLATES_ROUTES;
        })
        .catch(err => {
          console.error('[Route] detection-templates routes failed', err);
          throw err;
        }),
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: PERMISSIONS.DETECTION_LOGICS.DEFAULT }
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
    loadChildren: () =>
      import('./can-signals/can-signals.routes')
        .then(r => {
          console.debug('[Route] can-signals routes loaded');
          return r.CAN_SIGNALS_ROUTES;
        })
        .catch(err => {
          console.error('[Route] can-signals routes failed', err);
          throw err;
        }),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.CAN_SIGNALS.DEFAULT,
    },
  },
  {
    path: 'detection-logics',
    loadChildren: () =>
      import('./detection-logics/detection-logics.routes')
        .then(r => {
          console.debug('[Route] detection-logics routes loaded');
          return r.DETECTION_LOGICS_ROUTES;
        })
        .catch(err => {
          console.error('[Route] detection-logics routes failed', err);
          throw err;
        }),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.DETECTION_LOGICS.DEFAULT,
    },
  },
  {
    path: 'detection-results',
    loadChildren: () =>
      import('./detection-results/detection-results.routes')
        .then(r => {
          console.debug('[Route] detection-results routes loaded');
          return r.detectionResultsRoutes;
        })
        .catch(err => {
          console.error('[Route] detection-results routes failed', err);
          throw err;
        }),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.DETECTION_RESULTS.DEFAULT,
    },
  },
  {
    path: 'projects',
    loadChildren: () =>
      import('./projects/projects.routes')
        .then(r => {
          console.debug('[Route] projects routes loaded');
          return r.projectsRoutes;
        })
        .catch(err => {
          console.error('[Route] projects routes failed', err);
          throw err;
        }),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.PROJECTS.DEFAULT,
    },
  },
  {
    path: 'anomaly-analysis',
    loadChildren: () =>
      import('./anomaly-analysis/anomaly-analysis.routes')
        .then(r => {
          console.debug('[Route] anomaly-analysis routes loaded');
          return r.ANOMALY_ANALYSIS_ROUTES;
        })
        .catch(err => {
          console.error('[Route] anomaly-analysis routes failed', err);
          throw err;
        }),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.ANOMALY_ANALYSIS.DEFAULT,
    },
  },
  {
    path: 'oem-traceability',
    loadChildren: () =>
      import('./oem-traceability/oem-traceability.routes')
        .then(r => {
          console.debug('[Route] oem-traceability routes loaded');
          return r.OEM_TRACEABILITY_ROUTES;
        })
        .catch(err => {
          console.error('[Route] oem-traceability routes failed', err);
          throw err;
        }),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.OEM_TRACEABILITY.DEFAULT,
    },
  },
  {
    path: 'similar-comparison',
    loadChildren: () =>
      import('./similar-comparison/similar-comparison.routes')
        .then(r => {
          console.debug('[Route] similar-comparison routes loaded');
          return r.SIMILAR_COMPARISON_ROUTES;
        })
        .catch(err => {
          console.error('[Route] similar-comparison routes failed', err);
          throw err;
        }),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: PERMISSIONS.SIMILAR_COMPARISON.DEFAULT,
    },
  },
  {
    path: 'dashboard',
    loadChildren: () =>
      import('./dashboard/dashboard.routes')
        .then(r => {
          console.debug('[Route] dashboard routes loaded');
          return r.dashboardRoutes;
        })
        .catch(err => {
          console.error('[Route] dashboard routes failed', err);
          throw err;
        }),
    canActivate: [authGuard],
  },
  {
    path: 'safety-trace-audit',
    loadChildren: () =>
      import('./safety-trace-audit/safety-trace-audit.routes')
        .then(r => {
          console.debug('[Route] safety-trace-audit routes loaded');
          return r.SAFETY_TRACE_AUDIT_ROUTES;
        })
        .catch(err => {
          console.error('[Route] safety-trace-audit routes failed', err);
          throw err;
        }),
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: PERMISSIONS.SAFETY_TRACE.AUDIT.DEFAULT }
  },
  {
    path: 'can-specification',
    loadChildren: () =>
      import('./can-specification/can-specification.routes')
        .then(r => {
            console.debug('[Route] can-specification routes loaded');
            return r.CAN_SPECIFICATION_ROUTES;
        })
        .catch(err => {
          console.error('[Route] can-specification routes failed', err);
          throw err;
        }),
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: PERMISSIONS.CAN_SPECIFICATION.DEFAULT }
  },
  {
    path: 'compatibility-analysis',
    loadChildren: () =>
      import('./compatibility-analysis/compatibility-analysis.routes')
        .then(r => r.COMPATIBILITY_ANALYSIS_ROUTES)
        .catch(err => { console.error('[Route] compatibility-analysis routes failed', err); throw err; }),
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: PERMISSIONS.COMPATIBILITY_ANALYSIS.VIEW }
  },
  {
    path: 'threshold-optimization',
    loadChildren: () =>
      import('./threshold-optimization/threshold-optimization.routes')
        .then(r => r.THRESHOLD_OPTIMIZATION_ROUTES)
        .catch(err => { console.error('[Route] threshold-optimization routes failed', err); throw err; }),
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: PERMISSIONS.THRESHOLD_OPTIMIZATION.CALCULATE }
  },
  {
    path: 'knowledge-base-stats',
    loadChildren: () =>
      import('./knowledge-base-stats/knowledge-base-stats.routes')
        .then(r => r.KNOWLEDGE_BASE_STATS_ROUTES)
        .catch(err => { console.error('[Route] knowledge-base-stats routes failed', err); throw err; }),
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: PERMISSIONS.KNOWLEDGE_BASE.STATISTICS.VIEW }
  },
];
