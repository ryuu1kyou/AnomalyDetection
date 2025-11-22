import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DetectionTemplatesService, DetectionTemplateSummary } from '../../services/detection-templates.service';
import { RouterModule, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatInputModule } from '@angular/material/input';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'app-template-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatInputModule,
    ReactiveFormsModule
  ],
  templateUrl: './template-list.component.html',
  styleUrls: ['./template-list.component.scss']
})
export class TemplateListComponent {
  private svc = inject(DetectionTemplatesService);
  private router = inject(Router);

  protected loading = signal(false);
  protected templates = signal<DetectionTemplateSummary[]>([]);
  protected filter = new FormControl('');
  protected filtered = computed(() => {
    const term = (this.filter.value || '').toLowerCase().trim();
    if (!term) return this.templates();
    return this.templates().filter(t =>
      t.name.toLowerCase().includes(term) || (t.description || '').toLowerCase().includes(term)
    );
  });

  displayedColumns = ['name', 'type', 'version', 'parameters', 'lastUsedAt', 'actions'];

  ngOnInit(): void {
    this.load();
    this.filter.valueChanges.pipe(debounceTime(300)).subscribe(); // computed handles filtering
  }

  load(): void {
    this.loading.set(true);
    this.svc.getAvailable().subscribe({
      next: list => {
        this.templates.set(list);
        this.loading.set(false);
      },
      error: err => {
        console.error('[TemplateList] load failed', err);
        this.loading.set(false);
      }
    });
  }

  viewTemplate(t: DetectionTemplateSummary) {
    this.router.navigate(['detection-templates', t.templateType]);
  }

  createFrom(t: DetectionTemplateSummary) {
    this.router.navigate(['detection-templates', 'create'], { queryParams: { templateType: t.templateType } });
  }
}
