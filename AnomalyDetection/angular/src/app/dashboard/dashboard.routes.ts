import { Routes } from '@angular/router';

export const dashboardRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/dashboard.component').then(m => m.DashboardComponent),
    title: '::Menu:Dashboard',
  },
  {
    path: 'detailed',
    loadComponent: () =>
      import('./components/detailed-statistics.component').then(m => m.DetailedStatisticsComponent),
    title: '::Menu:Dashboard',
  },
];
