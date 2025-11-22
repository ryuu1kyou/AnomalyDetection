import { Component, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ExportDownloadService } from '../shared/services/export-download.service';
import { NotificationService } from '../shared/services/notification.service';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { PERMISSIONS } from '../shared/constants/permissions';

interface CanSpecImportDto {
  id: string;
  fileName: string;
  version: string;
  status: string;
  importDate: string;
  messageCount: number;
  signalCount: number;
  diffSummary?: { summaryText?: string };
}

interface CanSpecDiffDto {
  type: string;
  entityType: string;
  entityName: string;
  messageId: string;
  severity: string;
  changeCategory: string;
  impactedSubsystem: string;
  oldValue: string;
  newValue: string;
  changeSummary: string;
  comparisonDate: string;
}

@Component({
  selector: 'app-can-specification',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  template: `
    <div class="can-spec">
      <h2>CAN Specification Import & Diff</h2>
      <form (submit)="upload()" class="upload-form">
        <input type="file" (change)="onFile($event)" required />
        <input type="text" placeholder="Version" [(ngModel)]="version" name="version" />
        <button type="submit" [disabled]="!file">Import</button>
      </form>

      <div class="imports" *ngIf="imports().length">
        <h3>Imports</h3>
        <table>
          <thead>
            <tr>
              <th>File</th><th>Version</th><th>Status</th><th>Messages</th><th>Signals</th><th>Diff Summary</th><th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let imp of imports()" [class.selected]="imp.id === selectedImportId()">
              <td>{{imp.fileName}}</td>
              <td>{{imp.version}}</td>
              <td>{{imp.status}}</td>
              <td>{{imp.messageCount}}</td>
              <td>{{imp.signalCount}}</td>
              <td>{{imp.diffSummary?.summaryText || '—'}} </td>
              <td>
                <button (click)="selectImport(imp.id)">Select</button>
                <button (click)="loadDiffs(imp.id)" [disabled]="loadingDiffs()">Diffs</button>
                <button (click)="exportDiffs(imp.id, 'csv')">Export CSV</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div *ngIf="diffs().length">
        <h3>Diffs ({{diffs().length}})</h3>
        <table class="diff-table">
          <thead>
            <tr>
              <th>Type</th><th>Entity</th><th>Name</th><th>Severity</th><th>Category</th><th>Old</th><th>New</th><th>Date</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let d of diffs()">
              <td>{{d.type}}</td>
              <td>{{d.entityType}}</td>
              <td>{{d.entityName}}</td>
              <td>{{d.severity}}</td>
              <td>{{d.changeCategory}}</td>
              <td>{{d.oldValue}}</td>
              <td>{{d.newValue}}</td>
              <td>{{d.comparisonDate | date:'short'}}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    .can-spec { padding:1rem; }
    .upload-form { display:flex; gap:0.5rem; align-items:center; margin-bottom:1rem; }
    table { width:100%; border-collapse:collapse; font-size:0.75rem; }
    th, td { border:1px solid #394b5a; padding:4px 6px; }
    tr.selected { background:#1e3a5f; }
    .diff-table { margin-top:1rem; }
  `]
})
export class CanSpecificationComponent {
  imports = signal<CanSpecImportDto[]>([]);
  diffs = signal<CanSpecDiffDto[]>([]);
  file: File | null = null;
  version = '';
  loadingDiffs = signal<boolean>(false);
  selectedImportId = signal<string | null>(null);

  private baseUrl = '/api/app/can-spec-import'; // ABP conventional endpoint base

  constructor(private http: HttpClient, private downloader: ExportDownloadService, private notify: NotificationService) {
    this.loadImports();
  }

  onFile(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.file = input.files[0];
    }
  }

  selectImport(id: string) { this.selectedImportId.set(id); }

  loadImports() {
    this.http.get<any>(`${this.baseUrl}?maxResultCount=50`).subscribe({
      next: page => { this.imports.set(page.items || []); this.notify.success('Imports loaded'); },
      error: err => { console.error('Failed to load imports', err); this.notify.error('Imports load failed'); }
    });
  }

  upload() {
    if (!this.file) return;
    const form = new FormData();
    form.append('file', this.file);
    form.append('fileName', this.file.name);
    form.append('fileFormat', 'DBC');
    if (this.version) form.append('version', this.version);
    // Endpoint guess: POST /api/app/can-spec-import/import-specification
    this.http.post<any>(`${this.baseUrl}/import-specification`, form).subscribe({
      next: r => { console.log('Import result', r); this.loadImports(); this.notify.success('Import complete'); },
      error: err => { console.error('Import failed', err); this.notify.error('Import failed'); }
    });
  }

  loadDiffs(importId: string) {
    this.loadingDiffs.set(true);
    // Endpoint guess: GET /api/app/can-spec-import/diffs?specId=GUID
    this.http.get<any>(`${this.baseUrl}/diffs?specId=${importId}&maxResultCount=200`).subscribe({
      next: page => { this.diffs.set(page.items || []); this.loadingDiffs.set(false); this.notify.success('Diffs loaded'); },
      error: err => { console.error('Failed to load diffs', err); this.loadingDiffs.set(false); this.notify.error('Diffs load failed'); }
    });
  }

  exportDiffs(importId: string, format: string) {
    // Endpoint guess: GET /api/app/can-spec-import/export-diffs?specId=GUID&format=csv
    this.http.get(`${this.baseUrl}/export-diffs?specId=${importId}&format=${format}`, { responseType: 'blob' }).subscribe({
      next: blob => { this.downloader.downloadBlob(blob, `can_spec_diffs_${importId}.${format === 'excel' ? 'xlsx' : format}`); this.notify.success('Export ready'); },
      error: err => { console.error('Export diffs failed', err); this.notify.error('Export failed'); }
    });
  }
}
