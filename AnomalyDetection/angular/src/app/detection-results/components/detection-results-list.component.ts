import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSortModule, MatSort, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import { SelectionModel } from '@angular/cdk/collections';
import {
  Subject,
  debounceTime,
  distinctUntilChanged,
  takeUntil,
  startWith,
  switchMap,
  filter,
} from 'rxjs';

import { DetectionResultService } from '../services/detection-result.service';
import {
  RealtimeDetectionHubService,
  HubConnectionState,
} from '../services/realtime-detection-hub.service';
import {
  AnomalyDetectionResult,
  GetDetectionResultsInput,
  AnomalyLevel,
  ResolutionStatus,
  SharingLevel,
  DetectionType,
  CanSystemType,
} from '../models/detection-result.model';

@Component({
  selector: 'app-detection-results-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCheckboxModule,
    MatMenuModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatExpansionModule,
  ],
  template: `
    <div class="detection-results-container">
      <!-- Header -->
      <div class="header">
        <h2>異常検出結果一覧</h2>
        <div class="header-actions">
          <button mat-raised-button color="primary" (click)="refreshData()">
            <mat-icon>refresh</mat-icon>
            更新
          </button>
          <button mat-raised-button (click)="exportResults()">
            <mat-icon>download</mat-icon>
            エクスポート
          </button>
        </div>
      </div>

      <!-- Filters -->
      <mat-expansion-panel class="filter-panel" [expanded]="showFilters">
        <mat-expansion-panel-header>
          <mat-panel-title>
            <mat-icon>filter_list</mat-icon>
            フィルター
          </mat-panel-title>
        </mat-expansion-panel-header>

        <form [formGroup]="filterForm" class="filter-form">
          <div class="filter-row">
            <mat-form-field appearance="outline">
              <mat-label>検索</mat-label>
              <input matInput formControlName="filter" placeholder="信号名、説明で検索" />
              <mat-icon matSuffix>search</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>異常レベル</mat-label>
              <mat-select formControlName="anomalyLevel">
                <mat-option value="">すべて</mat-option>
                <mat-option [value]="AnomalyLevel.Info">Info</mat-option>
                <mat-option [value]="AnomalyLevel.Warning">Warning</mat-option>
                <mat-option [value]="AnomalyLevel.Error">Error</mat-option>
                <mat-option [value]="AnomalyLevel.Critical">Critical</mat-option>
                <mat-option [value]="AnomalyLevel.Fatal">Fatal</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>解決状況</mat-label>
              <mat-select formControlName="resolutionStatus">
                <mat-option value="">すべて</mat-option>
                <mat-option [value]="ResolutionStatus.Open">未対応</mat-option>
                <mat-option [value]="ResolutionStatus.Investigating">調査中</mat-option>
                <mat-option [value]="ResolutionStatus.InProgress">対応中</mat-option>
                <mat-option [value]="ResolutionStatus.Resolved">解決済み</mat-option>
                <mat-option [value]="ResolutionStatus.FalsePositive">誤検出</mat-option>
                <mat-option [value]="ResolutionStatus.Ignored">無視</mat-option>
              </mat-select>
            </mat-form-field>
          </div>

          <div class="filter-row">
            <mat-form-field appearance="outline">
              <mat-label>検出タイプ</mat-label>
              <mat-select formControlName="detectionType">
                <mat-option value="">すべて</mat-option>
                <mat-option [value]="DetectionType.OutOfRange">範囲外</mat-option>
                <mat-option [value]="DetectionType.RateOfChange">変化率</mat-option>
                <mat-option [value]="DetectionType.Timeout">通信断</mat-option>
                <mat-option [value]="DetectionType.Stuck">固着</mat-option>
                <mat-option [value]="DetectionType.Pattern">パターン</mat-option>
                <mat-option [value]="DetectionType.Statistical">統計的</mat-option>
                <mat-option [value]="DetectionType.Custom">カスタム</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>システム種別</mat-label>
              <mat-select formControlName="systemType">
                <mat-option value="">すべて</mat-option>
                <mat-option [value]="CanSystemType.Engine">エンジン</mat-option>
                <mat-option [value]="CanSystemType.Brake">ブレーキ</mat-option>
                <mat-option [value]="CanSystemType.Steering">ステアリング</mat-option>
                <mat-option [value]="CanSystemType.Transmission">トランスミッション</mat-option>
                <mat-option [value]="CanSystemType.Body">ボディ</mat-option>
                <mat-option [value]="CanSystemType.Safety">安全系</mat-option>
                <mat-option [value]="CanSystemType.ADAS">ADAS</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>検出日時（開始）</mat-label>
              <input matInput [matDatepicker]="startPicker" formControlName="detectedFrom" />
              <mat-datepicker-toggle matSuffix [for]="startPicker"></mat-datepicker-toggle>
              <mat-datepicker #startPicker></mat-datepicker>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>検出日時（終了）</mat-label>
              <input matInput [matDatepicker]="endPicker" formControlName="detectedTo" />
              <mat-datepicker-toggle matSuffix [for]="endPicker"></mat-datepicker-toggle>
              <mat-datepicker #endPicker></mat-datepicker>
            </mat-form-field>
          </div>

          <div class="filter-actions">
            <button mat-button type="button" (click)="clearFilters()">クリア</button>
            <button mat-raised-button color="primary" type="button" (click)="applyFilters()">
              適用
            </button>
          </div>
        </form>
      </mat-expansion-panel>

      <!-- Bulk Actions -->
      <div class="bulk-actions" *ngIf="selection.hasValue()">
        <span>{{ selection.selected.length }} 件選択中</span>
        <button mat-button (click)="bulkMarkAsInvestigating()">
          <mat-icon>search</mat-icon>
          調査中にする
        </button>
        <button mat-button (click)="bulkMarkAsFalsePositive()">
          <mat-icon>block</mat-icon>
          誤検出にする
        </button>
        <button mat-button (click)="bulkResolve()">
          <mat-icon>check_circle</mat-icon>
          解決済みにする
        </button>
      </div>

      <!-- Results Table -->
      <div class="table-container">
        <mat-table [dataSource]="dataSource" matSort class="results-table">
          <!-- Checkbox Column -->
          <ng-container matColumnDef="select">
            <mat-header-cell *matHeaderCellDef>
              <mat-checkbox
                (change)="$event ? masterToggle() : null"
                [checked]="selection.hasValue() && isAllSelected()"
                [indeterminate]="selection.hasValue() && !isAllSelected()"
              >
              </mat-checkbox>
            </mat-header-cell>
            <mat-cell *matCellDef="let row">
              <mat-checkbox
                (click)="$event.stopPropagation()"
                (change)="$event ? selection.toggle(row) : null"
                [checked]="selection.isSelected(row)"
              >
              </mat-checkbox>
            </mat-cell>
          </ng-container>

          <!-- Detection Time Column -->
          <ng-container matColumnDef="detectedAt">
            <mat-header-cell *matHeaderCellDef mat-sort-header>検出時刻</mat-header-cell>
            <mat-cell *matCellDef="let result">
              {{ result.detectedAt | date : 'yyyy/MM/dd HH:mm:ss' }}
            </mat-cell>
          </ng-container>

          <!-- Anomaly Level Column -->
          <ng-container matColumnDef="anomalyLevel">
            <mat-header-cell *matHeaderCellDef mat-sort-header>異常レベル</mat-header-cell>
            <mat-cell *matCellDef="let result">
              <mat-chip [class]="'level-' + getAnomalyLevelClass(result.anomalyLevel)">
                {{ getAnomalyLevelText(result.anomalyLevel) }}
              </mat-chip>
            </mat-cell>
          </ng-container>

          <!-- Signal Name Column -->
          <ng-container matColumnDef="signalName">
            <mat-header-cell *matHeaderCellDef mat-sort-header>信号名</mat-header-cell>
            <mat-cell *matCellDef="let result">
              <div class="signal-info">
                <div class="signal-name">{{ result.signalName }}</div>
                <div class="can-id">{{ result.canId }}</div>
              </div>
            </mat-cell>
          </ng-container>

          <!-- Detection Logic Column -->
          <ng-container matColumnDef="detectionLogicName">
            <mat-header-cell *matHeaderCellDef mat-sort-header>検出ロジック</mat-header-cell>
            <mat-cell *matCellDef="let result">{{ result.detectionLogicName }}</mat-cell>
          </ng-container>

          <!-- Confidence Score Column -->
          <ng-container matColumnDef="confidenceScore">
            <mat-header-cell *matHeaderCellDef mat-sort-header>信頼度</mat-header-cell>
            <mat-cell *matCellDef="let result">
              <div class="confidence-score">
                {{ result.confidenceScore * 100 | number : '1.1-1' }}%
              </div>
            </mat-cell>
          </ng-container>

          <!-- Resolution Status Column -->
          <ng-container matColumnDef="resolutionStatus">
            <mat-header-cell *matHeaderCellDef mat-sort-header>解決状況</mat-header-cell>
            <mat-cell *matCellDef="let result">
              <mat-chip [class]="'status-' + getResolutionStatusClass(result.resolutionStatus)">
                {{ getResolutionStatusText(result.resolutionStatus) }}
              </mat-chip>
            </mat-cell>
          </ng-container>

          <!-- System Type Column -->
          <ng-container matColumnDef="systemType">
            <mat-header-cell *matHeaderCellDef mat-sort-header>システム</mat-header-cell>
            <mat-cell *matCellDef="let result">{{ getSystemTypeText(result.systemType) }}</mat-cell>
          </ng-container>

          <!-- Actions Column -->
          <ng-container matColumnDef="actions">
            <mat-header-cell *matHeaderCellDef>操作</mat-header-cell>
            <mat-cell *matCellDef="let result">
              <button mat-icon-button [matMenuTriggerFor]="actionMenu">
                <mat-icon>more_vert</mat-icon>
              </button>
              <mat-menu #actionMenu="matMenu">
                <button mat-menu-item (click)="viewDetails(result)">
                  <mat-icon>visibility</mat-icon>
                  詳細表示
                </button>
                <button
                  mat-menu-item
                  (click)="markAsInvestigating(result)"
                  *ngIf="result.resolutionStatus === ResolutionStatus.Open"
                >
                  <mat-icon>search</mat-icon>
                  調査中にする
                </button>
                <button
                  mat-menu-item
                  (click)="markAsFalsePositive(result)"
                  *ngIf="result.resolutionStatus !== ResolutionStatus.FalsePositive"
                >
                  <mat-icon>block</mat-icon>
                  誤検出にする
                </button>
                <button
                  mat-menu-item
                  (click)="resolveResult(result)"
                  *ngIf="result.resolutionStatus !== ResolutionStatus.Resolved"
                >
                  <mat-icon>check_circle</mat-icon>
                  解決する
                </button>
                <button mat-menu-item (click)="shareResult(result)" *ngIf="!result.isShared">
                  <mat-icon>share</mat-icon>
                  共有する
                </button>
                <button mat-menu-item (click)="viewSimilar(result)">
                  <mat-icon>compare</mat-icon>
                  類似結果を表示
                </button>
              </mat-menu>
            </mat-cell>
          </ng-container>

          <mat-header-row *matHeaderRowDef="displayedColumns"></mat-header-row>
          <mat-row
            *matRowDef="let row; columns: displayedColumns"
            (click)="viewDetails(row)"
            class="clickable-row"
          ></mat-row>
        </mat-table>

        <!-- Loading Spinner -->
        <div class="loading-container" *ngIf="loading">
          <mat-spinner diameter="50"></mat-spinner>
        </div>

        <!-- No Data -->
        <div class="no-data" *ngIf="!loading && dataSource.data.length === 0">
          <mat-icon>search_off</mat-icon>
          <p>検出結果が見つかりませんでした</p>
        </div>
      </div>

      <!-- Paginator -->
      <mat-paginator
        [length]="totalCount"
        [pageSize]="pageSize"
        [pageSizeOptions]="[10, 25, 50, 100]"
        (page)="onPageChange($event)"
        showFirstLastButtons
      >
      </mat-paginator>

      <!-- Real-time Updates Indicator -->
      <div class="realtime-indicator" *ngIf="realtimeEnabled">
        <mat-icon class="pulse">wifi</mat-icon>
        リアルタイム更新中
      </div>
    </div>
  `,
  styleUrls: ['./detection-results-list.component.scss'],
})
export class DetectionResultsListComponent implements OnInit, OnDestroy {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  // Enums for template
  AnomalyLevel = AnomalyLevel;
  ResolutionStatus = ResolutionStatus;
  DetectionType = DetectionType;
  CanSystemType = CanSystemType;

