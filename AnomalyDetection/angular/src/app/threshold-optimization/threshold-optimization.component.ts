import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormArray } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ExportDownloadService } from '../shared/services/export-download.service';
import { NotificationService } from '../shared/services/notification.service';

interface ThresholdEvaluationDto {
  threshold: number; precision: number; recall: number; f1: number; truePositiveRate: number; falsePositiveRate: number; balancedAccuracy: number; youdenJ: number; support: number;
}
interface ThresholdOptimizationResultDto {
  detectionLogicId: string; recommendedThreshold: number; objective: string; bestScore: number; evaluations: ThresholdEvaluationDto[]; summary: string;
}

@Component({
  selector: 'app-threshold-optimization',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
  <div class="thopt-page">
    <h2>Threshold Optimization</h2>
    <form [formGroup]="form" (ngSubmit)="optimize()" class="form">
      <label>Detection Logic ID <input formControlName="detectionLogicId" required /></label>
      <label>Objective
        <select formControlName="objective">
          <option value="f1">F1</option>
          <option value="youden">Youden J</option>
          <option value="balanced_accuracy">Balanced Accuracy</option>
        </select>
      </label>
      <div class="candidates">
        <h4>Candidates</h4>
        <div *ngFor="let g of candidates.controls; let i = index" [formGroup]="g" class="cand-row">
          <input type="number" step="0.0001" formControlName="threshold" placeholder="Threshold" />
          <input type="number" formControlName="tp" placeholder="TP" />
          <input type="number" formControlName="fp" placeholder="FP" />
          <input type="number" formControlName="tn" placeholder="TN" />
          <input type="number" formControlName="fn" placeholder="FN" />
          <button type="button" (click)="removeCandidate(i)">×</button>
        </div>
        <button type="button" (click)="addCandidate()">Add Candidate</button>
      </div>
      <button type="submit">Optimize</button>
      <button type="button" (click)="export('csv')" [disabled]="!result()">Export CSV</button>
      <button type="button" (click)="export('excel')" [disabled]="!result()">Export Excel</button>
    </form>

    <div *ngIf="result() as r" class="result-box">
      <h3>Recommended Threshold: {{r.recommendedThreshold | number:'1.4-4'}} (Objective {{r.objective}} Score {{r.bestScore}})</h3>
      <p>{{r.summary}}</p>
      <table>
        <thead>
          <tr>
            <th>Threshold</th><th>P</th><th>R</th><th>F1</th><th>TPR</th><th>FPR</th><th>Bal.Acc</th><th>YoudenJ</th><th>Support</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let e of r.evaluations">
            <td>{{e.threshold}}</td>
            <td>{{e.precision}}</td>
            <td>{{e.recall}}</td>
            <td>{{e.f1}}</td>
            <td>{{e.truePositiveRate}}</td>
            <td>{{e.falsePositiveRate}}</td>
            <td>{{e.balancedAccuracy}}</td>
            <td>{{e.youdenJ}}</td>
            <td>{{e.support}}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  `,
  styles: [`
    .thopt-page { padding:1rem; display:flex; flex-direction:column; gap:1rem; }
    form.form { display:flex; flex-direction:column; gap:0.75rem; }
    label { display:flex; flex-direction:column; font-size:0.75rem; }
    .candidates { background:#1f2f40; padding:0.5rem; border-radius:4px; }
    .cand-row { display:grid; grid-template-columns: repeat(6,1fr) 40px; gap:0.25rem; margin-bottom:0.25rem; }
    table { width:100%; border-collapse:collapse; font-size:0.7rem; }
    th, td { border:1px solid #34485a; padding:2px 4px; }
  `]
})
export class ThresholdOptimizationComponent {
  form = this.fb.group({ detectionLogicId: [''], objective: ['f1'], candidates: this.fb.array([]) });
  result = signal<ThresholdOptimizationResultDto | null>(null);
  private baseUrl = '/api/app/threshold-optimization';

  constructor(private fb: FormBuilder, private http: HttpClient, private downloader: ExportDownloadService, private notify: NotificationService) { this.addCandidate(); this.addCandidate(); }

  get candidates() { return this.form.get('candidates') as FormArray; }
  addCandidate() { this.candidates.push(this.fb.group({ threshold: [0], tp: [0], fp: [0], tn: [0], fn: [0] })); }
  removeCandidate(i: number) { this.candidates.removeAt(i); }

  optimize() {
    const v = this.form.value;
    const candidates = (v.candidates || []).map((c: any) => ({ threshold: +c.threshold, truePositives: +c.tp, falsePositives: +c.fp, trueNegatives: +c.tn, falseNegatives: +c.fn }));
    const body = { detectionLogicId: v.detectionLogicId, objective: v.objective, candidates };
    this.http.post<ThresholdOptimizationResultDto>(`${this.baseUrl}/optimize`, body).subscribe({
      next: r => { this.result.set(r); this.notify.success('Optimization complete'); },
      error: e => { console.error('Optimize failed', e); this.notify.error('Optimization failed'); }
    });
  }

  export(format: string) {
    if (!this.result()) return;
    const v = this.form.value;
    const candidates = (v.candidates || []).map((c: any) => ({ threshold: +c.threshold, truePositives: +c.tp, falsePositives: +c.fp, trueNegatives: +c.tn, falseNegatives: +c.fn }));
    const body = { detectionLogicId: v.detectionLogicId, objective: v.objective, format, candidates };
    this.http.post(`${this.baseUrl}/export`, body, { responseType: 'blob' }).subscribe({
      next: blob => { this.downloader.downloadBlob(blob, `threshold_opt_${v.detectionLogicId}.${format==='excel'?'xlsx':format}`); this.notify.success('Export ready'); },
      error: e => { console.error('Export failed', e); this.notify.error('Export failed'); }
    });
  }
}
