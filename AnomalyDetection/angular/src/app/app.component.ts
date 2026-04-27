import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { DynamicLayoutComponent, AuthService } from '@abp/ng.core';
import { LoaderBarComponent } from '@abp/ng.theme.shared';
import { Router, NavigationEnd } from '@angular/router';
import { filter, take } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  template: `
    <abp-loader-bar />
    <abp-dynamic-layout />
  `,
  imports: [LoaderBarComponent, DynamicLayoutComponent],
})
export class AppComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        take(1)
      )
      .subscribe(() => {
        const url = this.router.url;
        if (
          this.authService.isAuthenticated &&
          (url.includes('?code=') || url.includes('&state=') || url.includes('&iss='))
        ) {
          this.router.navigate(['/'], { replaceUrl: true });
        }
        // DynamicLayoutComponent.getLayout() runs in another NavigationEnd subscription.
        // setTimeout defers detectChanges until after all NavigationEnd callbacks complete,
        // ensuring the layout signal is set before CD runs.
        setTimeout(() => this.cdr.detectChanges(), 0);
      });
  }
}
