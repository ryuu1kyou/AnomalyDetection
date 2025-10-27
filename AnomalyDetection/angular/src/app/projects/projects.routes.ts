import { Routes } from '@angular/router';

export const projectsRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/project-list.component').then(m => m.ProjectListComponent),
    title: 'プロジェクト一覧'
  }
  // TODO: Uncomment when components are implemented
  // {
  //   path: 'create',
  //   loadComponent: () => import('./components/project-create.component').then(m => m.ProjectCreateComponent),
  //   title: '新規プロジェクト作成'
  // },
  // {
  //   path: ':id',
  //   loadComponent: () => import('./components/project-detail.component').then(m => m.ProjectDetailComponent),
  //   title: 'プロジェクト詳細'
  // },
  // {
  //   path: ':id/edit',
  //   loadComponent: () => import('./components/project-edit.component').then(m => m.ProjectEditComponent),
  //   title: 'プロジェクト編集'
  // },
  // {
  //   path: ':id/members',
  //   loadComponent: () => import('./components/project-members.component').then(m => m.ProjectMembersComponent),
  //   title: 'プロジェクトメンバー管理'
  // },
  // {
  //   path: ':id/milestones',
  //   loadComponent: () => import('./components/project-milestones.component').then(m => m.ProjectMilestonesComponent),
  //   title: 'マイルストーン管理'
  // }
];