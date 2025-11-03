import { NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { SafeJsonInterceptor } from './interceptors/safe-json.interceptor';

@NgModule({
  providers: [{ provide: HTTP_INTERCEPTORS, useClass: SafeJsonInterceptor, multi: true }],
})
export class CoreModule {}
