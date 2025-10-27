import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { Subject, takeUntil, switchMap } from 'rxjs';
import { Chart, ChartConfiguration, ChartType, registerables } from 'chart.js';

import { DetectionResultService } from '../services/detection-result.service';
import { 
  AnomalyDetectionResult, 
  AnomalyLevel,
  ResolutionStatus,
  SharingLevel,
  DetectionType,
  CanSystemType,
  ResolveDetectionResultDto,
  MarkAsFalsePositiveDto,
  ReopenDetectionResultDto
} from '../models/detection-result.model';

Chart.register(...registerables);

@Component({
  selector: 'app-detection-result-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatTooltipModule,
    MatExpansionModule,
    MatDividerModule,
    MatListModule,
    MatMenuModule
  ],
  template: `
    <div class="detection-result-detail" *ngIf="result">
      <!-- Header -->
      <div class="header">
        <div class="header-info">
          <button mat-icon-button (click)="goBack()" class="back-button">
            <mat-icon>arrow_back</mat-icon>
          </button>
          <div class="title-section">
            <h2>異常検出結果詳細</h2>
            <div class="result-id">ID: {{ result.id }}</div>
          </div>
        </div>
        <div class="header-actions">
          <button mat-raised-button color="primary" (click)="refreshData()">
            <mat-icon>refresh</mat-icon>
            更新
          </button>
          <button mat-button [matMenuTriggerFor]="actionMenu">
            <mat-icon>more_vert</mat-icon>
            操作
          </button>
          <mat-menu #actionMenu="matMenu">
            <button mat-menu-item (click)="markAsInvestigating()" 
                    *ngIf="result.resolutionStatus === ResolutionStatus.Open">
              <mat-icon>search</mat-icon>
              調査中にする
            </button>
            <button mat-menu-item (click)="openResolveDialog()"
                    *ngIf="result.resolutionStatus !== ResolutionStatus.Resolved">
              <mat-icon>check_circle</mat-icon>
              解決する
            </button>
            <button mat-menu-item (click)="openFalsePositiveDialog()"
                    *ngIf="result.resolutionStatus !== ResolutionStatus.FalsePositive">
              <mat-icon>block</mat-icon>
              誤検出にする
            </button>
            <button mat-menu-item (click)="openReopenDialog()"
                    *ngIf="result.resolutionStatus === ResolutionStatus.Resolved">
              <mat-icon>replay</mat-icon>
              再開する
            </button>
            <button mat-menu-item (click)="openShareDialog()"
                    *ngIf="!result.isShared">
              <mat-icon>share</mat-icon>
              共有する
            </button>
          </mat-menu>
        </div>
      </div>

      <!-- Status Cards -->
      <div class="status-cards">
        <mat-card class="status-card">
          <mat-card-header>
            <mat-card-title>異常レベル</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <mat-chip [class]="'level-' + getAnomalyLevelClass(result.anomalyLevel)">
              {{ getAnomalyLevelText(result.anomalyLevel) }}
            </mat-chip>
          </mat-card-content>
        </mat-card>

        <mat-card class="status-card">
          <mat-card-header>
            <mat-card-title>解決状況</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <mat-chip [class]="'status-' + getResolutionStatusClass(result.resolutionStatus)">
              {{ getResolutionStatusText(result.resolutionStatus) }}
            </mat-chip>
          </mat-card-content>
        </mat-card>

        <mat-card class="status-card">
          <mat-card-header>
            <mat-card-title>信頼度</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="confidence-score">
              {{ (result.confidenceScore * 100) | number:'1.1-1' }}%
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="status-card">
          <mat-card-header>
            <mat-card-title>検出時間</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="execution-time">
              {{ result.executionTimeMs | number:'1.2-2' }}ms
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Main Content Tabs -->
      <mat-tab-group class="detail-tabs">
        <!-- Basic Information Tab -->
        <mat-tab label="基本情報">
          <div class="tab-content">
            <div class="info-grid">
              <mat-card>
                <mat-card-header>
                  <mat-card-title>検出情報</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="info-row">
                    <span class="label">検出時刻:</span>
                    <span class="value">{{ result.detectedAt | date:'yyyy/MM/dd HH:mm:ss.SSS' }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">検出タイプ:</span>
                    <span class="value">{{ getDetectionTypeText(result.detectionType) }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">トリガー条件:</span>
                    <span class="value">{{ result.triggerCondition }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">説明:</span>
                    <span class="value">{{ result.description }}</span>
                  </div>
                </mat-card-content>
              </mat-card>

              <mat-card>
                <mat-card-header>
                  <mat-card-title>CAN信号情報</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="info-row">
                    <span class="label">信号名:</span>
                    <span class="value">{{ result.signalName }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">CAN ID:</span>
                    <span class="value can-id">{{ result.canId }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">システム種別:</span>
                    <span class="value">{{ getSystemTypeText(result.systemType) }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">検出ロジック:</span>
                    <span class="value">{{ result.detectionLogicName }}</span>
                  </div>
                </mat-card-content>
              </mat-card>

              <mat-card>
                <mat-card-header>
                  <mat-card-title>解決情報</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="info-row" *ngIf="result.resolvedAt">
                    <span class="label">解決日時:</span>
                    <span class="value">{{ result.resolvedAt | date:'yyyy/MM/dd HH:mm:ss' }}</span>
                  </div>
                  <div class="info-row" *ngIf="result.resolvedByUserName">
                    <span class="label">解決者:</span>
                    <span class="value">{{ result.resolvedByUserName }}</span>
                  </div>
                  <div class="info-row" *ngIf="result.resolutionNotes">
                    <span class="label">解決メモ:</span>
                    <span class="value">{{ result.resolutionNotes }}</span>
                  </div>
                </mat-card-content>
              </mat-card>

              <mat-card>
                <mat-card-header>
                  <mat-card-title>共有情報</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="info-row">
                    <span class="label">共有レベル:</span>
                    <span class="value">{{ getSharingLevelText(result.sharingLevel) }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">共有状態:</span>
                    <span class="value">{{ result.isShared ? '共有中' : '非共有' }}</span>
                  </div>
                  <div class="info-row" *ngIf="result.sharedAt">
                    <span class="label">共有日時:</span>
                    <span class="value">{{ result.sharedAt | date:'yyyy/MM/dd HH:mm:ss' }}</span>
                  </div>
                  <div class="info-row" *ngIf="result.sharedByUserName">
                    <span class="label">共有者:</span>
                    <span class="value">{{ result.sharedByUserName }}</span>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
          </div>
        </mat-tab>

        <!-- Input Data Tab -->
        <mat-tab label="入力データ">
          <div class="tab-content">
            <mat-card>
              <mat-card-header>
                <mat-card-title>CAN信号値</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="signal-data">
                  <div class="signal-value-display">
                    <div class="value-large">{{ result.signalValue }}</div>
                    <div class="timestamp">{{ result.inputTimestamp | date:'yyyy/MM/dd HH:mm:ss.SSS' }}</div>
                  </div>
                  
                  <!-- Signal Value Visualization -->
                  <div class="chart-container">
                    <canvas #signalChart></canvas>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>

            <mat-card *ngIf="hasAdditionalInputData()">
              <mat-card-header>
                <mat-card-title>追加入力データ</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <mat-expansion-panel *ngFor="let item of getAdditionalInputDataItems()">
                  <mat-expansion-panel-header>
                    <mat-panel-title>{{ item.key }}</mat-panel-title>
                    <mat-panel-description>{{ item.type }}</mat-panel-description>
                  </mat-expansion-panel-header>
                  <div class="data-content">
                    <pre>{{ item.value | json }}</pre>
                  </div>
                </mat-expansion-panel>
              </mat-card-content>
            </mat-card>

            <mat-card>
              <mat-card-header>
                <mat-card-title>検出パラメータ</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <mat-list>
                  <mat-list-item *ngFor="let param of getDetectionParameterItems()">
                    <div matListItemTitle>{{ param.key }}</div>
                    <div matListItemLine>{{ param.value }}</div>
                  </mat-list-item>
                </mat-list>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>

        <!-- Timeline Tab -->
        <mat-tab label="タイムライン">
          <div class="tab-content">
            <mat-card>
              <mat-card-header>
                <mat-card-title>処理タイムライン</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="timeline">
                  <div class="timeline-item">
                    <div class="timeline-marker detection"></div>
                    <div class="timeline-content">
                      <div class="timeline-title">異常検出</div>
                      <div class="timeline-time">{{ result.detectedAt | date:'yyyy/MM/dd HH:mm:ss' }}</div>
                      <div class="timeline-description">{{ result.description }}</div>
                    </div>
                  </div>
                  
                  <div class="timeline-item" *ngIf="result.resolvedAt">
                    <div class="timeline-marker resolution"></div>
                    <div class="timeline-content">
                      <div class="timeline-title">解決</div>
                      <div class="timeline-time">{{ result.resolvedAt | date:'yyyy/MM/dd HH:mm:ss' }}</div>
                      <div class="timeline-description">{{ result.resolutionNotes }}</div>
                    </div>
                  </div>
                  
                  <div class="timeline-item" *ngIf="result.sharedAt">
                    <div class="timeline-marker sharing"></div>
                    <div class="timeline-content">
                      <div class="timeline-title">共有</div>
                      <div class="timeline-time">{{ result.sharedAt | date:'yyyy/MM/dd HH:mm:ss' }}</div>
                      <div class="timeline-description">{{ result.sharedByUserName }}により共有</div>
                    </div>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>

        <!-- Similar Results Tab -->
        <mat-tab label="類似結果">
          <div class="tab-content">
            <mat-card>
              <mat-card-header>
                <mat-card-title>類似する異常検出結果</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="similar-results" *ngIf="similarResults.length > 0; else noSimilarResults">
                  <mat-list>
                    <mat-list-item *ngFor="let similar of similarResults" (click)="viewSimilarResult(similar)">
                      <div matListItemTitle>{{ similar.signalName }} - {{ similar.detectionLogicName }}</div>
                      <div matListItemLine>
                        {{ similar.detectedAt | date:'yyyy/MM/dd HH:mm' }} - 
                        {{ getAnomalyLevelText(similar.anomalyLevel) }}
                      </div>
                      <mat-icon matListItemMeta>arrow_forward</mat-icon>
                    </mat-list-item>
                  </mat-list>
                </div>
                <ng-template #noSimilarResults>
                  <div class="no-similar-results">
                    <mat-icon>search_off</mat-icon>
                    <p>類似する結果が見つかりませんでした</p>
                  </div>
                </ng-template>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>
      </mat-tab-group>

      <!-- Comments Section -->
      <mat-card class="comments-section">
        <mat-card-header>
          <mat-card-title>コメント・メモ</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="commentForm" (ngSubmit)="addComment()">
            <mat-form-field appearance="outline" class="comment-input">
              <mat-label>コメントを追加</mat-label>
              <textarea matInput formControlName="comment" rows="3" 
                       placeholder="この異常検出結果についてのコメントやメモを入力してください"></textarea>
            </mat-form-field>
            <div class="comment-actions">
              <button mat-raised-button color="primary" type="submit" 
                      [disabled]="commentForm.invalid">
                コメント追加
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>

    <!-- Loading Spinner -->
    <div class="loading-container" *ngIf="loading">
      <mat-spinner diameter="50"></mat-spinner>
    </div>
  `,
  styleUrls: ['./detection-result-detail.component.scss']
})
export class DetectionResultDetailComponent implements OnInit, OnDestroy {
  @Input() resultId?: string;

