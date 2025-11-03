import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpInterceptor,
  HttpHandler,
  HttpRequest,
  HttpResponse,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, map, catchError } from 'rxjs';

/**
 * Global interceptor to safely parse JSON responses where services requested text and manually parsed.
 * If responseType is 'json' Angular handles parsing; this focuses on 'text' responses.
 * Detects HTML fallback (<!DOCTYPE ...) and converts to empty object.
 */
@Injectable()
export class SafeJsonInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Only wrap text responses explicitly requested.
    return next.handle(req).pipe(
      map(event => {
        if (
          event instanceof HttpResponse &&
          event.body &&
          typeof event.body === 'string' &&
          req.responseType === 'text'
        ) {
          const raw = event.body as string;
          if (!raw) {
            return event.clone({ body: {} });
          }
          if (raw.startsWith('<!DOCTYPE')) {
            console.warn('[SafeJsonInterceptor] HTML fallback detected for', req.url);
            return event.clone({ body: {} });
          }
          try {
            const parsed = JSON.parse(raw);
            return event.clone({ body: parsed });
          } catch (e) {
            console.warn('[SafeJsonInterceptor] JSON parse error for', req.url, e);
            return event.clone({ body: {} });
          }
        }
        return event;
      }),
      catchError(err => {
        // Preserve HttpErrorResponse but if status 200 with parse issue we already handled above.
        throw err;
      })
    );
  }
}
