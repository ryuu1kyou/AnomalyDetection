import { Component, signal, effect } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ExportDownloadService } from '../shared/services/export-download.service';
import { NotificationService } from '../shared/services/notification.service';
import { PERMISSIONS } from '../shared/constants/permissions';

interface SafetyTraceAuditAggregateDto {
  totalRecords: number;
  approvedCount: number;
  rejectedCount: number;
  underReviewCount: number;
  submittedCount: number;
  draftCount: number;
  asilDistribution: Record<number, number>;
  averageVerifications: number;
  averageValidations: number;
  highRiskPending: number;
}

@Component({
  selector: 'app-safety-trace-audit-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
  <div class="audit-dashboard">
    <h2>Safety Trace Audit Dashboard</h2>
    <form [formGroup]="filterForm" (ngSubmit)="refresh()" class="filter-form">
      <label>From <input type="date" formControlName="from" /></label>
      <label>To <input type="date" formControlName="to" /></label>
      <label>ASIL <input type="number" min="1" max="4" formControlName="asilLevel" /></label>
      <button type="submit">Apply</button>
      <button type="button" (click)="export('csv')">CSV</button>
      <button type="button" (click)="export('json')">JSON</button>
      <button type="button" (click)="export('excel')">Excel</button>
    </form>

    <div *ngIf="loading()">Loading...</div>

    <div *ngIf="aggregate() as agg" class="aggregate-cards">
      <div class="card">Total: {{agg.totalRecords}}</div>
      <div class="card approved">Approved: {{agg.approvedCount}}</div>
      <div class="card rejected">Rejected: {{agg.rejectedCount}}</div>
      <div class="card under-review">Under Review: {{agg.underReviewCount}}</div>
      <div class="card submitted">Submitted: {{agg.submittedCount}}</div>
      <div class="card draft">Draft: {{agg.draftCount}}</div>
      <div class="card high-risk" *ngIf="agg.highRiskPending > 0">High-Risk Pending: {{agg.highRiskPending}}</div>
    </div>

    <div *ngIf="aggregate() as agg">
      <h3>ASIL Distribution</h3>
      <table>
        <thead><tr><th>ASIL</th><th>Count</th></tr></thead>
        <tbody>
          <tr *ngFor="let asil of asilKeys(agg)"><td>{{asil}}</td><td>{{agg.asilDistribution[asil]}}</td></tr>
        </tbody>
      </table>
      <p>Avg Verifications: {{agg.averageVerifications}} | Avg Validations: {{agg.averageValidations}}</p>
    </div>
  </div>
  `,
  styles: [`
    .audit-dashboard { padding: 1rem; }
    .filter-form { display:flex; gap:1rem; flex-wrap:wrap; margin-bottom:1rem; }
    .filter-form label { display:flex; flex-direction:column; font-size:0.8rem; }
    .aggregate-cards { display:grid; grid-template-columns:repeat(auto-fill,minmax(140px,1fr)); gap:0.5rem; }
    .card { background:#202e3a; color:#fff; padding:0.75rem; border-radius:4px; font-size:0.85rem; }
    .card.approved { background:#2e7d32; }
    .card.rejected { background:#c62828; }
    .card.under-review { background:#1565c0; }
    .card.submitted { background:#6a1b9a; }
    .card.draft { background:#455a64; }
    .card.high-risk { background:#ef6c00; }
    table { width:300px; border-collapse: collapse; margin-top:0.5rem; }
    td, th { border:1px solid #3a4a57; padding:4px 6px; font-size:0.75rem; }
  `]
})
export class SafetyTraceAuditDashboardComponent {
  filterForm = this.fb.group({
    from: [''],
    to: [''],
    asilLevel: ['']
  });

  aggregate = signal<SafetyTraceAuditAggregateDto | null>(null);
  loading = signal<boolean>(false);

  private baseUrl = '/api/app/safety-trace-audit-report'; // ABP conventional endpoint base

  constructor(private fb: FormBuilder, private http: HttpClient, private downloader: ExportDownloadService, private notify: NotificationService) {
    this.refresh();
  }

  asilKeys(agg: SafetyTraceAuditAggregateDto) { return Object.keys(agg.asilDistribution); }

  buildQueryParams(): string {
    const v = this.filterForm.value;
    const params: string[] = [];
    if (v.from) params.push(`from=${v.from}`);
    if (v.to) params.push(`to=${v.to}`);
    if (v.asilLevel) params.push(`asilLevel=${v.asilLevel}`);
    return params.length ? ('?' + params.join('&')) : '';
  }

  refresh() {
    this.loading.set(true);
    const url = `${this.baseUrl}/aggregate${this.buildQueryParams()}`; // assumes GET mapping
    this.http.get<SafetyTraceAuditAggregateDto>(url).subscribe({
      next: d => { this.aggregate.set(d); this.loading.set(false); this.notify.success('Audit aggregate loaded'); },
      error: err => { console.error('Failed to load audit aggregate', err); this.loading.set(false); this.notify.error('Audit load failed'); }
    });
  }

  export(format: string) {
    const url = `${this.baseUrl}/export${this.buildQueryParams()}&format=${format}`;
    this.http.get<any>(url, { responseType: 'blob' as 'json' }).subscribe({
      next: blob => { const fileName = `safety_audit_${new Date().toISOString().substring(0,10)}.${format === 'excel' ? 'xlsx' : format}`; this.downloader.downloadBlob(blob as any, fileName); this.notify.success('Export ready'); },
      error: err => { console.error('Export failed', err); this.notify.error('Export failed'); }
    });
  }
}
