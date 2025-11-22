import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ExportDownloadService } from '../shared/services/export-download.service';
import { NotificationService } from '../shared/services/notification.service';
import { PERMISSIONS } from '../shared/constants/permissions';

interface CompatibilityStatusDto {
  oldSpecId: string;
  newSpecId: string;
  context: string;
  isCompatible: boolean;
  highestSeverity: number;
  breakingChangeCount: number;
  warningCount: number;
  infoCount: number;
  compatibilityScore: number;
  summary: string;
  impactedSubsystems: string[];
  keyFindings: string[];
  generatedAt: string;
}

interface CompatibilityAnalysisExportDto { analysisId: string; format: string; includeIssues: boolean; includeImpacts: boolean; }

@Component({
  selector: 'app-compatibility-analysis',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
  <div class="ca-page">
    <h2>Compatibility Analysis</h2>
    <form [formGroup]="form" (ngSubmit)="runAnalysis()" class="form-grid">
      <label>Old Spec ID<input formControlName="oldSpecId" required /></label>
      <label>New Spec ID<input formControlName="newSpecId" required /></label>
      <label>Context<select formControlName="context">
        <option value="General">General</option>
        <option value="Regulatory">Regulatory</option>
        <option value="Safety">Safety</option>
        <option value="Diagnostics">Diagnostics</option>
      </select></label>
      <div class="buttons">
        <button type="submit">Analyze</button>
        <button type="button" (click)="quickAssess()">Quick Assess</button>
      </div>
    </form>

    <div *ngIf="status() as st" class="status-box">
      <h3>Status: <span [class.ok]="st.isCompatible" [class.fail]="!st.isCompatible">{{st.isCompatible ? 'Compatible' : 'Attention Needed'}} (Score {{st.compatibilityScore}})</span></h3>
      <p>{{st.summary}}</p>
      <p>Breaking: {{st.breakingChangeCount}} | Warnings: {{st.warningCount}} | Info: {{st.infoCount}}</p>
      <p *ngIf="st.impactedSubsystems.length">Impacted: {{st.impactedSubsystems.join(', ')}}</p>
      <ul *ngIf="st.keyFindings.length">
        <li *ngFor="let f of st.keyFindings">{{f}}</li>
      </ul>
    </div>

    <div *ngIf="analysisId()" class="export-box">
      <h3>Export Analysis</h3>
      <label><input type="checkbox" [(ngModel)]="includeIssues" /> Issues</label>
      <label><input type="checkbox" [(ngModel)]="includeImpacts" /> Impacts</label>
      <button (click)="export('csv')">CSV</button>
      <button (click)="export('json')">JSON</button>
      <button (click)="export('excel')">Excel</button>
    </div>
  </div>
  `,
  styles: [`
    .ca-page { padding:1rem; display:flex; flex-direction:column; gap:1rem; }
    .form-grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(180px,1fr)); gap:0.75rem; }
    .form-grid label { display:flex; flex-direction:column; font-size:0.75rem; }
    .buttons { display:flex; gap:0.5rem; align-items:flex-end; }
    .status-box { background:#1f2f40; padding:0.75rem; border-radius:4px; font-size:0.8rem; }
    .status-box .ok { color:#4caf50; } .status-box .fail { color:#ff9800; }
    .export-box { background:#243546; padding:0.75rem; border-radius:4px; }
  `]
})
export class CompatibilityAnalysisComponent {
  form = this.fb.group({ oldSpecId: [''], newSpecId: [''], context: ['General'] });
  status = signal<CompatibilityStatusDto | null>(null);
  analysisId = signal<string | null>(null);
  includeIssues = true;
  includeImpacts = true;

  private baseUrl = '/api/app/compatibility-analysis';

  constructor(private fb: FormBuilder, private http: HttpClient, private downloader: ExportDownloadService, private notify: NotificationService) {}

  quickAssess() {
    const v = this.form.value;
    const body = { oldSpecId: v.oldSpecId, newSpecId: v.newSpecId, context: v.context, forceRefresh: true };
    this.http.post<CompatibilityStatusDto>(`${this.baseUrl}/assess-compatibility`, body).subscribe({
      next: r => { this.status.set(r); this.notify.success('Quick assessment complete'); },
      error: e => { console.error('Assess failed', e); this.notify.error('Quick assessment failed'); }
    });
  }

  runAnalysis() {
    const v = this.form.value;
    const body = { oldSpecId: v.oldSpecId, newSpecId: v.newSpecId };
    this.http.post<any>(`${this.baseUrl}/analyze-compatibility`, body).subscribe({
      next: r => { this.analysisId.set(r.analysisId); this.status.set({
        oldSpecId: v.oldSpecId || '', newSpecId: v.newSpecId || '', context: v.context || 'General',
        isCompatible: true, highestSeverity: r.breakingChangeCount>0?3:1, breakingChangeCount: r.breakingChangeCount,
        warningCount: r.warningCount, infoCount: r.infoCount, compatibilityScore: r.compatibilityScore,
        summary: r.summary || '', impactedSubsystems: [], keyFindings: [], generatedAt: new Date().toISOString()
      }); this.notify.success('Compatibility analysis completed'); },
      error: e => { console.error('Analysis failed', e); this.notify.error('Compatibility analysis failed'); }
    });
  }

  export(format: string) {
    if (!this.analysisId()) return;
    const url = `${this.baseUrl}/export?analysisId=${this.analysisId()}&format=${format}&includeIssues=${this.includeIssues}&includeImpacts=${this.includeImpacts}`;
    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: blob => { this.downloader.downloadBlob(blob, `compat_analysis_${this.analysisId()}.${format==='excel'?'xlsx':format}`); this.notify.success('Export ready'); },
      error: e => { console.error('Export failed', e); this.notify.error('Export failed'); }
    });
  }
}
