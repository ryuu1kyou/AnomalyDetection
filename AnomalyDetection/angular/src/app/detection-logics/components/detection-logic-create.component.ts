import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, switchMap, takeUntil, tap } from 'rxjs';
import {
  DetectionLogicService,
  DetectionTemplateSummary,
  TemplateParameterDefinition,
} from '../services/detection-logic.service';
import { DetectionType } from '../../detection-results/models/detection-result.model';
import { CanSignalService, CanSignalLookup } from '../../shared/services/can-signal.service';

interface DetectionTypeOption {
  value: DetectionType;
  label: string;
  description: string;
}

@Component({
  selector: 'app-detection-logic-create',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    NgIf,
    NgFor,
    AsyncPipe,
  ],
  templateUrl: './detection-logic-create.component.html',
  styleUrls: ['./detection-logic-create.component.scss'],
})
export class DetectionLogicCreateComponent implements OnDestroy {
  readonly DetectionType = DetectionType;

  protected readonly detectionTypeOptions: DetectionTypeOption[] = [
    {
      value: DetectionType.OutOfRange,
      label: 'しきい値逸脱 (Out of Range)',
      description: '信号値が設定した最小/最大範囲から逸脱した場合に検知します。',
    },
    {
      value: DetectionType.RateOfChange,
      label: '変化率監視 (Rate of Change)',
      description: '指定時間内の変化量を監視し、急激な変化を検知します。',
    },
    {
      value: DetectionType.Timeout,
      label: 'タイムアウト (Timeout)',
      description: '所定時間内に信号が更新されない場合に異常として扱います。',
    },
    {
      value: DetectionType.Stuck,
      label: '値張り付き (Stuck Value)',
      description: '信号値が一定範囲内に長時間留まり続ける場合に検知します。',
    },
  ];

  private readonly fb = inject(FormBuilder);
  private readonly detectionLogicService = inject(DetectionLogicService);
  private readonly canSignalService = inject(CanSignalService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly router = inject(Router);

  private readonly guidValidator: ValidatorFn = (
    control: AbstractControl
  ): ValidationErrors | null => {
    const value = control.value as string;
    if (!value) {
      return null;
    }

    const guidRegex =
      /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/;
    return guidRegex.test(value) ? null : { guid: true };
  };

  protected readonly form: FormGroup = this.fb.group({
    logicName: ['', [Validators.required, Validators.maxLength(200)]],
    detectionType: [DetectionType.OutOfRange, Validators.required],
    templateType: [null, Validators.required],
    canSignalId: ['', [Validators.required, this.guidValidator]],
    parameters: this.fb.group({}),
  });
  protected readonly signalSearchControl = new FormControl('', { nonNullable: true });
  protected parameterDefinitions: TemplateParameterDefinition[] = [];
  protected templates: DetectionTemplateSummary[] = [];
  protected selectedTemplate?: DetectionTemplateSummary;
  protected signalOptions: CanSignalLookup[] = [];
  protected isLoadingTemplates = false;
  protected isSaving = false;
  protected isLoadingSignals = false;

  private readonly destroy$ = new Subject<void>();

  constructor() {
    this.registerFormListeners();
    this.registerSignalSearch();
    this.loadTemplates(DetectionType.OutOfRange);
    this.loadSignals('');
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  protected get selectedDetectionTypeOption(): DetectionTypeOption | undefined {
    const value = this.form.get('detectionType')?.value as DetectionType | null;
    if (value === null || value === undefined) {
      return undefined;
    }
    return this.detectionTypeOptions.find(option => option.value === value);
  }

  protected trackByParamName(_: number, param: TemplateParameterDefinition): string {
    return param.name;
  }

  protected onSubmit(): void {
    if (this.form.invalid || !this.selectedTemplate) {
      this.form.markAllAsTouched();
      return;
    }

    const detectionType = this.form.get('detectionType')!.value as DetectionType;
    const logicName = (this.form.get('logicName')!.value as string).trim();
    const canSignalId = this.form.get('canSignalId')!.value as string;

    if (!logicName || !canSignalId) {
      return;
    }

    const parametersGroup = this.form.get('parameters') as FormGroup;
    const templateParameters: Record<string, unknown> = {};

    for (const definition of this.parameterDefinitions) {
      const control = parametersGroup.get(definition.name);
      if (!control) {
        continue;
      }

      const value = control.value;

      if (value === null || value === undefined || value === '') {
        if (definition.required) {
          control.markAsTouched();
          return;
        }
        continue;
      }

      templateParameters[definition.name] = this.normalizeParameterValue(definition, value);
    }

    this.isSaving = true;
    this.detectionLogicService
      .createFromTemplate(detectionType, {
        logicName,
        canSignalId,
        templateParameters,
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.isSaving = false;
          this.snackBar.open(
            `テンプレートから検出ロジック「${result.name}」を作成しました。`,
            '閉じる',
            {
              duration: 4500,
            }
          );
          void this.router.navigate(['/detection-logics'], {
            queryParams: { created: result.id },
          });
          this.cdr.markForCheck();
        },
        error: error => {
          this.isSaving = false;
          const message =
            error?.error?.message ?? 'ロジックの作成に失敗しました。入力内容を確認してください。';
          this.snackBar.open(message, '閉じる', { duration: 6000 });
          this.cdr.markForCheck();
        },
      });
  }

  protected onTemplateSelected(templateType: number | null): void {
    const template = this.templates.find(item => item.type === templateType);
    this.selectedTemplate = template;
    this.parameterDefinitions = template?.parameterDefinitions ?? [];
    this.buildParameterControls(template);
    this.cdr.markForCheck();
  }

  private registerFormListeners(): void {
    this.form
      .get('detectionType')!
      .valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(value => {
        if (value === undefined || value === null) {
          return;
        }
        this.templates = [];
        this.selectedTemplate = undefined;
        this.parameterDefinitions = [];
        this.form.get('templateType')!.reset();
        this.buildParameterControls(undefined);
        this.loadTemplates(value as DetectionType);
      });

    this.form
      .get('templateType')!
      .valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(value => this.onTemplateSelected(value as number | null));
  }

  private registerSignalSearch(): void {
    this.signalSearchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        tap(() => {
          this.isLoadingSignals = true;
          this.cdr.markForCheck();
        }),
        switchMap(term => this.canSignalService.searchCanSignals(term ?? '')),
        takeUntil(this.destroy$)
      )
      .subscribe(options => {
        this.signalOptions = options;
        this.isLoadingSignals = false;
        this.cdr.markForCheck();
      });
  }

