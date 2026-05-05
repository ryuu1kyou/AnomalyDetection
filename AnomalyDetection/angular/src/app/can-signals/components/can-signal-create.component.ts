import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject, takeUntil } from 'rxjs';
import { CanSignalService, CreateCanSignalDto } from '../../shared/services/can-signal.service';

const SYSTEM_TYPES = [
  { value: 1, label: 'エンジン' }, { value: 2, label: 'ブレーキ' },
  { value: 3, label: 'ステアリング' }, { value: 4, label: 'トランスミッション' },
  { value: 5, label: 'ボディ' }, { value: 6, label: 'シャーシ' },
  { value: 7, label: 'HVAC' }, { value: 8, label: 'ライティング' },
  { value: 9, label: 'インフォテインメント' }, { value: 10, label: 'セーフティ' },
  { value: 11, label: 'パワートレイン' }, { value: 12, label: 'ゲートウェイ' },
  { value: 13, label: 'バッテリー' }, { value: 14, label: 'モーター' },
  { value: 15, label: 'インバーター' }, { value: 16, label: 'チャージャー' },
  { value: 17, label: 'ADAS' }, { value: 18, label: 'サスペンション' },
  { value: 19, label: 'エキゾースト' }, { value: 20, label: '燃料' },
];

const DATA_TYPES = [
  { value: 1, label: 'Unsigned（符号なし整数）' },
  { value: 2, label: 'Signed（符号あり整数）' },
  { value: 3, label: 'Float（単精度浮動小数点）' },
  { value: 4, label: 'Double（倍精度浮動小数点）' },
  { value: 5, label: 'Boolean（真偽値）' },
  { value: 6, label: 'ASCII（文字列）' },
];

const BYTE_ORDERS = [
  { value: 0, label: 'Motorola (Big Endian)' },
  { value: 1, label: 'Intel (Little Endian)' },
];

