import { Routes } from '@angular/router';

export const DETECTION_LOGICS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/detection-logic-list.component').then(
        m => m.DetectionLogicListComponent
      ),
    title: '::Menu:DetectionLogics',
  },
  {
    path: 'create',
    loadComponent: () =>
      import('./components/detection-logic-create.component').then(
        m => m.DetectionLogicCreateComponent
      ),
    title: '::Menu:DetectionLogics',
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./components/detection-logic-detail.component').then(
        m => m.DetectionLogicDetailComponent
      ),
    title: '::Menu:DetectionLogics',
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./components/detection-logic-edit.component').then(
        m => m.DetectionLogicEditComponent
      ),
    title: '::Menu:DetectionLogics',
  },
  {
    path: ':id/execute',
    loadComponent: () =>
      import('./components/detection-logic-execute.component').then(
        m => m.DetectionLogicExecuteComponent
      ),
    title: '::Menu:DetectionLogics',
  },
];
