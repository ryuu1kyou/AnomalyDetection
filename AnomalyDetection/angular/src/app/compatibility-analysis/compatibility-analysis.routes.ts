import { Routes } from '@angular/router';

export const COMPATIBILITY_ANALYSIS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./compatibility-analysis.component')
      .then(c => c.CompatibilityAnalysisComponent)
      .catch(err => { console.error('[Route] compatibility-analysis component load failed', err); throw err; })
  }
];
