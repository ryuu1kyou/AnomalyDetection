import { Injectable } from '@angular/core';
// Fallback simple notification using browser alert (ABP MessageService not found).
// Later can replace with a proper toast component.

@Injectable({ providedIn: 'root' })
export class NotificationService {
  success(text: string) { console.info('[SUCCESS]', text); }
  error(text: string) { console.error('[ERROR]', text); alert(text); }
  info(text: string) { console.info('[INFO]', text); }
  warn(text: string) { console.warn('[WARN]', text); }
}
