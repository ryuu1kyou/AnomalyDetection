import { Component, OnInit, inject } from '@angular/core';
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

  ngOnInit() {
    // Clean up OAuth callback parameters from URL after successful login
    // This prevents "wrong state/nonce" errors on page reload
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        take(1)
      )
      .subscribe(() => {
        const url = this.router.url;
        // If URL contains OAuth callback parameters and user is authenticated
        if (
          this.authService.isAuthenticated &&
          (url.includes('?code=') || url.includes('&state=') || url.includes('&iss='))
        ) {
          console.log('[AppComponent] Cleaning OAuth callback parameters from URL');
          // Navigate to root without parameters
          this.router.navigate(['/'], { replaceUrl: true });
        }
      });
  }
}
