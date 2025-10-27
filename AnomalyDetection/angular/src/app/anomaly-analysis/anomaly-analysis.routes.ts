import { Routes } from '@angular/router';

export const ANOMALY_ANALYSIS_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'pattern-analysis',
    pathMatch: 'full'
  },
  {
    path: 'pattern-analysis',
    loadComponent: () => import('./components/pattern-analysis/pattern-analysis.component').then(m => m.PatternAnalysisComponent),
    data: { title: 'Anomaly Pattern Analysis' }
  },
  {
    path: 'threshold-recommendations',
    loadComponent: () => import('./components/threshold-recommendations/threshold-recommendations.component').then(m => m.ThresholdRecommendationsComponent),
    data: { title: 'Threshold Recommendations' }
  },
  {
    path: 'accuracy-metrics',
    loadComponent: () => import('./components/accuracy-metrics/accuracy-metrics.component').then(m => m.AccuracyMetricsComponent),
    data: { title: 'Detection Accuracy Metrics' }
  }
];