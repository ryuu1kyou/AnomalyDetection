import { Directive, Input, TemplateRef, ViewContainerRef, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { PermissionService } from '../services/permission.service';

@Directive({
  selector: '[hasPermission]',
  standalone: true
})
export class HasPermissionDirective implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private permission: string | string[] = '';
  private requireAll = false;

  @Input() set hasPermission(permission: string | string[]) {
    this.permission = permission;
    this.updateView();
  }

  @Input() set hasPermissionRequireAll(requireAll: boolean) {
    this.requireAll = requireAll;
    this.updateView();
  }

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private permissionService: PermissionService
  ) {}

  ngOnInit() {
    this.updateView();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateView() {
    if (!this.permission) {
      this.viewContainer.clear();
      return;
    }

    const permissions = Array.isArray(this.permission) ? this.permission : [this.permission];
    
    const hasPermission$ = this.requireAll 
      ? this.permissionService.hasAllPermissions$(permissions)
      : this.permissionService.hasAnyPermission$(permissions);

    hasPermission$
      .pipe(takeUntil(this.destroy$))
      .subscribe(hasPermission => {
        if (hasPermission) {
          this.viewContainer.createEmbeddedView(this.templateRef);
        } else {
          this.viewContainer.clear();
        }
      });
  }
}