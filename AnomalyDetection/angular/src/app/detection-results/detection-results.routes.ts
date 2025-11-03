import { Routes } from '@angular/router';

export const detectionResultsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/detection-results-list.component').then(
        m => m.DetectionResultsListComponent
      ),
    title: '::Menu:DetectionResults',
  },
  {
    path: 'shared',
    loadComponent: () =>
      import('./components/shared-results-list.component').then(m => m.SharedResultsListComponent),
    title: '::Menu:DetectionResults',
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./components/detection-result-detail.component').then(
        m => m.DetectionResultDetailComponent
      ),
    title: '::Menu:DetectionResults',
  },
];
