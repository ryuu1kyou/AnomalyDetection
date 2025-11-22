import { Routes } from '@angular/router';

export const SAFETY_TRACE_AUDIT_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./safety-trace-audit-dashboard.component')
      .then(c => c.SafetyTraceAuditDashboardComponent)
      .catch(err => { console.error('[Route] safety-trace-audit-dashboard load failed', err); throw err; })
  }
];
