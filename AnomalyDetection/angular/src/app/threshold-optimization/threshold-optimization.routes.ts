import { Routes } from '@angular/router';

export const THRESHOLD_OPTIMIZATION_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./threshold-optimization.component')
      .then(c => c.ThresholdOptimizationComponent)
      .catch(err => { console.error('[Route] threshold-optimization component load failed', err); throw err; })
  }
];
