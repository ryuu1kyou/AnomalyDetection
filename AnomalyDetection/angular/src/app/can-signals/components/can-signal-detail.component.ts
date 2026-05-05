import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, takeUntil } from 'rxjs';
import { CanSignalService, CanSignalDto } from '../../shared/services/can-signal.service';

@Component({
  selector: 'app-can-signal-detail',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  template: `
    <div class="container-fluid py-4">

      <!-- ヘッダー -->
      <div class="d-flex align-items-center gap-2 mb-4">
        <button mat-icon-button routerLink="/can-signals" matTooltip="一覧に戻る">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h2 class="mb-0 flex-grow-1">
          <mat-icon class="me-2 align-middle">sensors</mat-icon>
          CAN信号詳細
        </h2>
        @if (signal) {
          <button mat-stroked-button [routerLink]="['/can-signals', signal.id, 'edit']">
            <mat-icon>edit</mat-icon>
            編集
          </button>
        }
      </div>

      <!-- ローディング -->
      @if (isLoading) {
        <div class="text-center py-5">
          <mat-spinner diameter="50" class="mx-auto"></mat-spinner>
          <p class="mt-3 text-muted">読み込み中...</p>
        </div>
      }

      <!-- エラー -->
      @if (errorMessage) {
        <mat-card class="error-card mb-3">
          <mat-card-content class="d-flex align-items-center gap-2 py-2">
            <mat-icon color="warn">error</mat-icon>
            <span>{{ errorMessage }}</span>
            <button mat-button color="primary" (click)="reload()">
              <mat-icon>refresh</mat-icon>再読み込み
            </button>
          </mat-card-content>
        </mat-card>
      }

      @if (signal) {
        <!-- ステータスヘッダーカード -->
        <mat-card class="mb-3">
          <mat-card-content class="py-3">
            <div class="d-flex flex-wrap align-items-center gap-3">
              <div>
                <div class="text-muted small">信号名</div>
                <div class="fs-5 fw-bold">{{ signal.signalName }}</div>
              </div>
              <div>
                <div class="text-muted small">CAN ID</div>
                <code class="fs-6">{{ signal.canId }}</code>
              </div>
              <div class="ms-auto d-flex gap-2 flex-wrap">
                <mat-chip [class]="getStatusClass(signal.status)">
                  <mat-icon matChipAvatar>{{ getStatusIcon(signal.status) }}</mat-icon>
                  {{ getStatusLabel(signal.status) }}
                </mat-chip>
                @if (signal.isStandard) {
                  <mat-chip class="chip-standard">
                    <mat-icon matChipAvatar>star</mat-icon>
                    標準信号
                  </mat-chip>
                }
                <mat-chip class="chip-system">{{ getSystemTypeLabel(signal.systemType) }}</mat-chip>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <div class="row g-3">

          <!-- 信号仕様 -->
          <div class="col-md-6">
            <mat-card class="h-100">
              <mat-card-header>
                <mat-icon mat-card-avatar>tune</mat-icon>
                <mat-card-title>信号仕様</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <table class="detail-table w-100">
                  <tr>
                    <th>データ型</th>
                    <td>{{ getDataTypeLabel(signal.dataType) }}</td>
                  </tr>
                  <tr>
                    <th>バイトオーダー</th>
                    <td>{{ getByteOrderLabel(signal.byteOrder) }}</td>
                  </tr>
                  <tr>
                    <th>スタートビット</th>
                    <td>{{ signal.startBit }} bit</td>
                  </tr>
                  <tr>
                    <th>ビット長</th>
                    <td>{{ signal.length }} bit</td>
                  </tr>
                  <tr>
                    <th>最小値</th>
                    <td>{{ signal.minValue }}</td>
                  </tr>
                  <tr>
                    <th>最大値</th>
                    <td>{{ signal.maxValue }}</td>
                  </tr>
                </table>
              </mat-card-content>
            </mat-card>
          </div>

          <!-- 物理値変換 -->
          <div class="col-md-6">
            <mat-card class="h-100">
              <mat-card-header>
                <mat-icon mat-card-avatar>calculate</mat-icon>
                <mat-card-title>物理値変換</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <table class="detail-table w-100">
                  <tr>
                    <th>係数 (Factor)</th>
                    <td>{{ signal.factor }}</td>
                  </tr>
                  <tr>
                    <th>オフセット</th>
                    <td>{{ signal.offset }}</td>
                  </tr>
                  <tr>
                    <th>単位</th>
                    <td>{{ signal.unit || '—' }}</td>
                  </tr>
                  <tr>
                    <th>サイクル時間</th>
                    <td>{{ signal.cycleTime }} ms</td>
                  </tr>
                  <tr>
                    <th>タイムアウト</th>
                    <td>{{ signal.timeoutTime }} ms</td>
                  </tr>
                  <tr>
                    <th>バージョン</th>
                    <td>{{ signal.version || '—' }}</td>
                  </tr>
                </table>
              </mat-card-content>
            </mat-card>
          </div>

          <!-- エンティティ属性 -->
          <div class="col-md-6">
            <mat-card class="h-100">
              <mat-card-header>
                <mat-icon mat-card-avatar>info</mat-icon>
                <mat-card-title>エンティティ属性</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <table class="detail-table w-100">
                  <tr>
                    <th>OEMコード</th>
                    <td>
                      @if (signal.oemCode?.code) {
                        <span class="fw-bold">{{ signal.oemCode.code }}</span>
                        @if (signal.oemCode.name) {
                          <span class="text-muted ms-1">({{ signal.oemCode.name }})</span>
                        }
                      } @else { — }
                    </td>
                  </tr>
                  <tr>
                    <th>有効日</th>
                    <td>{{ signal.effectiveDate ? (signal.effectiveDate | date:'yyyy/MM/dd') : '—' }}</td>
                  </tr>
                  <tr>
                    <th>説明</th>
                    <td class="description-cell">{{ signal.description || '—' }}</td>
                  </tr>
                  <tr>
                    <th>ソース文書</th>
                    <td class="description-cell">{{ signal.sourceDocument || '—' }}</td>
                  </tr>
                  <tr>
                    <th>備考</th>
                    <td class="description-cell">{{ signal.notes || '—' }}</td>
                  </tr>
                </table>
              </mat-card-content>
            </mat-card>
          </div>

          <!-- トレサビ -->
          <div class="col-md-6">
            <mat-card class="h-100 traceability-card">
              <mat-card-header>
                <mat-icon mat-card-avatar>account_tree</mat-icon>
                <mat-card-title>トレサビ / 資産共通化</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <table class="detail-table w-100">
                  <tr>
                    <th>Feature ID</th>
                    <td>
                      @if (signal.featureId) {
                        <code class="feature-code">{{ signal.featureId }}</code>
                      } @else {
                        <span class="text-warning d-flex align-items-center gap-1">
                          <mat-icon class="small-icon">warning</mat-icon>未設定
                        </span>
                      }
                    </td>
                  </tr>
                  <tr>
                    <th>Decision ID</th>
                    <td>
                      @if (signal.decisionId) {
                        <code class="feature-code">{{ signal.decisionId }}</code>
                      } @else { — }
                    </td>
                  </tr>
                  <tr>
                    <th>共通化分類</th>
                    <td>
                      <mat-chip [class]="getCommonalityClass(signal.commonalityStatus)">
                        {{ getCommonalityLabel(signal.commonalityStatus) }}
                      </mat-chip>
                    </td>
                  </tr>
                  @if (signal.commonalityStatus === 0) {
                    <tr>
                      <th>解決期限</th>
                      <td>
                        @if (signal.unknownResolutionDueDate) {
                          <span [class]="isDueDatePast(signal.unknownResolutionDueDate) ? 'text-danger fw-bold' : ''">
                            {{ signal.unknownResolutionDueDate | date:'yyyy/MM/dd' }}
                            @if (isDueDatePast(signal.unknownResolutionDueDate)) {
                              <mat-icon class="small-icon ms-1">schedule</mat-icon>期限超過
                            }
                          </span>
                        } @else {
                          <span class="text-danger fw-bold">
                            <mat-icon class="small-icon">error</mat-icon> 期限未設定
                          </span>
                        }
                      </td>
                    </tr>
                  }
                </table>
              </mat-card-content>
            </mat-card>
          </div>

          <!-- 監査情報 -->
          <div class="col-12">
            <mat-card>
              <mat-card-header>
                <mat-icon mat-card-avatar>history</mat-icon>
                <mat-card-title>監査情報</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="row">
                  <div class="col-md-6">
                    <table class="detail-table w-100">
                      <tr>
                        <th>作成日時</th>
                        <td>{{ signal.creationTime ? (signal.creationTime | date:'yyyy/MM/dd HH:mm') : '—' }}</td>
                      </tr>
                      <tr>
                        <th>作成者</th>
                        <td>{{ signal.creatorId || '—' }}</td>
                      </tr>
                    </table>
                  </div>
                  <div class="col-md-6">
                    <table class="detail-table w-100">
                      <tr>
                        <th>最終更新</th>
                        <td>{{ signal.lastModificationTime ? (signal.lastModificationTime | date:'yyyy/MM/dd HH:mm') : '—' }}</td>
                      </tr>
                      <tr>
                        <th>更新者</th>
                        <td>{{ signal.lastModifierId || '—' }}</td>
                      </tr>
                    </table>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>
          </div>

        </div>
      }

    </div>
  `,
  styles: [`
    .error-card {
      background-color: #fff3e0;
      border-left: 4px solid #ff9800;
    }

    .traceability-card {
      border-left: 3px solid #1976d2;
    }

    .detail-table {
      border-collapse: collapse;
    }
    .detail-table th {
      width: 38%;
      padding: 6px 12px 6px 0;
      font-weight: 500;
      color: rgba(0,0,0,.6);
      font-size: 13px;
      vertical-align: top;
      white-space: nowrap;
    }
    .detail-table td {
      padding: 6px 0;
      font-size: 14px;
      vertical-align: top;
    }
    .detail-table tr + tr th,
    .detail-table tr + tr td {
      border-top: 1px solid rgba(0,0,0,.06);
    }

    .description-cell {
      white-space: pre-wrap;
      word-break: break-word;
    }

    code {
      background-color: #f5f5f5;
      padding: 2px 6px;
      border-radius: 3px;
      font-size: 13px;
    }
    .feature-code {
      background-color: #e3f2fd;
      color: #1565c0;
    }

    .chip-standard { background-color: #fff8e1 !important; color: #f57f17 !important; }
    .chip-system   { background-color: #e8f5e9 !important; color: #2e7d32 !important; }
    .chip-active   { background-color: #e8f5e9 !important; color: #2e7d32 !important; }
    .chip-inactive { background-color: #f5f5f5 !important; color: #616161 !important; }
    .chip-testing  { background-color: #e3f2fd !important; color: #1565c0 !important; }
    .chip-deprecated { background-color: #fce4ec !important; color: #c62828 !important; }

    .chip-unknown   { background-color: #fff8e1 !important; color: #e65100 !important; }
    .chip-core      { background-color: #e8f5e9 !important; color: #1b5e20 !important; }
    .chip-baseline  { background-color: #e3f2fd !important; color: #0d47a1 !important; }
    .chip-customer  { background-color: #f3e5f5 !important; color: #4a148c !important; }
    .chip-vehicle   { background-color: #e0f2f1 !important; color: #004d40 !important; }

    .small-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
      vertical-align: middle;
    }

    mat-card-header mat-icon[mat-card-avatar] {
      font-size: 24px;
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      background-color: #e3f2fd;
      border-radius: 50%;
      color: #1976d2;
    }
  `],
})
export class CanSignalDetailComponent implements OnInit, OnDestroy {
  protected signal?: CanSignalDto;
  protected isLoading = false;
  protected errorMessage?: string;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly canSignalService = inject(CanSignalService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const id = params.get('id');
      if (id) this.loadSignal(id);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  protected reload(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.loadSignal(id);
  }

  protected getStatusLabel(status?: number): string {
    const labels: Record<number, string> = {
      0: 'テスト中',
      1: 'アクティブ',
      2: '非アクティブ',
      3: '廃止',
    };
    return status !== undefined ? (labels[status] ?? '不明') : '—';
  }

  protected getStatusIcon(status?: number): string {
    const icons: Record<number, string> = {
      0: 'science',
      1: 'check_circle',
      2: 'pause_circle',
      3: 'cancel',
    };
    return status !== undefined ? (icons[status] ?? 'help') : 'help';
  }

  protected getStatusClass(status?: number): string {
    const classes: Record<number, string> = {
      0: 'chip-testing',
      1: 'chip-active',
      2: 'chip-inactive',
      3: 'chip-deprecated',
    };
    return status !== undefined ? (classes[status] ?? '') : '';
  }

  protected getSystemTypeLabel(type?: number): string {
    const labels: Record<number, string> = {
      1: 'エンジン', 2: 'ブレーキ', 3: 'ステアリング', 4: 'トランスミッション',
      5: 'ボディ', 6: 'シャーシ', 7: 'HVAC', 8: 'ライティング',
      9: 'インフォテインメント', 10: 'セーフティ', 11: 'パワートレイン',
      12: 'ゲートウェイ', 13: 'バッテリー', 14: 'モーター', 15: 'インバーター',
      16: 'チャージャー', 17: 'ADAS', 18: 'サスペンション', 19: 'エキゾースト', 20: '燃料',
    };
    return type !== undefined ? (labels[type] ?? '不明') : '未設定';
  }

  protected getDataTypeLabel(type?: number): string {
    const labels: Record<number, string> = {
      1: 'Unsigned', 2: 'Signed', 3: 'Float', 4: 'Double', 5: 'Boolean', 6: 'ASCII',
    };
    return type !== undefined ? (labels[type] ?? '不明') : '未設定';
  }

  protected getByteOrderLabel(order?: number): string {
    const labels: Record<number, string> = { 0: 'Motorola (Big Endian)', 1: 'Intel (Little Endian)' };
    return order !== undefined ? (labels[order] ?? '不明') : '未設定';
  }

  protected getCommonalityLabel(status?: number): string {
    const labels: Record<number, string> = {
      0: 'Unknown（判断保留）',
      1: 'Core（共通核）',
      2: 'BaselineDerived（参照ベース派生）',
      3: 'CustomerSpecific（OEM固有）',
      4: 'VehicleSpecific（車種固有）',
    };
    return status !== undefined ? (labels[status] ?? '不明') : '未設定';
  }

  protected getCommonalityClass(status?: number): string {
    const classes: Record<number, string> = {
      0: 'chip-unknown',
      1: 'chip-core',
      2: 'chip-baseline',
      3: 'chip-customer',
      4: 'chip-vehicle',
    };
    return status !== undefined ? (classes[status] ?? '') : '';
  }

  protected isDueDatePast(dateStr?: string): boolean {
    if (!dateStr) return false;
    return new Date(dateStr) < new Date();
  }

  private loadSignal(id: string): void {
    this.isLoading = true;
    this.errorMessage = undefined;
    this.signal = undefined;
    this.cdr.markForCheck();

    this.canSignalService
      .getCanSignal(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: signal => {
          this.signal = signal;
          this.isLoading = false;
          this.cdr.markForCheck();
        },
        error: err => {
          this.isLoading = false;
          this.errorMessage = err?.error?.message ?? 'CAN信号の取得に失敗しました。';
          this.cdr.markForCheck();
        },
      });
  }
}
