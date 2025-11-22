import { Routes } from '@angular/router';

export const CAN_SPECIFICATION_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./can-specification.component')
      .then(c => c.CanSpecificationComponent)
      .catch(err => { console.error('[Route] can-specification component load failed', err); throw err; })
  }
];
