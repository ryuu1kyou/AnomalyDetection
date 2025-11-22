import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DetectionTemplatesService, DetectionTemplateSummary, TemplateParameterDescriptor } from '../../services/detection-templates.service';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-template-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, MatCardModule, MatButtonModule, MatInputModule, MatIconModule, MatSnackBarModule],
  templateUrl: './template-form.component.html',
  styleUrls: ['./template-form.component.scss']
})
export class TemplateFormComponent {
  private route = inject(ActivatedRoute);
  private svc = inject(DetectionTemplatesService);
  private fb = inject(FormBuilder);
  private snack = inject(MatSnackBar);
  private router = inject(Router);

  protected template = signal<DetectionTemplateSummary | null>(null);
  public form = this.fb.group({
    templateType: [''], // store as string for form binding; cast when submitting
    parameters: this.fb.group({})
  });
  protected validationStatus = signal<'idle' | 'valid' | 'invalid' | 'checking'>('idle');
  protected validationErrors = signal<string[]>([]);
  protected loading = signal(false);
  protected creating = signal(false);

  ngOnInit() {
    const templateTypeParam = this.route.snapshot.queryParamMap.get('templateType');
    if (templateTypeParam) {
      const tType = Number(templateTypeParam);
      this.loading.set(true);
      this.svc.getByType(tType).subscribe({
        next: t => {
          this.template.set(t);
          this.form.patchValue({ templateType: String(t.templateType) });
          // build dynamic parameter controls
          const paramsGroup: Record<string, any> = {};
          t.parameters.forEach(p => {
            paramsGroup[p.name] = [p.defaultValue ?? '', this.buildValidators(p)];
          });
          this.form.setControl('parameters', this.fb.group(paramsGroup));
          this.loading.set(false);
        },
        error: err => { console.error('[TemplateForm] load failed', err); this.loading.set(false); }
      });
    }
  }

  buildValidators(p: TemplateParameterDescriptor) {
    const validators = [] as any[];
    if (p.required) {
      validators.push((control: any) => (control.value === null || control.value === '' ? { required: true } : null));
    }
    if (p.type === 'number') {
      validators.push((control: any) => isNaN(Number(control.value)) ? { number: true } : null);
      if (typeof p.min === 'number') {
        validators.push((control: any) => Number(control.value) < p.min! ? { min: p.min } : null);
      }
      if (typeof p.max === 'number') {
        validators.push((control: any) => Number(control.value) > p.max! ? { max: p.max } : null);
      }
    }
    return validators;
  }

  validate() {
    if (!this.template()) return;
    if (this.form.invalid) {
      this.validationStatus.set('invalid');
      this.validationErrors.set(['フォームの入力エラーを修正してください']);
      return;
    }
    this.validationStatus.set('checking');
    const raw = (this.form.value.parameters as any) || {};
    this.svc.validateParameters({ templateType: this.template()!.templateType, parameters: raw }).subscribe({
      next: res => {
        if (res.isValid) {
          this.validationStatus.set('valid');
          this.validationErrors.set([]);
          this.snack.open('パラメータ検証 OK', 'OK', { duration: 2500 });
        } else {
          this.validationStatus.set('invalid');
          this.validationErrors.set(res.errors);
          this.snack.open('パラメータ検証エラー', '閉じる', { duration: 4000 });
        }
      },
      error: err => {
        console.error('[TemplateForm] validate failed', err);
        this.validationStatus.set('invalid');
        this.validationErrors.set(['検証 API 失敗']);
        this.snack.open('検証失敗', '閉じる', { duration: 4000 });
      }
    });
  }

  submit() {
    if (!this.template()) return;
    this.creating.set(true);
    const overrides = (this.form.value.parameters as any) || {};
  this.svc.createFromTemplate({ templateType: this.template()!.templateType, overrides })
      .subscribe({
        next: res => {
          this.creating.set(false);
            this.snack.open('Detection logic created from template', 'OK', { duration: 3000 });
            // navigate to detection-logics list highlighting new item (backend should respond with id ideally)
            this.router.navigate(['/detection-logics'], { queryParams: { created: 'true' } });
        },
        error: err => {
          console.error('[TemplateForm] create failed', err);
          this.snack.open('Create failed', 'Dismiss', { duration: 4000 });
          this.creating.set(false);
        }
      });
  }
}
