import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { Subject, takeUntil } from 'rxjs';
import { CanSignalService, CanSignalDto } from '../../shared/services/can-signal.service';

@Component({
  selector: 'app-can-signal-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatChipsModule,
  ],
  template: `
    <div class="container-fluid py-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="mb-0">
          <mat-icon class="me-2 align-middle">sensors</mat-icon>
          CAN信号一覧
        </h2>
        <button mat-raised-button color="primary" (click)="createSignal()">
          <mat-icon>add</mat-icon>
          新規作成
        </button>
      </div>

      @if (isLoading) {
      <div class="text-center py-5">
        <mat-spinner diameter="50" class="mx-auto"></mat-spinner>
        <p class="mt-3 text-muted">読み込み中...</p>
      </div>
      } @else if (errorMessage) {
      <mat-card class="error-card">
        <mat-card-content>
          <div class="d-flex align-items-center">
            <mat-icon color="warn" class="me-2">error</mat-icon>
            <span>{{ errorMessage }}</span>
          </div>
          <button mat-button color="primary" (click)="reload()" class="mt-2">
            <mat-icon>refresh</mat-icon>
            再読み込み
          </button>
        </mat-card-content>
      </mat-card>
      } @else if (canSignals.length === 0) {
      <mat-card>
        <mat-card-content class="text-center py-5">
          <mat-icon class="large-icon text-muted mb-3">sensor_door</mat-icon>
          <p class="text-muted">CAN信号がまだ登録されていません</p>
          <button mat-raised-button color="primary" (click)="createSignal()">
            <mat-icon>add</mat-icon>
            最初の信号を作成
          </button>
        </mat-card-content>
      </mat-card>
      } @else {
      <mat-card>
        <mat-card-content class="p-0">
          <div class="table-responsive">
            <table mat-table [dataSource]="canSignals" class="w-100">
              <ng-container matColumnDef="signalName">
                <th mat-header-cell *matHeaderCellDef>信号名</th>
                <td mat-cell *matCellDef="let signal">
                  <div class="fw-bold">{{ signal.signalName }}</div>
                  <small class="text-muted">{{ signal.description || '説明なし' }}</small>
                </td>
              </ng-container>

              <ng-container matColumnDef="canId">
                <th mat-header-cell *matHeaderCellDef>CAN ID</th>
                <td mat-cell *matCellDef="let signal">
                  <code>{{ signal.canId }}</code>
                </td>
              </ng-container>

              <ng-container matColumnDef="systemType">
                <th mat-header-cell *matHeaderCellDef>システム種別</th>
                <td mat-cell *matCellDef="let signal">
                  <mat-chip class="system-chip">
                    {{ getSystemTypeLabel(signal.systemType) }}
                  </mat-chip>
                </td>
              </ng-container>

              <ng-container matColumnDef="dataType">
                <th mat-header-cell *matHeaderCellDef>データ型</th>
                <td mat-cell *matCellDef="let signal">
                  {{ getDataTypeLabel(signal.dataType) }}
                </td>
              </ng-container>

              <ng-container matColumnDef="bitInfo">
                <th mat-header-cell *matHeaderCellDef>ビット情報</th>
                <td mat-cell *matCellDef="let signal">
                  @if (signal.startBit !== undefined && signal.bitLength !== undefined) {
                  <span>{{ signal.startBit }}bit / {{ signal.bitLength }}bit</span>
                  } @else {
                  <span class="text-muted">-</span>
                  }
                </td>
              </ng-container>

              <ng-container matColumnDef="range">
                <th mat-header-cell *matHeaderCellDef>範囲</th>
                <td mat-cell *matCellDef="let signal">
                  @if (signal.minValue !== undefined || signal.maxValue !== undefined) {
                  <span>{{ signal.minValue ?? '-' }} ~ {{ signal.maxValue ?? '-' }}</span>
                  @if (signal.unit) {
                  <span class="ms-1">{{ signal.unit }}</span>
                  } } @else {
                  <span class="text-muted">-</span>
                  }
                </td>
              </ng-container>

              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef class="text-end">操作</th>
                <td mat-cell *matCellDef="let signal" class="text-end">
                  <button
                    mat-icon-button
                    [routerLink]="['/can-signals', signal.id]"
                    matTooltip="詳細を表示"
                  >
                    <mat-icon>visibility</mat-icon>
                  </button>
                  <button
                    mat-icon-button
                    [routerLink]="['/can-signals', signal.id, 'edit']"
                    matTooltip="編集"
                  >
                    <mat-icon>edit</mat-icon>
                  </button>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr
                mat-row
                *matRowDef="let row; columns: displayedColumns"
                class="clickable-row"
                (click)="viewDetail(row)"
              ></tr>
            </table>
          </div>

          @if (totalCount > canSignals.length) {
          <div class="p-3 text-center border-top">
            <small class="text-muted">
              {{ canSignals.length }} / {{ totalCount }} 件を表示中
            </small>
          </div>
          }
        </mat-card-content>
      </mat-card>
      }
    </div>
  `,
  styles: [
    `
      .large-icon {
        font-size: 64px;
        width: 64px;
        height: 64px;
      }

      .error-card {
        background-color: #fff3e0;
        border-left: 4px solid #ff9800;
      }

      .clickable-row {
        cursor: pointer;
        transition: background-color 0.2s;
      }

      .clickable-row:hover {
        background-color: rgba(0, 0, 0, 0.04);
      }

      .system-chip {
        height: 24px;
        font-size: 12px;
      }

      code {
        background-color: #f5f5f5;
        padding: 2px 6px;
        border-radius: 3px;
        font-size: 13px;
      }
    `,
  ],
})
export class CanSignalListComponent implements OnInit, OnDestroy {
  protected canSignals: CanSignalDto[] = [];
  protected totalCount = 0;
  protected isLoading = false;
  protected errorMessage?: string;
  protected displayedColumns = [
    'signalName',
    'canId',
    'systemType',
    'dataType',
    'bitInfo',
    'range',
    'actions',
  ];

