import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ExportDownloadService {
  downloadBlob(blob: Blob, fileName: string) {
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(a.href);
  }
}
