import { provideAbpCore, withOptions } from '@abp/ng.core';
import { provideAbpOAuth } from '@abp/ng.oauth';
import { provideSettingManagementConfig } from '@abp/ng.setting-management/config';
import { provideFeatureManagementConfig } from '@abp/ng.feature-management';
import { provideAbpThemeShared } from '@abp/ng.theme.shared';
import { provideIdentityConfig } from '@abp/ng.identity/config';
import { provideAccountConfig } from '@abp/ng.account/config';
import { provideTenantManagementConfig } from '@abp/ng.tenant-management/config';
import { registerLocaleForEsBuild } from '@abp/ng.core/locale';
import { provideThemeLeptonX } from '@abp/ng.theme.lepton-x';
import { provideSideMenuLayout } from '@abp/ng.theme.lepton-x/layouts';
import { provideLogo, withEnvironmentOptions } from '@volo/ngx-lepton-x.core';
import { ApplicationConfig, APP_INITIALIZER, LOCALE_ID } from '@angular/core';
import { AuthService } from '@abp/ng.core';
import { registerLocaleData } from '@angular/common';
import localeJa from '@angular/common/locales/ja';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { environment } from '../environments/environment';
import { APP_ROUTES } from './app.routes';
import { APP_ROUTE_PROVIDER } from './route.provider';

// Ensure we return a stable async function; wrap to avoid accidental undefined.
// Register Angular built-in Japanese locale data for pipes (DecimalPipe, DatePipe, etc.)
registerLocaleData(localeJa);
const localeLoaderInner = registerLocaleForEsBuild();
const localeLoaderFn = async (locale: string) => {
  console.debug('[LocaleLoader] loading locale', locale);
  return localeLoaderInner(locale);
};

export const appConfig: ApplicationConfig = {
  providers: [
    {
      provide: APP_INITIALIZER,
      multi: true,
      useFactory: (auth: AuthService) => () => {
        if (!auth.isAuthenticated) {
          // Defer navigation slightly to avoid blocking other initializers
          setTimeout(() => auth.navigateToLogin(), 0);
        }
      },
      deps: [AuthService],
    },
    provideRouter(APP_ROUTES),
    APP_ROUTE_PROVIDER,
    provideAnimations(),
    provideHttpClient(withInterceptorsFromDi()),
    provideAbpCore(
      withOptions({
        environment,
        // Provide correct locale loader function (ABP expects (locale: string) => Promise<any>).
        // Fixes runtime TypeError: Cannot read properties of undefined (reading 'then').
        registerLocaleFn: localeLoaderFn,
      })
    ),
    provideAbpOAuth(),
    provideIdentityConfig(),
    provideSettingManagementConfig(),
    provideFeatureManagementConfig(),
    provideThemeLeptonX(),
    provideSideMenuLayout(),
    provideLogo(withEnvironmentOptions(environment)),
    provideAccountConfig(),
    provideTenantManagementConfig(),
    provideAbpThemeShared(),
    // NgRx Store configuration
    provideStore(),
    provideEffects(),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: environment.production,
    }),
    // Locale providers for Japanese formatting
    { provide: LOCALE_ID, useValue: 'ja' },
    { provide: MAT_DATE_LOCALE, useValue: 'ja-JP' },
  ],
};
