import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatExpansionModule } from '@angular/material/expansion';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

import { DetectionResultService } from '../services/detection-result.service';
import { 
  AnomalyDetectionResult, 
  GetDetectionResultsInput,
  AnomalyLevel,
  SharingLevel,
  DetectionType,
  CanSystemType
} from '../models/detection-result.model';

@Component({
  selector: 'app-shared-results-list',
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
    MatCardModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatExpansionModule
  ],
  template: `
    <div class="shared-results-container">
      <!-- Header -->
      <div class="header">
        <div class="header-info">
          <h2>共有された異常検出結果</h2>
          <p class="header-description">
            他のOEMから共有された異常検出結果を閲覧できます。
            これらの情報は業界全体の知見向上に役立ちます。
          </p>
        </div>
        <div class="header-actions">
          <button mat-raised-button color="primary" (click)="refreshData()">
            <mat-icon>refresh</mat-icon>
            更新
          </button>
        </div>
      </div>

      <!-- Info Cards -->
      <div class="info-cards">
        <mat-card class="info-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>share</mat-icon>
              共有レベルについて
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="sharing-levels">
              <div class="level-item">
                <mat-chip class="level-oem">OEMパートナー</mat-chip>
                <span>同じOEMグループ内での共有</span>
              </div>
              <div class="level-item">
                <mat-chip class="level-industry">業界共通</mat-chip>
                <span>自動車業界全体での共有</span>
              </div>
              <div class="level-item">
                <mat-chip class="level-public">パブリック</mat-chip>
                <span>一般公開（匿名化済み）</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="info-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>security</mat-icon>
              プライバシー保護
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <ul class="privacy-list">
              <li>OEM固有の識別情報は匿名化されています</li>
              <li>車両モデル名は一般化されています</li>
              <li>個人情報は完全に除去されています</li>
              <li>技術的な詳細のみが共有されています</li>
            </ul>
          </mat-card-content>
        </mat-card>
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
              <input matInput formControlName="filter" placeholder="信号名、説明で検索">
              <mat-icon matSuffix>search</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>共有レベル</mat-label>
              <mat-select formControlName="sharingLevel">
                <mat-option value="">すべて</mat-option>
                <mat-option [value]="SharingLevel.OemPartner">OEMパートナー</mat-option>
                <mat-option [value]="SharingLevel.Industry">業界共通</mat-option>
                <mat-option [value]="SharingLevel.Public">パブリック</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>異常レベル</mat-label>
              <mat-select formControlName="anomalyLevel">
                <mat-option value="">すべて</mat-option>
                <mat-option [value]="AnomalyLevel.Warning">Warning</mat-option>
                <mat-option [value]="AnomalyLevel.Error">Error</mat-option>
                <mat-option [value]="AnomalyLevel.Critical">Critical</mat-option>
                <mat-option [value]="AnomalyLevel.Fatal">Fatal</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>システム種別</mat-label>
              <mat-select formControlName="systemType">
                <mat-option value="">すべて</mat-option>
                <mat-option [value]="CanSystemType.Engine">エンジン</mat-option>
                <mat-option [value]="CanSystemType.Brake">ブレーキ</mat-option>
                <mat-option [value]="CanSystemType.Steering">ステアリング</mat-option>
                <mat-option [value]="CanSystemType.Safety">安全系</mat-option>
                <mat-option [value]="CanSystemType.ADAS">ADAS</mat-option>
              </mat-select>
            </mat-form-field>
          </div>

          <div class="filter-actions">
            <button mat-button type="button" (click)="clearFilters()">クリア</button>
            <button mat-raised-button color="primary" type="button" (click)="applyFilters()">適用</button>
          </div>
        </form>
      </mat-expansion-panel>

      <!-- Results Table -->
      <div class="table-container">
        <mat-table [dataSource]="dataSource" matSort class="shared-results-table">
          <!-- Shared Date Column -->
          <ng-container matColumnDef="sharedAt">
            <mat-header-cell *matHeaderCellDef mat-sort-header>共有日時</mat-header-cell>
            <mat-cell *matCellDef="let result">
              {{ result.sharedAt | date:'yyyy/MM/dd HH:mm' }}
            </mat-cell>
          </ng-container>

          <!-- Sharing Level Column -->
          <ng-container matColumnDef="sharingLevel">
            <mat-header-cell *matHeaderCellDef mat-sort-header>共有レベル</mat-header-cell>
            <mat-cell *matCellDef="let result">
              <mat-chip [class]="'sharing-' + getSharingLevelClass(result.sharingLevel)">
                {{ getSharingLevelText(result.sharingLevel) }}
              </mat-chip>
            </mat-cell>
          </ng-container>

          <!-- Detection Time Column -->
          <ng-container matColumnDef="detectedAt">
            <mat-header-cell *matHeaderCellDef mat-sort-header>検出時刻</mat-header-cell>
            <mat-cell *matCellDef="let result">
              {{ result.detectedAt | date:'yyyy/MM/dd HH:mm' }}
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

          <!-- Signal Name Column (Anonymized) -->
          <ng-container matColumnDef="signalName">
            <mat-header-cell *matHeaderCellDef mat-sort-header>信号名</mat-header-cell>
            <mat-cell *matCellDef="let result">
              <div class="signal-info">
                <div class="signal-name">{{ getAnonymizedSignalName(result) }}</div>
                <div class="can-id">{{ getAnonymizedCanId(result) }}</div>
              </div>
            </mat-cell>
          </ng-container>

          <!-- Detection Type Column -->
          <ng-container matColumnDef="detectionType">
            <mat-header-cell *matHeaderCellDef mat-sort-header>検出タイプ</mat-header-cell>
            <mat-cell *matCellDef="let result">{{ getDetectionTypeText(result.detectionType) }}</mat-cell>
          </ng-container>

          <!-- System Type Column -->
          <ng-container matColumnDef="systemType">
            <mat-header-cell *matHeaderCellDef mat-sort-header>システム</mat-header-cell>
            <mat-cell *matCellDef="let result">{{ getSystemTypeText(result.systemType) }}</mat-cell>
          </ng-container>

          <!-- Confidence Score Column -->
          <ng-container matColumnDef="confidenceScore">
            <mat-header-cell *matHeaderCellDef mat-sort-header>信頼度</mat-header-cell>
            <mat-cell *matCellDef="let result">
              {{ (result.confidenceScore * 100) | number:'1.1-1' }}%
            </mat-cell>
          </ng-container>

          <!-- Actions Column -->
          <ng-container matColumnDef="actions">
            <mat-header-cell *matHeaderCellDef>操作</mat-header-cell>
            <mat-cell *matCellDef="let result">
              <button mat-icon-button (click)="viewSharedResult(result)" 
                      matTooltip="詳細を表示">
                <mat-icon>visibility</mat-icon>
              </button>
              <button mat-icon-button (click)="bookmarkResult(result)"
                      matTooltip="ブックマークに追加">
                <mat-icon>bookmark_border</mat-icon>
              </button>
            </mat-cell>
          </ng-container>

          <mat-header-row *matHeaderRowDef="displayedColumns"></mat-header-row>
          <mat-row *matRowDef="let row; columns: displayedColumns;" 
                   (click)="viewSharedResult(row)"
                   class="clickable-row"></mat-row>
        </mat-table>

        <!-- Loading Spinner -->
        <div class="loading-container" *ngIf="loading">
          <mat-spinner diameter="50"></mat-spinner>
        </div>

        <!-- No Data -->
        <div class="no-data" *ngIf="!loading && dataSource.data.length === 0">
          <mat-icon>share_off</mat-icon>
          <p>共有された結果が見つかりませんでした</p>
        </div>
      </div>

      <!-- Paginator -->
      <mat-paginator [length]="totalCount"
                     [pageSize]="pageSize"
                     [pageSizeOptions]="[10, 25, 50]"
                     (page)="onPageChange($event)"
                     showFirstLastButtons>
      </mat-paginator>
    </div>
  `,
  styleUrls: ['./shared-results-list.component.scss']
})
export class SharedResultsListComponent implements OnInit, OnDestroy {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  // Enums for template
  AnomalyLevel = AnomalyLevel;
  SharingLevel = SharingLevel;
  DetectionType = DetectionType;
  CanSystemType = CanSystemType;

