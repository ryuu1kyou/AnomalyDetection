import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { PermissionService } from '../services/permission.service';

@Injectable({
  providedIn: 'root'
})
export class CustomPermissionGuard implements CanActivate, CanActivateChild {
  constructor(
    private permissionService: PermissionService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkPermission(route);
  }

  canActivateChild(
    childRoute: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkPermission(childRoute);
  }

  private checkPermission(route: ActivatedRouteSnapshot): Observable<boolean> {
    const requiredPermissions = route.data['requiredPermissions'] as string[];
    const requireAll = route.data['requireAllPermissions'] as boolean || false;
    const redirectTo = route.data['redirectTo'] as string || '/';

    if (!requiredPermissions || requiredPermissions.length === 0) {
      return of(true);
    }

    const hasPermission$ = requireAll
      ? this.permissionService.hasAllPermissions$(requiredPermissions)
      : this.permissionService.hasAnyPermission$(requiredPermissions);

    return hasPermission$.pipe(
      map(hasPermission => {
        if (!hasPermission) {
          this.router.navigate([redirectTo]);
          return false;
        }
        return true;
      }),
      catchError(() => {
        this.router.navigate([redirectTo]);
        return of(false);
      })
    );
  }
}