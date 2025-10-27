import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { Subject, takeUntil } from 'rxjs';

import { DashboardService } from '../services/dashboard.service';
import { 
  SystemAnomalyReport,
  GetDetectionStatisticsInput,
  GenerateSystemReportInput
} from '../models/dashboard.model';

@Component({
  selector: 'app-detailed-statistics',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTabsModule,
    MatTableModule,
    MatCheckboxModule,
    MatExpansionModule,
    MatDividerModule
  ],
  template: `
    <div class="detailed-statistics-container">
      <!-- Header -->
      <div class="header">
        <div class="header-info">
          <h2>詳細統計・レポート</h2>
          <p class="header-description">
            システム別の詳細な統計情報とカスタムレポートの生成
          </p>
        </div>
        <div class="header-actions">
          <button mat-raised-button color="primary" (click)="refreshData()">
            <mat-icon>refresh</mat-icon>
            更新
          </button>
        </div>
      </div>

      <!-- Filters -->
      <mat-card class="filter-card">
        <mat-card-header>
          <mat-card-title>分析条件</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="filterForm" class="filter-form">
            <div class="filter-row">
              <mat-form-field appearance="outline">
                <mat-label>期間（開始）</mat-label>
                <input matInput [matDatepicker]="startPicker" formControlName="startDate">
                <mat-datepicker-toggle matSuffix [for]="startPicker"></mat-datepicker-toggle>
                <mat-datepicker #startPicker></mat-datepicker>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>期間（終了）</mat-label>
                <input matInput [matDatepicker]="endPicker" formControlName="endDate">
                <mat-datepicker-toggle matSuffix [for]="endPicker"></mat-datepicker-toggle>
                <mat-datepicker #endPicker></mat-datepicker>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>OEMコード</mat-label>
                <mat-select formControlName="oemCode">
                  <mat-option value="">すべて</mat-option>
                  <mat-option value="TOYOTA">Toyota</mat-option>
                  <mat-option value="HONDA">Honda</mat-option>
                  <mat-option value="NISSAN">Nissan</mat-option>
                  <mat-option value="MAZDA">Mazda</mat-option>
                  <mat-option value="SUBARU">Subaru</mat-option>
                </mat-select>
              </mat-form-field>

              <button mat-raised-button color="primary" (click)="applyFilters()">
                分析実行
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>

      <!-- Loading Spinner -->
      <div class="loading-container" *ngIf="loading">
        <mat-spinner diameter="50"></mat-spinner>
      </div>

      <!-- Statistics Content -->
      <div class="statistics-content" *ngIf="!loading">
        <mat-tab-group class="statistics-tabs">
          <!-- System Analysis Tab -->
          <mat-tab label="システム別分析">
            <div class="tab-content">
              <div class="system-reports-grid">
                <mat-card *ngFor="let report of systemReports" class="system-report-card">
                  <mat-card-header>
                    <mat-card-title>{{ getSystemTypeText(report.systemType) }}</mat-card-title>
                    <mat-card-subtitle>システム別異常分析</mat-card-subtitle>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="report-metrics">
                      <div class="metric-row">
                        <span class="metric-label">総異常数:</span>
                        <span class="metric-value">{{ report.totalAnomalies }}</span>
                      </div>
                      <div class="metric-row">
                        <span class="metric-label">解決済み:</span>
                        <span class="metric-value resolved">{{ report.resolvedAnomalies }}</span>
                      </div>
                      <div class="metric-row">
                        <span class="metric-label">重要異常:</span>
                        <span class="metric-value critical">{{ report.criticalAnomalies }}</span>
                      </div>
                      <div class="metric-row">
                        <span class="metric-label">平均解決時間:</span>
                        <span class="metric-value">{{ report.averageResolutionTime | number:'1.1-1' }}時間</span>
                      </div>
                    </div>

                    <mat-divider></mat-divider>

                    <div class="top-signals">
                      <h4>主要異常信号</h4>
                      <div class="signal-list">
                        <div class="signal-item" *ngFor="let signal of report.topSignals">
                          <div class="signal-info">
                            <div class="signal-name">{{ signal.signalName }}</div>
                            <div class="signal-id">{{ signal.canId }}</div>
                          </div>
                          <div class="signal-stats">
                            <div class="anomaly-count">{{ signal.anomalyCount }}件</div>
                            <div class="last-detection">{{ signal.lastDetection | date:'MM/dd HH:mm' }}</div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </mat-card-content>
                  <mat-card-actions>
                    <button mat-button (click)="generateSystemReport(report.systemType)">
                      <mat-icon>assessment</mat-icon>
                      詳細レポート
                    </button>
                  </mat-card-actions>
                </mat-card>
              </div>
            </div>
          </mat-tab>

          <!-- OEM Comparison Tab -->
          <mat-tab label="OEM比較分析">
            <div class="tab-content">
              <mat-card class="comparison-card">
                <mat-card-header>
                  <mat-card-title>OEM間比較分析</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="comparison-controls">
                    <mat-form-field appearance="outline">
                      <mat-label>比較対象OEM</mat-label>
                      <mat-select formControlName="compareOems" multiple>
                        <mat-option value="TOYOTA">Toyota</mat-option>
                        <mat-option value="HONDA">Honda</mat-option>
                        <mat-option value="NISSAN">Nissan</mat-option>
                        <mat-option value="MAZDA">Mazda</mat-option>
                        <mat-option value="SUBARU">Subaru</mat-option>
                      </mat-select>
                    </mat-form-field>
                    <button mat-raised-button color="primary" (click)="generateOemComparison()">
                      比較分析実行
                    </button>
                  </div>

                  <div class="comparison-results" *ngIf="oemComparisonData">
                    <div class="comparison-chart">
                      <!-- TODO: Implement comparison chart -->
                      <div class="chart-placeholder">
                        <mat-icon>compare</mat-icon>
                        <p>OEM比較チャート表示予定</p>
                      </div>
                    </div>

                    <div class="comparison-table">
                      <mat-table [dataSource]="oemComparisonData" class="comparison-data-table">
                        <ng-container matColumnDef="oem">
                          <mat-header-cell *matHeaderCellDef>OEM</mat-header-cell>
                          <mat-cell *matCellDef="let element">{{ element.oem }}</mat-cell>
                        </ng-container>

                        <ng-container matColumnDef="totalAnomalies">
                          <mat-header-cell *matHeaderCellDef>総異常数</mat-header-cell>
                          <mat-cell *matCellDef="let element">{{ element.totalAnomalies }}</mat-cell>
                        </ng-container>

                        <ng-container matColumnDef="resolutionRate">
                          <mat-header-cell *matHeaderCellDef>解決率</mat-header-cell>
                          <mat-cell *matCellDef="let element">{{ element.resolutionRate }}%</mat-cell>
                        </ng-container>

                        <ng-container matColumnDef="averageTime">
                          <mat-header-cell *matHeaderCellDef>平均解決時間</mat-header-cell>
                          <mat-cell *matCellDef="let element">{{ element.averageTime }}h</mat-cell>
                        </ng-container>

                        <mat-header-row *matHeaderRowDef="comparisonColumns"></mat-header-row>
                        <mat-row *matRowDef="let row; columns: comparisonColumns;"></mat-row>
                      </mat-table>
                    </div>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
          </mat-tab>

          <!-- Custom Reports Tab -->
          <mat-tab label="カスタムレポート">
            <div class="tab-content">
              <mat-card class="custom-report-card">
                <mat-card-header>
                  <mat-card-title>カスタムレポート生成</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <form [formGroup]="reportForm" class="report-form">
                    <div class="form-section">
                      <h4>レポート設定</h4>
                      
                      <mat-form-field appearance="outline" class="full-width">
                        <mat-label>レポート名</mat-label>
                        <input matInput formControlName="reportName" placeholder="レポート名を入力">
                      </mat-form-field>

                      <div class="form-row">
                        <mat-form-field appearance="outline">
                          <mat-label>対象システム</mat-label>
                          <mat-select formControlName="targetSystem">
                            <mat-option value="">すべて</mat-option>
                            <mat-option value="Engine">エンジン</mat-option>
                            <mat-option value="Brake">ブレーキ</mat-option>
                            <mat-option value="Steering">ステアリング</mat-option>
                            <mat-option value="Safety">安全系</mat-option>
                            <mat-option value="ADAS">ADAS</mat-option>
                          </mat-select>
                        </mat-form-field>

                        <mat-form-field appearance="outline">
                          <mat-label>出力形式</mat-label>
                          <mat-select formControlName="format">
                            <mat-option value="pdf">PDF</mat-option>
                            <mat-option value="excel">Excel</mat-option>
                            <mat-option value="csv">CSV</mat-option>
                          </mat-select>
                        </mat-form-field>
                      </div>
                    </div>

                    <mat-divider></mat-divider>

                    <div class="form-section">
                      <h4>含める内容</h4>
                      
                      <div class="checkbox-group">
                        <mat-checkbox formControlName="includeOverview">
                          概要統計
                        </mat-checkbox>
                        <mat-checkbox formControlName="includeCharts">
                          グラフ・チャート
                        </mat-checkbox>
                        <mat-checkbox formControlName="includeDetailedData">
                          詳細データ
                        </mat-checkbox>
                        <mat-checkbox formControlName="includeTrendAnalysis">
                          トレンド分析
                        </mat-checkbox>
                        <mat-checkbox formControlName="includeRecommendations">
                          推奨事項
                        </mat-checkbox>
                      </div>
                    </div>

                    <mat-divider></mat-divider>

                    <div class="form-section">
                      <h4>期間設定</h4>
                      
                      <div class="form-row">
                        <mat-form-field appearance="outline">
                          <mat-label>開始日</mat-label>
                          <input matInput [matDatepicker]="reportStartPicker" formControlName="reportStartDate">
                          <mat-datepicker-toggle matSuffix [for]="reportStartPicker"></mat-datepicker-toggle>
                          <mat-datepicker #reportStartPicker></mat-datepicker>
                        </mat-form-field>

                        <mat-form-field appearance="outline">
                          <mat-label>終了日</mat-label>
                          <input matInput [matDatepicker]="reportEndPicker" formControlName="reportEndDate">
                          <mat-datepicker-toggle matSuffix [for]="reportEndPicker"></mat-datepicker-toggle>
                          <mat-datepicker #reportEndPicker></mat-datepicker>
                        </mat-form-field>
                      </div>
                    </div>
                  </form>
                </mat-card-content>
                <mat-card-actions>
                  <button mat-raised-button color="primary" 
                          [disabled]="reportForm.invalid || generatingReport"
                          (click)="generateCustomReport()">
                    <mat-icon *ngIf="!generatingReport">description</mat-icon>
                    <mat-spinner *ngIf="generatingReport" diameter="20"></mat-spinner>
                    {{ generatingReport ? 'レポート生成中...' : 'レポート生成' }}
                  </button>
                </mat-card-actions>
              </mat-card>
            </div>
          </mat-tab>

          <!-- Trend Analysis Tab -->
          <mat-tab label="トレンド分析">
            <div class="tab-content">
              <mat-card class="trend-analysis-card">
                <mat-card-header>
                  <mat-card-title>異常検出トレンド分析</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="trend-controls">
                    <mat-form-field appearance="outline">
                      <mat-label>分析期間</mat-label>
                      <mat-select formControlName="trendPeriod" (selectionChange)="loadTrendAnalysis()">
                        <mat-option value="week">過去1週間</mat-option>
                        <mat-option value="month">過去1ヶ月</mat-option>
                        <mat-option value="quarter">過去3ヶ月</mat-option>
                        <mat-option value="year">過去1年</mat-option>
                      </mat-select>
                    </mat-form-field>
                  </div>

                  <div class="trend-results" *ngIf="trendAnalysisData">
                    <div class="trend-summary">
                      <div class="trend-metric">
                        <div class="trend-title">検出数トレンド</div>
                        <div class="trend-value" [class]="getTrendClass(trendAnalysisData.detectionTrend)">
                          <mat-icon>{{ getTrendIcon(trendAnalysisData.detectionTrend) }}</mat-icon>
                          {{ trendAnalysisData.detectionTrend.percentage }}%
                        </div>
                        <div class="trend-description">{{ trendAnalysisData.detectionTrend.period }}</div>
                      </div>

                      <div class="trend-metric">
                        <div class="trend-title">解決率トレンド</div>
                        <div class="trend-value" [class]="getTrendClass(trendAnalysisData.resolutionTrend)">
                          <mat-icon>{{ getTrendIcon(trendAnalysisData.resolutionTrend) }}</mat-icon>
                          {{ trendAnalysisData.resolutionTrend.percentage }}%
                        </div>
                        <div class="trend-description">{{ trendAnalysisData.resolutionTrend.period }}</div>
                      </div>
                    </div>

                    <div class="trend-chart">
                      <!-- TODO: Implement trend chart -->
                      <div class="chart-placeholder">
                        <mat-icon>trending_up</mat-icon>
                        <p>トレンドチャート表示予定</p>
                      </div>
                    </div>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
          </mat-tab>
        </mat-tab-group>
      </div>
    </div>
  `,
  styleUrls: ['./detailed-statistics.component.scss']
})
export class DetailedStatisticsComponent implements OnInit, OnDestroy {
  systemReports: SystemAnomalyReport[] = [];
  oemComparisonData: any[] = [];
  trendAnalysisData: any;
  loading = false;
  generatingReport = false;
  