  private loadTemplates(detectionType: DetectionType): void {
    this.isLoadingTemplates = true;
    this.cdr.markForCheck();

    this.detectionLogicService
      .getTemplates(detectionType)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: templates => {
          this.templates = templates;
          const firstTemplate = templates[0];
          this.isLoadingTemplates = false;
          if (firstTemplate) {
            this.form.get('templateType')!.setValue(firstTemplate.type, { emitEvent: false });
            this.onTemplateSelected(firstTemplate.type);
          } else {
            this.selectedTemplate = undefined;
            this.parameterDefinitions = [];
            this.buildParameterControls(undefined);
          }
          this.cdr.markForCheck();
        },
        error: error => {
          console.error('Failed to load detection templates', error);
          this.isLoadingTemplates = false;
          this.snackBar.open(
            'テンプレート一覧の取得に失敗しました。権限または接続を確認してください。',
            '閉じる',
            {
              duration: 6000,
            }
          );
          this.cdr.markForCheck();
        },
      });
  }

  private loadSignals(term: string): void {
    this.isLoadingSignals = true;
    this.cdr.markForCheck();

    this.canSignalService
      .searchCanSignals(term)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: options => {
          this.signalOptions = options;
          this.isLoadingSignals = false;
          this.cdr.markForCheck();
        },
        error: error => {
          console.error('Failed to load CAN signals', error);
          this.isLoadingSignals = false;
          this.snackBar.open(
            'CAN信号の取得に失敗しました。権限やフィルタ条件を確認してください。',
            '閉じる',
            {
              duration: 6000,
            }
          );
          this.cdr.markForCheck();
        },
      });
  }

  private buildParameterControls(template: DetectionTemplateSummary | undefined): void {
    const parametersGroup = this.fb.group({});

    if (template) {
      for (const definition of template.parameterDefinitions) {
        const defaultValue =
          template.defaultParameters?.[definition.name] ?? definition.defaultValue ?? '';
        const validators = this.createValidators(definition);
        parametersGroup.addControl(definition.name, new FormControl(defaultValue, validators));
      }
    }

    this.form.setControl('parameters', parametersGroup);
    this.cdr.markForCheck();
  }

  private createValidators(definition: TemplateParameterDefinition): ValidatorFn[] {
    const validators: ValidatorFn[] = [];
    if (definition.required) {
      validators.push(Validators.required);
    }

    if (definition.type === 'number' || definition.type === 'integer') {
      if (definition.minValue !== undefined) {
        validators.push(Validators.min(definition.minValue));
      }
      if (definition.maxValue !== undefined) {
        validators.push(Validators.max(definition.maxValue));
      }
    }

    if (definition.minLength !== undefined) {
      validators.push(Validators.minLength(definition.minLength));
    }
    if (definition.maxLength !== undefined) {
      validators.push(Validators.maxLength(definition.maxLength));
    }

    return validators;
  }

  private normalizeParameterValue(
    definition: TemplateParameterDefinition,
    value: unknown
  ): unknown {
    switch (definition.type) {
      case 'integer':
        return typeof value === 'number' ? Math.trunc(value) : parseInt(value as string, 10);
      case 'number':
        return typeof value === 'number' ? value : parseFloat(value as string);
      case 'boolean':
        if (typeof value === 'boolean') {
          return value;
        }
        if (typeof value === 'string') {
          return value.toLowerCase() === 'true' || value === '1';
        }
        return Boolean(value);
      default:
        return value;
    }
  }
}
