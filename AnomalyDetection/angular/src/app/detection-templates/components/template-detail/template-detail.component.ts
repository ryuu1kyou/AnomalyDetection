import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { DetectionTemplatesService, DetectionTemplateSummary } from '../../services/detection-templates.service';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-template-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './template-detail.component.html',
  styleUrls: ['./template-detail.component.scss']
})
export class TemplateDetailComponent {
  private route = inject(ActivatedRoute);
  private svc = inject(DetectionTemplatesService);
  protected template = signal<DetectionTemplateSummary | null>(null);
  protected loading = signal(false);

  ngOnInit() {
    const typeParam = this.route.snapshot.paramMap.get('type');
    if (!typeParam) return;
    const type = Number(typeParam);
    this.loading.set(true);
    this.svc.getByType(type).subscribe({
      next: t => { this.template.set(t); this.loading.set(false); },
      error: err => { console.error('[TemplateDetail] load failed', err); this.loading.set(false); }
    });
  }
}