  // Enums for template
  AnomalyLevel = AnomalyLevel;
  ResolutionStatus = ResolutionStatus;
  DetectionType = DetectionType;
  CanSystemType = CanSystemType;
  SharingLevel = SharingLevel;

  result?: AnomalyDetectionResult;
  similarResults: AnomalyDetectionResult[] = [];
  loading = false;
  
  commentForm: FormGroup;
  
  private destroy$ = new Subject<void>();
  private chart?: Chart;

  constructor(
    private detectionResultService: DetectionResultService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.commentForm = this.fb.group({
      comment: ['', [Validators.required, Validators.minLength(1)]]
    });
  }

  ngOnInit(): void {
    if (this.resultId) {
      this.loadResult(this.resultId);
    } else {
      this.route.params
        .pipe(takeUntil(this.destroy$))
        .subscribe(params => {
          if (params['id']) {
            this.loadResult(params['id']);
          }
        });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    
    if (this.chart) {
      this.chart.destroy();
    }
  }

  loadResult(id: string): void {
    this.loading = true;
    
    this.detectionResultService.get(id)
      .pipe(
        switchMap(result => {
          this.result = result;
          return this.detectionResultService.getSimilarResults(id);
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (similarResults) => {
          this.similarResults = similarResults;
          this.loading = false;
          this.initializeChart();
        },
        error: (error) => {
          console.error('Error loading detection result:', error);
          this.snackBar.open('データの読み込みに失敗しました', '閉じる', { duration: 3000 });
          this.loading = false;
        }
      });
  }

  refreshData(): void {
    if (this.result) {
      this.loadResult(this.result.id);
    }
  }

  goBack(): void {
    this.router.navigate(['/detection-results']);
  }

  // Action methods
  markAsInvestigating(): void {
    if (!this.result) return;
    
    this.detectionResultService.markAsInvestigating(this.result.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('調査中に変更しました', '閉じる', { duration: 2000 });
          this.refreshData();
        },
        error: (error) => {
          console.error('Error marking as investigating:', error);
          this.snackBar.open('操作に失敗しました', '閉じる', { duration: 3000 });
        }
      });
  }