  // Data
  dataSource = new MatTableDataSource<AnomalyDetectionResult>([]);
  selection = new SelectionModel<AnomalyDetectionResult>(true, []);
  totalCount = 0;
  pageSize = 25;
  currentPage = 0;
  loading = false;
  realtimeEnabled = false;

  // Filters
  filterForm: FormGroup;
  showFilters = true;

  // Columns
  displayedColumns: string[] = [
    'select',
    'detectedAt',
    'anomalyLevel',
    'signalName',
    'detectionLogicName',
    'confidenceScore',
    'resolutionStatus',
    'systemType',
    'actions',
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private detectionResultService: DetectionResultService,
    private realtimeHubService: RealtimeDetectionHubService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.filterForm = this.createFilterForm();
  }

  ngOnInit(): void {
    this.setupFilterSubscription();
    this.loadData();
    this.setupRealtimeUpdates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    // Disconnect SignalR when component is destroyed
    this.realtimeHubService.stopConnection();
  }

  private createFilterForm(): FormGroup {
    return this.fb.group({
      filter: [''],
      anomalyLevel: [''],
      resolutionStatus: [''],
      detectionType: [''],
      systemType: [''],
      detectedFrom: [null],
      detectedTo: [null],
      minConfidenceScore: [null],
      maxConfidenceScore: [null],
      isHighPriority: [false],
    });
  }

