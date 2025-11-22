import { Routes } from '@angular/router';

export const KNOWLEDGE_BASE_STATS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./knowledge-base-stats.component')
      .then(c => c.KnowledgeBaseStatsComponent)
      .catch(err => { console.error('[Route] knowledge-base-stats component load failed', err); throw err; })
  }
];
