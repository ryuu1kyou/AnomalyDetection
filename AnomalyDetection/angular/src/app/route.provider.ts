import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';
import { PERMISSIONS } from './shared/constants/permissions';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    // Ensure we return void or a Promise; previous implementation returned undefined implicitly.
    // If Angular/ABP chains .then, undefined caused TypeError. Return a resolved Promise after configuration.
    configureRoutes();
    return Promise.resolve();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
    {
      path: '/',
      name: '::Menu:Home',
      iconClass: 'fas fa-home',
      order: 1,
      layout: eLayoutType.application,
    },
    {
      path: '/dashboard',
      name: '::Menu:Dashboard',
      iconClass: 'fas fa-chart-line',
      order: 2,
      layout: eLayoutType.application,
    },
    {
      path: '/can-signals',
      name: '::Menu:CanSignals',
      iconClass: 'fas fa-signal',
      order: 10,
      layout: eLayoutType.application,
      requiredPolicy: PERMISSIONS.CAN_SIGNALS.DEFAULT,
    },
    {
      path: '/detection-logics',
      name: '::Menu:DetectionLogics',
      iconClass: 'fas fa-brain',
      order: 20,
      layout: eLayoutType.application,
      requiredPolicy: PERMISSIONS.DETECTION_LOGICS.DEFAULT,
    },
    {
      path: '/detection-results',
      name: '::Menu:DetectionResults',
      iconClass: 'fas fa-exclamation-triangle',
      order: 30,
      layout: eLayoutType.application,
      requiredPolicy: PERMISSIONS.DETECTION_RESULTS.DEFAULT,
    },
    {
      path: '/projects',
      name: '::Menu:Projects',
      iconClass: 'fas fa-project-diagram',
      order: 40,
      layout: eLayoutType.application,
      requiredPolicy: PERMISSIONS.PROJECTS.DEFAULT,
    },
    {
      path: '/oem-traceability',
      name: '::Menu:OemTraceability',
      iconClass: 'fas fa-history',
      order: 50,
      layout: eLayoutType.application,
      requiredPolicy: PERMISSIONS.OEM_TRACEABILITY.DEFAULT,
    },
    {
      path: '/similar-comparison',
      name: '::Menu:SimilarComparison',
      iconClass: 'fas fa-search',
      order: 60,
      layout: eLayoutType.application,
      requiredPolicy: PERMISSIONS.SIMILAR_COMPARISON.DEFAULT,
    },
    {
      path: '/anomaly-analysis',
      name: '::Menu:AnomalyAnalysis',
      iconClass: 'fas fa-chart-bar',
      order: 70,
      layout: eLayoutType.application,
      requiredPolicy: PERMISSIONS.ANOMALY_ANALYSIS.DEFAULT,
    },
  ]);
}
