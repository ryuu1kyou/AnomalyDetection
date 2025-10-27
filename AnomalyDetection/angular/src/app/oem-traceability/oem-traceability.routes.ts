import { Routes } from '@angular/router';

export const OEM_TRACEABILITY_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () => 
      import('./components/oem-traceability-dashboard/oem-traceability-dashboard.component')
        .then(m => m.OemTraceabilityDashboardComponent),
    data: {
      title: 'OEMトレーサビリティ',
      breadcrumb: 'ダッシュボード'
    }
  },
  {
    path: 'customizations',
    loadComponent: () => 
      import('./components/oem-customization-management/oem-customization-management.component')
        .then(m => m.OemCustomizationManagementComponent),
    data: {
      title: 'OEMカスタマイズ管理',
      breadcrumb: 'カスタマイズ管理'
    }
  }
];