  private readonly canSignalService = inject(CanSignalService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroy$ = new Subject<void>();
  private readonly pageSize = 50;

  ngOnInit(): void {
    this.loadCanSignals();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  protected createSignal(): void {
    void this.router.navigate(['/can-signals/create']);
  }

  protected viewDetail(signal: CanSignalDto): void {
    void this.router.navigate(['/can-signals', signal.id]);
  }

  protected reload(): void {
    this.loadCanSignals();
  }

  protected getSystemTypeLabel(type?: number): string {
    const labels: Record<number, string> = {
      1: 'エンジン',
      2: 'ブレーキ',
      3: 'ステアリング',
      4: 'トランスミッション',
      5: 'ボディ',
      6: 'シャーシ',
      7: 'HVAC',
      8: 'ライティング',
      9: 'インフォテインメント',
      10: 'セーフティ',
      11: 'パワートレイン',
      12: 'ゲートウェイ',
      13: 'バッテリー',
      14: 'モーター',
      15: 'インバーター',
      16: 'チャージャー',
      17: 'ADAS',
      18: 'サスペンション',
      19: 'エキゾースト',
      20: '燃料',
    };
    return type !== undefined ? labels[type] ?? '不明' : '未設定';
  }

  protected getDataTypeLabel(type?: number): string {
    const labels: Record<number, string> = {
      1: 'Unsigned',
      2: 'Signed',
      3: 'Float',
      4: 'Double',
      5: 'Boolean',
      6: 'ASCII',
    };
    return type !== undefined ? labels[type] ?? '不明' : '未設定';
  }

  private loadCanSignals(): void {
    this.isLoading = true;
    this.errorMessage = undefined;
    this.cdr.markForCheck();

    this.canSignalService
      .getCanSignals({
        skipCount: 0,
        maxResultCount: this.pageSize,
        sorting: 'CreationTime DESC',
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.canSignals = response.items;
          this.totalCount = response.totalCount;
          this.isLoading = false;
          this.cdr.markForCheck();
        },
        error: error => {
          this.canSignals = [];
          this.isLoading = false;
          this.errorMessage = error?.error?.message ?? 'CAN信号一覧の取得に失敗しました。';
          this.cdr.markForCheck();
        },
      });
  }
}
