import { Routes } from '@angular/router';

export const dashboardRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/dashboard.component').then(m => m.DashboardComponent),
    title: '統計ダッシュボード'
  },
  {
    path: 'detailed',
    loadComponent: () => import('./components/detailed-statistics.component').then(m => m.DetailedStatisticsComponent),
    title: '詳細統計・レポート'
  }
];