  filterForm: FormGroup;
  reportForm: FormGroup;
  
  comparisonColumns: string[] = ['oem', 'totalAnomalies', 'resolutionRate', 'averageTime'];
  
  private destroy$ = new Subject<void>();

  constructor(
    private dashboardService: DashboardService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.filterForm = this.createFilterForm();
    this.reportForm = this.createReportForm();
  }

  ngOnInit(): void {
    this.loadSystemReports();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createFilterForm(): FormGroup {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(endDate.getDate() - 30);

    return this.fb.group({
      startDate: [startDate],
      endDate: [endDate],
      oemCode: [''],
      compareOems: [[]],
      trendPeriod: ['month']
    });
  }

  private createReportForm(): FormGroup {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(endDate.getDate() - 30);

    return this.fb.group({
      reportName: ['異常検出統計レポート'],
      targetSystem: [''],
      format: ['pdf'],
      includeOverview: [true],
      includeCharts: [true],
      includeDetailedData: [false],
      includeTrendAnalysis: [true],
      includeRecommendations: [true],
      reportStartDate: [startDate],
      reportEndDate: [endDate]
    });
  }

  loadSystemReports(): void {
    this.loading = true;
    
    const systems = ['Engine', 'Brake', 'Steering', 'Safety', 'ADAS'];
    const filterValue = this.filterForm.value;
    
    const input: GetDetectionStatisticsInput = {
      startDate: filterValue.startDate,
      endDate: filterValue.endDate,
      oemCode: filterValue.oemCode || undefined
    };

    // Load reports for each system
    const reportPromises = systems.map(system => 
      this.dashboardService.getSystemAnomalyReport(system, input).toPromise()
    );

    Promise.all(reportPromises)
      .then(reports => {
        this.systemReports = reports.filter(report => report != null) as SystemAnomalyReport[];
        this.loading = false;
      })
      .catch(error => {
        console.error('Error loading system reports:', error);
        this.snackBar.open('システムレポートの読み込みに失敗しました', '閉じる', { duration: 3000 });
        this.loading = false;
      });
  }

  applyFilters(): void {
    this.loadSystemReports();
  }

  refreshData(): void {
    this.loadSystemReports();
  }

  generateSystemReport(systemType: string): void {
    const filterValue = this.filterForm.value;
    const input: GenerateSystemReportInput = {
      systemType,
      startDate: filterValue.startDate,
      endDate: filterValue.endDate,
      includeDetails: true,
      format: 'pdf'
    };

    this.dashboardService.generateSystemReport(input)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `${systemType}-report-${new Date().toISOString().split('T')[0]}.pdf`;
          a.click();
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          console.error('Error generating system report:', error);
          this.snackBar.open('システムレポートの生成に失敗しました', '閉じる', { duration: 3000 });
        }
      });
  }

  generateOemComparison(): void {
    const compareOems = this.filterForm.get('compareOems')?.value || [];
    
    if (compareOems.length < 2) {
      this.snackBar.open('比較するOEMを2つ以上選択してください', '閉じる', { duration: 3000 });
      return;
    }

    this.dashboardService.getOemComparison(compareOems)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.oemComparisonData = data;
        },
        error: (error) => {
          console.error('Error generating OEM comparison:', error);
          this.snackBar.open('OEM比較分析に失敗しました', '閉じる', { duration: 3000 });
        }
      });
  }

  generateCustomReport(): void {
    this.generatingReport = true;
    const reportValue = this.reportForm.value;
    
    const input: GenerateSystemReportInput = {
      systemType: reportValue.targetSystem || undefined,
      startDate: reportValue.reportStartDate,
      endDate: reportValue.reportEndDate,
      includeDetails: reportValue.includeDetailedData,
      format: reportValue.format
    };

    this.dashboardService.generateSystemReport(input)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `${reportValue.reportName}-${new Date().toISOString().split('T')[0]}.${reportValue.format}`;
          a.click();
          window.URL.revokeObjectURL(url);
          this.generatingReport = false;
          this.snackBar.open('レポートを生成しました', '閉じる', { duration: 2000 });
        },
        error: (error) => {
          console.error('Error generating custom report:', error);
          this.snackBar.open('カスタムレポートの生成に失敗しました', '閉じる', { duration: 3000 });
          this.generatingReport = false;
        }
      });
  }

  loadTrendAnalysis(): void {
    const period = this.filterForm.get('trendPeriod')?.value;
    
    this.dashboardService.getTrendAnalysis(period)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.trendAnalysisData = data;
        },
        error: (error) => {
          console.error('Error loading trend analysis:', error);
          this.snackBar.open('トレンド分析の読み込みに失敗しました', '閉じる', { duration: 3000 });
        }
      });
  }

  getSystemTypeText(systemType: string): string {
    const texts: Record<string, string> = {
      'Engine': 'エンジン',
      'Brake': 'ブレーキ',
      'Steering': 'ステアリング',
      'Safety': '安全系',
      'ADAS': 'ADAS',
      'Transmission': 'トランスミッション',
      'Body': 'ボディ',
      'Chassis': 'シャーシ'
    };
    return texts[systemType] || systemType;
  }

  getTrendIcon(trend: any): string {
    switch (trend?.direction) {
      case 'up': return 'trending_up';
      case 'down': return 'trending_down';
      case 'stable': return 'trending_flat';
      default: return 'trending_flat';
    }
  }

  getTrendClass(trend: any): string {
    switch (trend?.direction) {
      case 'up': return 'trend-up';
      case 'down': return 'trend-down';
      case 'stable': return 'trend-stable';
      default: return 'trend-stable';
    }
  }
}