  private setupFilterSubscription(): void {
    this.filterForm.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 0;
        this.loadData();
      });
  }

  private setupRealtimeUpdates(): void {
    // Initialize SignalR connection
    this.realtimeHubService
      .startConnection()
      .then(() => {
        console.log('SignalR connection established');
        this.realtimeEnabled = true;

        // Subscribe to all detection results (can be filtered by project if needed)
        this.realtimeHubService
          .subscribeToAllResults()
          .catch(err => console.error('Error subscribing to all results:', err));

        // Handle new detection results
        this.realtimeHubService.onNewDetectionResult
          .pipe(
            filter(result => result !== null),
            takeUntil(this.destroy$)
          )
          .subscribe(result => {
            if (result) {
              // Prepend new result to the current data
              const currentData = this.dataSource.data;
              this.dataSource.data = [result, ...currentData];
              this.totalCount++;

              // Show notification
              this.snackBar
                .open(
                  `新しい異常検出: ${result.signalName} (信頼度: ${(
                    result.confidenceScore * 100
                  ).toFixed(1)}%)`,
                  '表示',
                  { duration: 5000 }
                )
                .onAction()
                .subscribe(() => {
                  this.viewDetails(result);
                });
            }
          });

        // Handle detection result updates
        this.realtimeHubService.onDetectionResultUpdated
          .pipe(
            filter(result => result !== null),
            takeUntil(this.destroy$)
          )
          .subscribe(result => {
            if (result) {
              // Update the existing result in the data source
              const currentData = this.dataSource.data;
              const index = currentData.findIndex(r => r.id === result.id);
              if (index !== -1) {
                currentData[index] = result;
                this.dataSource.data = [...currentData];

                this.snackBar.open(`検出結果が更新されました: ${result.signalName}`, '閉じる', {
                  duration: 3000,
                });
              }
            }
          });

        // Handle detection result deletions
        this.realtimeHubService.onDetectionResultDeleted
          .pipe(
            filter(resultId => resultId !== null),
            takeUntil(this.destroy$)
          )
          .subscribe(resultId => {
            if (resultId) {
              // Remove the deleted result from the data source
              const currentData = this.dataSource.data;
              this.dataSource.data = currentData.filter(r => r.id !== resultId);
              this.totalCount--;

              this.snackBar.open('検出結果が削除されました', '閉じる', { duration: 3000 });
            }
          });

        // Monitor connection state changes
        this.realtimeHubService.connectionState.pipe(takeUntil(this.destroy$)).subscribe(state => {
          if (state === HubConnectionState.Reconnecting) {
            this.snackBar.open('リアルタイム接続を再確立中...', '閉じる', { duration: 2000 });
          } else if (state === HubConnectionState.Disconnected) {
            this.realtimeEnabled = false;
            this.snackBar.open('リアルタイム接続が切断されました', '閉じる', { duration: 3000 });
          } else if (state === HubConnectionState.Connected) {
            this.realtimeEnabled = true;
          }
        });
      })
      .catch(error => {
        console.error('Failed to start SignalR connection:', error);
        this.realtimeEnabled = false;
        this.snackBar.open(
          'リアルタイム更新の接続に失敗しました。手動で更新してください。',
          '閉じる',
          { duration: 5000 }
        );
      });
  }

  loadData(): void {
    this.loading = true;

    const input: GetDetectionResultsInput = {
      ...this.filterForm.value,
      skipCount: this.currentPage * this.pageSize,
      maxResultCount: this.pageSize,
      sorting: this.sort?.active ? `${this.sort.active} ${this.sort.direction}` : 'detectedAt desc',
    };

    this.detectionResultService
      .getList(input)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          // Guard against null enum values to avoid toString errors in templates
          this.dataSource.data = (result.items || []).map(r => ({
            ...r,
            anomalyLevel: r.anomalyLevel ?? null,
            resolutionStatus: r.resolutionStatus ?? null,
          }));
          this.totalCount = result.totalCount ?? 0;
          this.loading = false;
          this.selection.clear();
        },
        error: error => {
          console.error('Error loading detection results:', error);
          this.snackBar.open('データの読み込みに失敗しました', '閉じる', { duration: 3000 });
          this.loading = false;
        },
      });
  }

  refreshData(): void {
    this.loadData();
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadData();
  }

  applyFilters(): void {
    this.currentPage = 0;
    this.loadData();
  }

  clearFilters(): void {
    this.filterForm.reset();
    this.currentPage = 0;
    this.loadData();
  }

  // Selection methods
  isAllSelected(): boolean {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  masterToggle(): void {
    this.isAllSelected()
      ? this.selection.clear()
      : this.dataSource.data.forEach(row => this.selection.select(row));
  }

  // Action methods
  viewDetails(result: AnomalyDetectionResult): void {
    // TODO: Navigate to detail page or open dialog
    console.log('View details for:', result);
  }

  markAsInvestigating(result: AnomalyDetectionResult): void {
    this.detectionResultService
      .markAsInvestigating(result.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('調査中に変更しました', '閉じる', { duration: 2000 });
          this.loadData();
        },
        error: error => {
          console.error('Error marking as investigating:', error);
          this.snackBar.open('操作に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  markAsFalsePositive(result: AnomalyDetectionResult): void {
    // TODO: Open dialog to get reason
    const input = { reason: '誤検出として判定', notes: '' };

    this.detectionResultService
      .markAsFalsePositive(result.id, input)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('誤検出に変更しました', '閉じる', { duration: 2000 });
          this.loadData();
        },
        error: error => {
          console.error('Error marking as false positive:', error);
          this.snackBar.open('操作に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  resolveResult(result: AnomalyDetectionResult): void {
    // TODO: Open dialog to get resolution notes
    const input = { resolutionNotes: '解決済み' };

    this.detectionResultService
      .resolve(result.id, input)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('解決済みに変更しました', '閉じる', { duration: 2000 });
          this.loadData();
        },
        error: error => {
          console.error('Error resolving result:', error);
          this.snackBar.open('操作に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  shareResult(result: AnomalyDetectionResult): void {
    import('./share-result-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.ShareResultDialogComponent, {
        width: '600px',
        data: { result },
      });

      dialogRef.afterClosed().subscribe(shareData => {
        if (shareData) {
          this.detectionResultService
            .share(result.id, shareData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => {
                this.snackBar.open('結果を共有しました', '閉じる', { duration: 2000 });
                this.loadData();
              },
              error: error => {
                console.error('Error sharing result:', error);
                this.snackBar.open('共有に失敗しました', '閉じる', { duration: 3000 });
              },
            });
        }
      });
    });
  }

  viewSimilar(result: AnomalyDetectionResult): void {
    // TODO: Navigate to similar results view
    console.log('View similar results for:', result);
  }

  // Bulk actions
  bulkMarkAsInvestigating(): void {
    const ids = this.selection.selected.map(r => r.id);
    this.detectionResultService
      .bulkUpdateResolutionStatus(ids, ResolutionStatus.Investigating)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open(`${ids.length}件を調査中に変更しました`, '閉じる', { duration: 2000 });
          this.loadData();
        },
        error: error => {
          console.error('Error bulk updating:', error);
          this.snackBar.open('一括操作に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  bulkMarkAsFalsePositive(): void {
    const ids = this.selection.selected.map(r => r.id);
    this.detectionResultService
      .bulkMarkAsFalsePositive(ids, '一括誤検出判定')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open(`${ids.length}件を誤検出に変更しました`, '閉じる', { duration: 2000 });
          this.loadData();
        },
        error: error => {
          console.error('Error bulk marking as false positive:', error);
          this.snackBar.open('一括操作に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  bulkResolve(): void {
    const ids = this.selection.selected.map(r => r.id);
    this.detectionResultService
      .bulkUpdateResolutionStatus(ids, ResolutionStatus.Resolved, '一括解決')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open(`${ids.length}件を解決済みに変更しました`, '閉じる', {
            duration: 2000,
          });
          this.loadData();
        },
        error: error => {
          console.error('Error bulk resolving:', error);
          this.snackBar.open('一括操作に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  exportResults(): void {
    const input: GetDetectionResultsInput = {
      ...this.filterForm.value,
      skipCount: 0,
      maxResultCount: this.totalCount,
    };

    this.detectionResultService
      .export(input, 'csv')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: blob => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `detection-results-${new Date().toISOString().split('T')[0]}.csv`;
          a.click();
          window.URL.revokeObjectURL(url);
        },
        error: error => {
          console.error('Error exporting results:', error);
          this.snackBar.open('エクスポートに失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  // Helper methods for display
  getAnomalyLevelText(level: AnomalyLevel): string {
    const texts = {
      [AnomalyLevel.Info]: 'Info',
      [AnomalyLevel.Warning]: 'Warning',
      [AnomalyLevel.Error]: 'Error',
      [AnomalyLevel.Critical]: 'Critical',
      [AnomalyLevel.Fatal]: 'Fatal',
    };
    return texts[level] || 'Unknown';
  }

  getAnomalyLevelClass(level: AnomalyLevel): string {
    const classes = {
      [AnomalyLevel.Info]: 'info',
      [AnomalyLevel.Warning]: 'warning',
      [AnomalyLevel.Error]: 'error',
      [AnomalyLevel.Critical]: 'critical',
      [AnomalyLevel.Fatal]: 'fatal',
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
      [ResolutionStatus.Escalated]: 'エスカレート',
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
      [ResolutionStatus.Escalated]: 'escalated',
    };
    return classes[status] || 'default';
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
      [CanSystemType.Fuel]: '燃料',
    };
    return texts[type] || 'Unknown';
  }
}