@Component({
  selector: 'app-can-signal-create',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  template: `
    <div class="container-fluid py-4">

      <div class="d-flex align-items-center gap-2 mb-4">
        <button mat-icon-button routerLink="/can-signals" matTooltip="一覧に戻る">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h2 class="mb-0">
          <mat-icon class="me-2 align-middle">add_circle</mat-icon>
          新規CAN信号作成
        </h2>
      </div>

      @if (errorMessage) {
        <mat-card class="error-card mb-3">
          <mat-card-content class="d-flex align-items-center gap-2 py-2">
            <mat-icon color="warn">error</mat-icon>
            <span>{{ errorMessage }}</span>
          </mat-card-content>
        </mat-card>
      }

      <form [formGroup]="form" (ngSubmit)="onSubmit()">

        <!-- 基本情報 -->
        <mat-card class="mb-3">
          <mat-card-header>
            <mat-icon mat-card-avatar>badge</mat-icon>
            <mat-card-title>基本情報</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="row g-3">
              <div class="col-md-6">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>信号名 *</mat-label>
                  <input matInput formControlName="signalName" placeholder="例: ENGINE_SPEED">
                  <mat-hint align="end">{{ form.get('signalName')?.value?.length ?? 0 }}/100</mat-hint>
                  <mat-error>信号名は必須で100文字以内です</mat-error>
                </mat-form-field>
              </div>
              <div class="col-md-6">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>CAN ID *</mat-label>
                  <input matInput formControlName="canId" placeholder="例: 0x100">
                  <mat-hint>16進数形式 (0x0〜0x7FF)</mat-hint>
                  <mat-error>CAN IDは必須で8文字以内です</mat-error>
                </mat-form-field>
              </div>
              <div class="col-md-6" formGroupName="oemCode">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>OEMコード *</mat-label>
                  <input matInput formControlName="code" placeholder="例: TOYOTA">
                  <mat-error>OEMコードは必須です</mat-error>
                </mat-form-field>
              </div>
              <div class="col-md-6" formGroupName="oemCode">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>OEM名称</mat-label>
                  <input matInput formControlName="name" placeholder="例: Toyota Motor Corporation">
                </mat-form-field>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- 信号仕様 -->
        <mat-card class="mb-3">
          <mat-card-header>
            <mat-icon mat-card-avatar>tune</mat-icon>
            <mat-card-title>信号仕様</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="row g-3">
              <div class="col-md-4">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>データ型 *</mat-label>
                  <mat-select formControlName="dataType">
                    @for (dt of dataTypes; track dt.value) {
                      <mat-option [value]="dt.value">{{ dt.label }}</mat-option>
                    }
                  </mat-select>
                  <mat-error>データ型を選択してください</mat-error>
                </mat-form-field>
              </div>
              <div class="col-md-4">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>バイトオーダー</mat-label>
                  <mat-select formControlName="byteOrder">
                    @for (bo of byteOrders; track bo.value) {
                      <mat-option [value]="bo.value">{{ bo.label }}</mat-option>
                    }
                  </mat-select>
                </mat-form-field>
              </div>
              <div class="col-md-4">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>スタートビット</mat-label>
                  <input matInput type="number" formControlName="startBit" min="0" max="63">
                  <mat-hint>0〜63</mat-hint>
                  <mat-error>0〜63の範囲で指定してください</mat-error>
                </mat-form-field>
              </div>
              <div class="col-md-4">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>ビット長 *</mat-label>
                  <input matInput type="number" formControlName="length" min="1" max="64">
                  <mat-hint>1〜64</mat-hint>
                  <mat-error>1〜64の範囲で指定してください</mat-error>
                </mat-form-field>
              </div>
              <div class="col-md-4">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>最小値</mat-label>
                  <input matInput type="number" formControlName="minValue" step="any">
                </mat-form-field>
              </div>
              <div class="col-md-4">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>最大値</mat-label>
                  <input matInput type="number" formControlName="maxValue" step="any">
                </mat-form-field>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- 物理値変換・タイミング -->
        <mat-card class="mb-3">
          <mat-card-header>
            <mat-icon mat-card-avatar>calculate</mat-icon>
            <mat-card-title>物理値変換 / タイミング</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="row g-3">
              <div class="col-md-3">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>係数 (Factor)</mat-label>
                  <input matInput type="number" formControlName="factor" step="any">
                </mat-form-field>
              </div>
              <div class="col-md-3">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>オフセット</mat-label>
                  <input matInput type="number" formControlName="offset" step="any">
                </mat-form-field>
              </div>
              <div class="col-md-3">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>単位</mat-label>
                  <input matInput formControlName="unit" placeholder="例: rpm, km/h">
                  <mat-hint align="end">{{ form.get('unit')?.value?.length ?? 0 }}/20</mat-hint>
                </mat-form-field>
              </div>
              <div class="col-md-3">
                <!-- spacer -->
              </div>
              <div class="col-md-3">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>サイクル時間 (ms) *</mat-label>
                  <input matInput type="number" formControlName="cycleTime" min="1" max="10000">
                  <mat-hint>1〜10000 ms</mat-hint>
                  <mat-error>1〜10000の範囲で指定してください</mat-error>
                </mat-form-field>
              </div>
              <div class="col-md-3">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>タイムアウト (ms) *</mat-label>
                  <input matInput type="number" formControlName="timeoutTime" min="1" max="30000">
                  <mat-hint>1〜30000 ms</mat-hint>
                  <mat-error>1〜30000の範囲で指定してください</mat-error>
                </mat-form-field>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- エンティティ属性 -->
        <mat-card class="mb-3">
          <mat-card-header>
            <mat-icon mat-card-avatar>info</mat-icon>
            <mat-card-title>エンティティ属性</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="row g-3">
              <div class="col-md-6">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>システム種別 *</mat-label>
                  <mat-select formControlName="systemType">
                    @for (st of systemTypes; track st.value) {
                      <mat-option [value]="st.value">{{ st.label }}</mat-option>
                    }
                  </mat-select>
                  <mat-error>システム種別を選択してください</mat-error>
                </mat-form-field>
              </div>
              <div class="col-md-6 d-flex align-items-center">
                <mat-checkbox formControlName="isStandard" class="mb-1">標準信号</mat-checkbox>
              </div>
              <div class="col-md-6">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>有効日</mat-label>
                  <input matInput type="date" formControlName="effectiveDate">
                </mat-form-field>
              </div>
              <div class="col-md-6">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>ソース文書</mat-label>
                  <input matInput formControlName="sourceDocument" placeholder="例: CAN_DB_v1.2.dbc">
                  <mat-hint align="end">{{ form.get('sourceDocument')?.value?.length ?? 0 }}/500</mat-hint>
                </mat-form-field>
              </div>
              <div class="col-12">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>説明</mat-label>
                  <textarea matInput formControlName="description" rows="3"
                    placeholder="信号の概要を記述してください"></textarea>
                  <mat-hint align="end">{{ form.get('description')?.value?.length ?? 0 }}/1000</mat-hint>
                </mat-form-field>
              </div>
              <div class="col-12">
                <mat-form-field appearance="outline" class="w-100">
                  <mat-label>備考</mat-label>
                  <textarea matInput formControlName="notes" rows="3"></textarea>
                  <mat-hint align="end">{{ form.get('notes')?.value?.length ?? 0 }}/2000</mat-hint>
                </mat-form-field>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Actions -->
        <div class="d-flex justify-content-end gap-2">
          <button mat-button type="button" routerLink="/can-signals">キャンセル</button>
          <button mat-raised-button color="primary" type="submit"
            [disabled]="isSubmitting || form.invalid">
            @if (isSubmitting) {
              <mat-spinner diameter="18" class="me-2"></mat-spinner>
            }
            <mat-icon>save</mat-icon>
            作成
          </button>
        </div>

      </form>
    </div>
  `,
  styles: [`
    .error-card { background-color: #fff3e0; border-left: 4px solid #ff9800; }
    mat-card-header mat-icon[mat-card-avatar] {
      font-size: 24px; width: 40px; height: 40px;
      display: flex; align-items: center; justify-content: center;
      background-color: #e3f2fd; border-radius: 50%; color: #1976d2;
    }
  `],
})
export class CanSignalCreateComponent implements OnDestroy {
  protected readonly systemTypes = SYSTEM_TYPES;
  protected readonly dataTypes = DATA_TYPES;
  protected readonly byteOrders = BYTE_ORDERS;
  protected isSubmitting = false;
  protected errorMessage?: string;

  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly canSignalService = inject(CanSignalService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroy$ = new Subject<void>();

  protected readonly form = this.fb.group({
    signalName: ['', [Validators.required, Validators.maxLength(100)]],
    canId: ['', [Validators.required, Validators.maxLength(8)]],
    startBit: [0, [Validators.required, Validators.min(0), Validators.max(63)]],
    length: [8, [Validators.required, Validators.min(1), Validators.max(64)]],
    dataType: [1, Validators.required],
    minValue: [0.0],
    maxValue: [0.0],
    byteOrder: [0],
    factor: [1.0],
    offset: [0.0],
    unit: ['', Validators.maxLength(20)],
    cycleTime: [100, [Validators.required, Validators.min(1), Validators.max(10000)]],
    timeoutTime: [300, [Validators.required, Validators.min(1), Validators.max(30000)]],
    systemType: [1, Validators.required],
    description: ['', Validators.maxLength(1000)],
    oemCode: this.fb.group({
      code: ['', [Validators.required, Validators.maxLength(20)]],
      name: ['', Validators.maxLength(100)],
    }),
    isStandard: [false],
    effectiveDate: [''],
    sourceDocument: ['', Validators.maxLength(500)],
    notes: ['', Validators.maxLength(2000)],
  });

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = undefined;
    this.cdr.markForCheck();

    const v = this.form.getRawValue();
    const dto: CreateCanSignalDto = {
      signalName: v.signalName!,
      canId: v.canId!,
      startBit: v.startBit ?? 0,
      length: v.length ?? 8,
      dataType: v.dataType ?? 1,
      minValue: v.minValue ?? 0,
      maxValue: v.maxValue ?? 0,
      byteOrder: v.byteOrder ?? 0,
      factor: v.factor ?? 1.0,
      offset: v.offset ?? 0.0,
      unit: v.unit ?? '',
      cycleTime: v.cycleTime ?? 100,
      timeoutTime: v.timeoutTime ?? 300,
      systemType: v.systemType ?? 1,
      description: v.description ?? '',
      oemCode: { code: v.oemCode!.code!, name: v.oemCode!.name ?? '' },
      isStandard: v.isStandard ?? false,
      effectiveDate: v.effectiveDate || null,
      sourceDocument: v.sourceDocument ?? '',
      notes: v.notes ?? '',
    };

    this.canSignalService
      .createCanSignal(dto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: created => {
          this.snackBar.open('CAN信号を作成しました', '閉じる', { duration: 3000 });
          void this.router.navigate(['/can-signals', created.id]);
        },
        error: err => {
          this.isSubmitting = false;
          this.errorMessage = err?.error?.error?.message ?? err?.error?.message ?? 'CAN信号の作成に失敗しました。';
          this.cdr.markForCheck();
        },
      });
  }
}