  openResolveDialog(): void {
    // TODO: Implement resolve dialog
    const input: ResolveDetectionResultDto = {
      resolutionNotes: '解決済み'
    };
    
    if (!this.result) return;
    
    this.detectionResultService.resolve(this.result.id, input)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('解決済みに変更しました', '閉じる', { duration: 2000 });
          this.refreshData();
        },
        error: (error) => {
          console.error('Error resolving result:', error);
          this.snackBar.open('操作に失敗しました', '閉じる', { duration: 3000 });
        }
      });
  }

  openFalsePositiveDialog(): void {
    // TODO: Implement false positive dialog
    const input: MarkAsFalsePositiveDto = {
      reason: '誤検出として判定',
      notes: ''
    };
    
    if (!this.result) return;
    
    this.detectionResultService.markAsFalsePositive(this.result.id, input)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('誤検出に変更しました', '閉じる', { duration: 2000 });
          this.refreshData();
        },
        error: (error) => {
          console.error('Error marking as false positive:', error);
          this.snackBar.open('操作に失敗しました', '閉じる', { duration: 3000 });
        }
      });
  }

  openReopenDialog(): void {
    // TODO: Implement reopen dialog
    const input: ReopenDetectionResultDto = {
      reason: '再調査が必要',
      notes: ''
    };
    
    if (!this.result) return;
    
    this.detectionResultService.reopen(this.result.id, input)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('再開しました', '閉じる', { duration: 2000 });
          this.refreshData();
        },
        error: (error) => {
          console.error('Error reopening result:', error);
          this.snackBar.open('操作に失敗しました', '閉じる', { duration: 3000 });
        }
      });
  }

  openShareDialog(): void {
    if (!this.result) return;
    
    import('./share-result-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.ShareResultDialogComponent, {
        width: '600px',
        data: { result: this.result }
      });

      dialogRef.afterClosed().subscribe(shareData => {
        if (shareData && this.result) {
          this.detectionResultService.share(this.result.id, shareData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => {
                this.snackBar.open('結果を共有しました', '閉じる', { duration: 2000 });
                this.refreshData();
              },
              error: (error) => {
                console.error('Error sharing result:', error);
                this.snackBar.open('共有に失敗しました', '閉じる', { duration: 3000 });
              }
            });
        }
      });
    });
  }

  addComment(): void {
    if (this.commentForm.valid) {
      const comment = this.commentForm.value.comment;
      // TODO: Implement comment functionality
      console.log('Add comment:', comment);
      this.commentForm.reset();
    }
  }

  viewSimilarResult(result: AnomalyDetectionResult): void {
    this.router.navigate(['/detection-results', result.id]);
  }

  // Data helper methods
  hasAdditionalInputData(): boolean {
    return this.result?.additionalInputData && 
           Object.keys(this.result.additionalInputData).length > 0;
  }

  getAdditionalInputDataItems(): Array<{key: string, value: any, type: string}> {
    if (!this.result?.additionalInputData) return [];
    
    return Object.entries(this.result.additionalInputData).map(([key, value]) => ({
      key,
      value,
      type: typeof value
    }));
  }

  getDetectionParameterItems(): Array<{key: string, value: any}> {
    if (!this.result?.detectionParameters) return [];
    
    return Object.entries(this.result.detectionParameters).map(([key, value]) => ({
      key,
      value: typeof value === 'object' ? JSON.stringify(value) : String(value)
    }));
  }

  private initializeChart(): void {
    if (!this.result) return;
    
    // TODO: Implement signal value visualization chart
    // This would show the signal value over time with the anomaly point highlighted
  }

  // Helper methods for display (same as in list component)
  getAnomalyLevelText(level: AnomalyLevel): string {
    const texts = {
      [AnomalyLevel.Info]: 'Info',
      [AnomalyLevel.Warning]: 'Warning',
      [AnomalyLevel.Error]: 'Error',
      [AnomalyLevel.Critical]: 'Critical',
      [AnomalyLevel.Fatal]: 'Fatal'
    };
    return texts[level] || 'Unknown';
  }

  getAnomalyLevelClass(level: AnomalyLevel): string {
    const classes = {
      [AnomalyLevel.Info]: 'info',
      [AnomalyLevel.Warning]: 'warning',
      [AnomalyLevel.Error]: 'error',
      [AnomalyLevel.Critical]: 'critical',
      [AnomalyLevel.Fatal]: 'fatal'
    };
    return classes[level] || 'default';
  }

  getResolutionStatusText(status: ResolutionStatus): string {
    const texts = {
      [ResolutionStatus.Open]: '未対応',
      [ResolutionStatus.Investigating]: '調査中',
      [ResolutionStatus.InProgress]: '対応中',
      [ResolutionStatus.Resolved]: '解決済み',
      [ResolutionStatus.FalsePositive]: '誤検出',
      [ResolutionStatus.Ignored]: '無視',
      [ResolutionStatus.Reopened]: '再開',
      [ResolutionStatus.Escalated]: 'エスカレート'
    };
    return texts[status] || 'Unknown';
  }

  getResolutionStatusClass(status: ResolutionStatus): string {
    const classes = {
      [ResolutionStatus.Open]: 'open',
      [ResolutionStatus.Investigating]: 'investigating',
      [ResolutionStatus.InProgress]: 'in-progress',
      [ResolutionStatus.Resolved]: 'resolved',
      [ResolutionStatus.FalsePositive]: 'false-positive',
      [ResolutionStatus.Ignored]: 'ignored',
      [ResolutionStatus.Reopened]: 'reopened',
      [ResolutionStatus.Escalated]: 'escalated'
    };
    return classes[status] || 'default';
  }

  getDetectionTypeText(type: DetectionType): string {
    const texts = {
      [DetectionType.OutOfRange]: '範囲外',
      [DetectionType.RateOfChange]: '変化率',
      [DetectionType.Timeout]: '通信断',
      [DetectionType.Stuck]: '固着',
      [DetectionType.Pattern]: 'パターン',
      [DetectionType.Statistical]: '統計的',
      [DetectionType.Custom]: 'カスタム'
    };
    return texts[type] || 'Unknown';
  }

  getSystemTypeText(type: CanSystemType): string {
    const texts = {
      [CanSystemType.Engine]: 'エンジン',
      [CanSystemType.Brake]: 'ブレーキ',
      [CanSystemType.Steering]: 'ステアリング',
      [CanSystemType.Transmission]: 'トランスミッション',
      [CanSystemType.Body]: 'ボディ',
      [CanSystemType.Chassis]: 'シャーシ',
      [CanSystemType.HVAC]: 'HVAC',
      [CanSystemType.Lighting]: '照明',
      [CanSystemType.Infotainment]: 'インフォテインメント',
      [CanSystemType.Safety]: '安全系',
      [CanSystemType.Powertrain]: 'パワートレイン',
      [CanSystemType.Gateway]: 'ゲートウェイ',
      [CanSystemType.Battery]: 'バッテリー',
      [CanSystemType.Motor]: 'モーター',
      [CanSystemType.Inverter]: 'インバーター',
      [CanSystemType.Charger]: '充電器',
      [CanSystemType.ADAS]: 'ADAS',
      [CanSystemType.Suspension]: 'サスペンション',
      [CanSystemType.Exhaust]: '排気',
      [CanSystemType.Fuel]: '燃料'
    };
    return texts[type] || 'Unknown';
  }

  getSharingLevelText(level: SharingLevel): string {
    const texts = {
      [SharingLevel.Private]: 'プライベート',
      [SharingLevel.OemPartner]: 'OEMパートナー',
      [SharingLevel.Industry]: '業界共通',
      [SharingLevel.Public]: 'パブリック'
    };
    return texts[level] || 'Unknown';
  }
}