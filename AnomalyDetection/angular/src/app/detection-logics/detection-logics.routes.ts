import { Routes } from '@angular/router';

export const DETECTION_LOGICS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/detection-logic-list.component').then(m => m.DetectionLogicListComponent),
    title: '異常検出ロジック一覧'
  },
  {
    path: 'create',
    loadComponent: () => import('./components/detection-logic-create.component').then(m => m.DetectionLogicCreateComponent),
    title: '新規検出ロジック作成'
  },
  {
    path: ':id',
    loadComponent: () => import('./components/detection-logic-detail.component').then(m => m.DetectionLogicDetailComponent),
    title: '検出ロジック詳細'
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./components/detection-logic-edit.component').then(m => m.DetectionLogicEditComponent),
    title: '検出ロジック編集'
  },
  {
    path: ':id/execute',
    loadComponent: () => import('./components/detection-logic-execute.component').then(m => m.DetectionLogicExecuteComponent),
    title: '検出ロジック実行'
  }
];