import { Routes } from '@angular/router';

export const CAN_SIGNALS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/can-signal-list.component').then(m => m.CanSignalListComponent),
    title: '::Menu:CanSignals',
  },
  {
    path: 'create',
    loadComponent: () =>
      import('./components/can-signal-create.component').then(m => m.CanSignalCreateComponent),
    title: '::Menu:CanSignals',
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./components/can-signal-detail.component').then(m => m.CanSignalDetailComponent),
    title: '::Menu:CanSignals',
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./components/can-signal-edit.component').then(m => m.CanSignalEditComponent),
    title: '::Menu:CanSignals',
  },
];
