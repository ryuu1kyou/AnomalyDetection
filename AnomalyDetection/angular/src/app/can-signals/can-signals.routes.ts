import { Routes } from '@angular/router';

export const CAN_SIGNALS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/can-signal-list.component').then(m => m.CanSignalListComponent),
    title: 'CAN信号一覧'
  },
  {
    path: 'create',
    loadComponent: () => import('./components/can-signal-create.component').then(m => m.CanSignalCreateComponent),
    title: '新規CAN信号作成'
  },
  {
    path: ':id',
    loadComponent: () => import('./components/can-signal-detail.component').then(m => m.CanSignalDetailComponent),
    title: 'CAN信号詳細'
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./components/can-signal-edit.component').then(m => m.CanSignalEditComponent),
    title: 'CAN信号編集'
  }
];