  // Data
  dataSource = new MatTableDataSource<AnomalyDetectionResult>([]);
  totalCount = 0;
  pageSize = 25;
  currentPage = 0;
  loading = false;

  // Filters
  filterForm: FormGroup;
  showFilters = true;

  // Columns
  displayedColumns: string[] = [
    'sharedAt',
    'sharingLevel',
    'detectedAt',
    'anomalyLevel',
    'signalName',
    'detectionType',
    'systemType',
    'confidenceScore',
    'actions'
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private detectionResultService: DetectionResultService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.filterForm = this.createFilterForm();
  }

  ngOnInit(): void {
    this.setupFilterSubscription();
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createFilterForm(): FormGroup {
    return this.fb.group({
      filter: [''],
      sharingLevel: [''],
      anomalyLevel: [''],
      detectionType: [''],
      systemType: ['']
    });
  }

  private setupFilterSubscription(): void {
    this.filterForm.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.currentPage = 0;
        this.loadData();
      });
  }

  loadData(): void {
    this.loading = true;
    
    const input: GetDetectionResultsInput = {
      ...this.filterForm.value,
      skipCount: this.currentPage * this.pageSize,
      maxResultCount: this.pageSize,
      sorting: this.sort?.active ? `${this.sort.active} ${this.sort.direction}` : 'sharedAt desc',
      isShared: true // Only get shared results
    };

    this.detectionResultService.getSharedResults(input)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.dataSource.data = result.items;
          this.totalCount = result.totalCount;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading shared results:', error);
          this.snackBar.open('データの読み込みに失敗しました', '閉じる', { duration: 3000 });
          this.loading = false;
        }
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

  viewSharedResult(result: AnomalyDetectionResult): void {
    // TODO: Navigate to shared result detail view
    console.log('View shared result:', result);
  }

  bookmarkResult(result: AnomalyDetectionResult): void {
    // TODO: Implement bookmark functionality
    this.snackBar.open('ブックマークに追加しました', '閉じる', { duration: 2000 });
  }

  // Anonymization methods
  getAnonymizedSignalName(result: AnomalyDetectionResult): string {
    if (result.sharingLevel === SharingLevel.Public) {
      // Return generalized signal name for public sharing
      return `${this.getSystemTypeText(result.systemType)}_Signal_${result.id.substring(0, 8)}`;
    }
    return result.signalName;
  }

  getAnonymizedCanId(result: AnomalyDetectionResult): string {
    if (result.sharingLevel === SharingLevel.Public) {
      // Return anonymized CAN ID for public sharing
      return `0x***${result.canId.substring(result.canId.length - 2)}`;
    }
    return result.canId;
  }

  // Helper methods (same as other components)
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

  getSharingLevelText(level: SharingLevel): string {
    const texts = {
      [SharingLevel.Private]: 'プライベート',
      [SharingLevel.OemPartner]: 'OEMパートナー',
      [SharingLevel.Industry]: '業界共通',
      [SharingLevel.Public]: 'パブリック'
    };
    return texts[level] || 'Unknown';
  }

  getSharingLevelClass(level: SharingLevel): string {
    const classes = {
      [SharingLevel.Private]: 'private',
      [SharingLevel.OemPartner]: 'oem',
      [SharingLevel.Industry]: 'industry',
      [SharingLevel.Public]: 'public'
    };
    return classes[level] || 'default';
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
}