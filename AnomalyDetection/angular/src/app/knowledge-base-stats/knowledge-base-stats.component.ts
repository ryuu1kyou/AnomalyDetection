import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ExportDownloadService } from '../shared/services/export-download.service';
import { NotificationService } from '../shared/services/notification.service';

interface KnowledgeBaseStatisticsDto {
  totalArticles: number; publishedArticles: number; draftArticles: number; totalComments: number; averageRating: number; topTags: string[]; popularArticles: { id: string; title: string; summary: string; usefulCount: number; averageRating: number; }[];
}

@Component({
  selector: 'app-knowledge-base-stats',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="kb-stats-page">
      <h2>Knowledge Base Statistics</h2>
      <div class="actions">
        <button (click)="refresh()">Refresh</button>
        <button (click)="export('csv')">CSV</button>
        <button (click)="export('json')">JSON</button>
        <button (click)="export('excel')">Excel</button>
      </div>
      <div *ngIf="stats() as s" class="stats-grid">
        <div class="card">Total: {{s.totalArticles}}</div>
        <div class="card">Published: {{s.publishedArticles}}</div>
        <div class="card">Draft: {{s.draftArticles}}</div>
        <div class="card">Comments: {{s.totalComments}}</div>
        <div class="card">Avg Rating: {{s.averageRating}}</div>
      </div>
      <div *ngIf="stats() as s">
        <h3>Top Tags</h3>
        <p *ngIf="!s.topTags.length">(none)</p>
        <ul *ngIf="s.topTags.length"><li *ngFor="let t of s.topTags">{{t}}</li></ul>
        <h3>Popular Articles</h3>
        <table *ngIf="s.popularArticles.length">
          <thead><tr><th>Title</th><th>Useful</th><th>Avg Rating</th></tr></thead>
          <tbody>
            <tr *ngFor="let p of s.popularArticles"><td>{{p.title}}</td><td>{{p.usefulCount}}</td><td>{{p.averageRating}}</td></tr>
          </tbody>
        </table>
        <p *ngIf="!s.popularArticles.length">No popular articles.</p>
      </div>
    </div>
  `,
  styles: [`
    .kb-stats-page { padding:1rem; display:flex; flex-direction:column; gap:1rem; }
    .actions { display:flex; gap:0.5rem; }
    .stats-grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(140px,1fr)); gap:0.5rem; }
    .card { background:#1f2f40; color:#fff; padding:0.5rem; font-size:0.75rem; border-radius:4px; }
    table { width:100%; border-collapse:collapse; font-size:0.7rem; }
    th, td { border:1px solid #34485a; padding:2px 4px; }
  `]
})
export class KnowledgeBaseStatsComponent {
  stats = signal<KnowledgeBaseStatisticsDto | null>(null);
  private baseUrl = '/api/app/knowledge-base-statistics-export';

  constructor(private http: HttpClient, private downloader: ExportDownloadService, private notify: NotificationService) { this.refresh(); }

  refresh() {
    this.http.get<KnowledgeBaseStatisticsDto>(`${this.baseUrl}/statistics`).subscribe({
      next: r => { this.stats.set(r); this.notify.success('Statistics loaded'); },
      error: e => { console.error('Stats load failed', e); this.notify.error('Statistics load failed'); }
    });
  }

  export(format: string) {
    this.http.get(`${this.baseUrl}/export?format=${format}`, { responseType: 'blob' }).subscribe({
      next: blob => { this.downloader.downloadBlob(blob, `kb_stats.${format==='excel'?'xlsx':format}`); this.notify.success('Export ready'); },
      error: e => { console.error('Export failed', e); this.notify.error('Export failed'); }
    });
  }
}
