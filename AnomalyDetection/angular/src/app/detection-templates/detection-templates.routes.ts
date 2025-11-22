import { Routes } from '@angular/router';
import { CustomPermissionGuard } from '../shared/guards/permission.guard';
import { PERMISSIONS } from '../shared/constants/permissions';
import { TemplateListComponent, TemplateFormComponent, TemplateDetailComponent } from './components';

export const DETECTION_TEMPLATES_ROUTES: Routes = [
  {
    path: 'detection-templates',
    canActivate: [CustomPermissionGuard],
    data: { requiredPermissions: [PERMISSIONS.DETECTION_TEMPLATES.VIEW] },
    children: [
      { path: '', component: TemplateListComponent },
      { path: 'create', component: TemplateFormComponent },
      { path: ':type', component: TemplateDetailComponent }
    ]
  }
];
