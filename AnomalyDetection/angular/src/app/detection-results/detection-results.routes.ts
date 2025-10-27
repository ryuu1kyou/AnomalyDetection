import { Routes } from '@angular/router';

export const detectionResultsRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/detection-results-list.component').then(m => m.DetectionResultsListComponent),
    title: '異常検出結果一覧'
  },
  {
    path: 'shared',
    loadComponent: () => import('./components/shared-results-list.component').then(m => m.SharedResultsListComponent),
    title: '共有された異常検出結果'
  },
  {
    path: ':id',
    loadComponent: () => import('./components/detection-result-detail.component').then(m => m.DetectionResultDetailComponent),
    title: '異常検出結果詳細'
  }
];