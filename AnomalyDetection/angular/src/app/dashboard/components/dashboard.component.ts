import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, takeUntil, interval, switchMap, startWith, catchError, of } from 'rxjs';

import { DashboardService } from '../services/dashboard.service';
import {
  DashboardStatistics,
  DetectionStatistics,
  GetDetectionStatisticsInput,
  MetricCard,
  TrendData,
} from '../models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatInputModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatGridListModule,
    MatDividerModule,
    MatTooltipModule,
  ],
  template: `
    <div class="dashboard-container">
      <!-- Header -->
      <div class="dashboard-header">
        <div class="header-info">
          <h2>統計ダッシュボード</h2>
          <p class="header-description">
            異常検出システムの統計情報とパフォーマンス指標を表示します
          </p>
        </div>
        <div class="header-actions">
          <button mat-raised-button color="primary" (click)="refreshData()">
            <mat-icon>refresh</mat-icon>
            更新
          </button>
          <button mat-button (click)="exportDashboard()">
            <mat-icon>download</mat-icon>
            エクスポート
          </button>
        </div>
      </div>

      <!-- Filters -->
      <mat-card class="filter-card">
        <mat-card-content>
          <form [formGroup]="filterForm" class="filter-form">
            <div class="filter-row">
              <mat-form-field appearance="outline">
                <mat-label>期間（開始）</mat-label>
                <input matInput [matDatepicker]="startPicker" formControlName="startDate" />
                <mat-datepicker-toggle matSuffix [for]="startPicker"></mat-datepicker-toggle>
                <mat-datepicker #startPicker></mat-datepicker>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>期間（終了）</mat-label>
                <input matInput [matDatepicker]="endPicker" formControlName="endDate" />
                <mat-datepicker-toggle matSuffix [for]="endPicker"></mat-datepicker-toggle>
                <mat-datepicker #endPicker></mat-datepicker>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>システム種別</mat-label>
                <mat-select formControlName="systemType">
                  <mat-option value="">すべて</mat-option>
                  <mat-option value="Engine">エンジン</mat-option>
                  <mat-option value="Brake">ブレーキ</mat-option>
                  <mat-option value="Steering">ステアリング</mat-option>
                  <mat-option value="Safety">安全系</mat-option>
                  <mat-option value="ADAS">ADAS</mat-option>
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>集計単位</mat-label>
                <mat-select formControlName="groupBy">
                  <mat-option value="day">日別</mat-option>
                  <mat-option value="week">週別</mat-option>
                  <mat-option value="month">月別</mat-option>
                </mat-select>
              </mat-form-field>

              <button mat-raised-button color="primary" (click)="applyFilters()">適用</button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>

      <!-- Loading Spinner -->
      <div class="loading-container" *ngIf="loading">
        <mat-spinner diameter="50"></mat-spinner>
      </div>

      <!-- Dashboard Content -->
      <div class="dashboard-content" *ngIf="!loading">
        <!-- Overview Metrics -->
        <div class="metrics-section">
          <h3>概要統計</h3>
          <mat-grid-list cols="4" rowHeight="120px" gutterSize="16px">
            <mat-grid-tile *ngFor="let metric of overviewMetrics">
              <mat-card class="metric-card">
                <mat-card-content>
                  <div class="metric-content">
                    <div class="metric-icon" [class]="metric.color">
                      <mat-icon>{{ metric.icon }}</mat-icon>
                    </div>
                    <div class="metric-info">
                      <div class="metric-value">{{ metric.value }}</div>
                      <div class="metric-title">{{ metric.title }}</div>
                      <div class="metric-trend" *ngIf="metric.trend">
                        <mat-icon [class]="getTrendClass(metric.trend)">
                          {{ getTrendIcon(metric.trend) }}
                        </mat-icon>
                        <span>{{ metric.trend.percentage }}%</span>
                      </div>
                    </div>
                  </div>
                </mat-card-content>
              </mat-card>
            </mat-grid-tile>
          </mat-grid-list>
        </div>

        <mat-divider></mat-divider>

        <!-- Charts Section -->
        <div class="charts-section">
          <div class="charts-grid">
            <!-- Anomaly Level Distribution -->
            <mat-card class="chart-card">
              <mat-card-header>
                <mat-card-title>異常レベル別分布</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="chart-container">
                  <div class="chart-placeholder" *ngIf="!detectionStats?.anomalyLevelDistribution">
                    <mat-icon>pie_chart</mat-icon>
                    <p>データを読み込み中...</p>
                  </div>
                  <div class="distribution-chart" *ngIf="detectionStats?.anomalyLevelDistribution">
                    <div
                      class="distribution-item"
                      *ngFor="let item of detectionStats.anomalyLevelDistribution"
                    >
                      <div class="distribution-bar">
                        <div
                          class="bar-fill"
                          [style.width.%]="item.percentage"
                          [class]="'level-' + item.category.toLowerCase()"
                        ></div>
                      </div>
                      <div class="distribution-info">
                        <span class="category">{{ item.category }}</span>
                        <span class="value">{{ item.value }} ({{ item.percentage }}%)</span>
                      </div>
                    </div>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>

            <!-- System Type Distribution -->
            <mat-card class="chart-card">
              <mat-card-header>
                <mat-card-title>システム別異常数</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="chart-container">
                  <div class="chart-placeholder" *ngIf="!detectionStats?.systemTypeDistribution">
                    <mat-icon>bar_chart</mat-icon>
                    <p>データを読み込み中...</p>
                  </div>
                  <div class="distribution-chart" *ngIf="detectionStats?.systemTypeDistribution">
                    <div
                      class="distribution-item"
                      *ngFor="let item of detectionStats.systemTypeDistribution"
                    >
                      <div class="distribution-bar">
                        <div class="bar-fill system-bar" [style.width.%]="item.percentage"></div>
                      </div>
                      <div class="distribution-info">
                        <span class="category">{{ item.category }}</span>
                        <span class="value">{{ item.value }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>

            <!-- Detection Trend -->
            <mat-card class="chart-card full-width">
              <mat-card-header>
                <mat-card-title>検出数推移</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="chart-container">
                  <div class="chart-placeholder" *ngIf="!detectionStats?.dailyDetections">
                    <mat-icon>timeline</mat-icon>
                    <p>データを読み込み中...</p>
                  </div>
                  <div class="timeline-chart" *ngIf="detectionStats?.dailyDetections">
                    <!-- TODO: Implement actual chart library integration -->
                    <div class="timeline-placeholder">
                      <mat-icon>show_chart</mat-icon>
                      <p>時系列グラフ表示予定</p>
                      <p class="chart-info">
                        期間: {{ detectionStats.dailyDetections.length }}日間のデータ
                      </p>
                    </div>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>

            <!-- Resolution Status -->
            <mat-card class="chart-card">
              <mat-card-header>
                <mat-card-title>解決状況</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="chart-container">
                  <div
                    class="chart-placeholder"
                    *ngIf="!detectionStats?.resolutionStatusDistribution"
                  >
                    <mat-icon>donut_small</mat-icon>
                    <p>データを読み込み中...</p>
                  </div>
                  <div
                    class="distribution-chart"
                    *ngIf="detectionStats?.resolutionStatusDistribution"
                  >
                    <div
                      class="distribution-item"
                      *ngFor="let item of detectionStats.resolutionStatusDistribution"
                    >
                      <div class="distribution-bar">
                        <div
                          class="bar-fill resolution-bar"
                          [style.width.%]="item.percentage"
                          [class]="'status-' + item.category.toLowerCase()"
                        ></div>
                      </div>
                      <div class="distribution-info">
                        <span class="category">{{ item.category }}</span>
                        <span class="value">{{ item.value }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit, OnDestroy {
  dashboardStats?: DashboardStatistics;
  detectionStats?: DetectionStatistics;
  overviewMetrics: MetricCard[] = [];
  loading = false;

  filterForm: FormGroup;

  private destroy$ = new Subject<void>();
  private refreshInterval = 30000; // 30 seconds

  constructor(
    private dashboardService: DashboardService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.filterForm = this.createFilterForm();
  }

  ngOnInit(): void {
    this.loadDashboardData();
    this.setupAutoRefresh();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createFilterForm(): FormGroup {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(endDate.getDate() - 30); // Default to last 30 days

    return this.fb.group({
      startDate: [startDate],
      endDate: [endDate],
      systemType: [''],
      groupBy: ['day'],
    });
  }

  private setupAutoRefresh(): void {
    interval(this.refreshInterval)
      .pipe(
        startWith(0),
        switchMap(() =>
          this.dashboardService.getDashboardStatistics().pipe(
            catchError(err => {
              // Treat JSON parse errors (status 200 with ok=false) as empty stats instead of surfacing as errors.
              if (err?.status === 200 && err?.error instanceof SyntaxError) {
                console.warn(
                  'Received 200 with parse error for dashboard statistics. Using empty fallback.'
                );
                return of({} as DashboardStatistics);
              }
              console.error('Real-time dashboard stats error:', err);
              return of({} as DashboardStatistics);
            })
          )
        ),
        takeUntil(this.destroy$)
      )
      .subscribe(stats => {
        this.dashboardStats = stats;
        this.updateOverviewMetrics();
      });
  }

  loadDashboardData(): void {
    this.loading = true;

    const filterValue = this.filterForm.value;
    const input: GetDetectionStatisticsInput = {
      startDate: filterValue.startDate,
      endDate: filterValue.endDate,
      systemType: filterValue.systemType || undefined,
      groupBy: filterValue.groupBy,
    };

    this.dashboardService
      .getDetectionStatistics(input)
      .pipe(
        catchError(err => {
          if (err?.status === 200 && err?.error instanceof SyntaxError) {
            console.warn(
              'Received 200 with parse error for detection statistics. Using empty fallback.'
            );
            return of({} as DetectionStatistics);
          }
          console.error('Error loading detection statistics:', err);
          this.snackBar.open('統計データの読み込みに失敗しました', '閉じる', { duration: 3000 });
          return of({} as DetectionStatistics);
        }),
        takeUntil(this.destroy$)
      )
      .subscribe(stats => {
        this.detectionStats = stats;
        this.loading = false;
      });
  }

  private updateOverviewMetrics(): void {
    // Backend provides KPI cards in dashboardStats.KpiCards; map them to MetricCard UI structure.
    if (!this.dashboardStats || !('KpiCards' in this.dashboardStats)) {
      this.overviewMetrics = [];
      return;
    }
    const kpiCards: any[] = (this.dashboardStats as any).KpiCards || [];
    this.overviewMetrics = kpiCards.map(card => {
      const trend: TrendData | undefined =
        card.percentageChange != null
          ? {
              direction: this.mapTrendDirection(card.trendDirection),
              percentage: Math.round(card.percentageChange),
              period: '前期間比',
            }
          : undefined;
      return {
        title: card.title,
        value: card.value + (card.unit ? card.unit : ''),
        icon: this.mapKpiIcon(card.icon, card.title),
        color: this.mapKpiColor(card.color),
        trend,
        description: card.description,
      } as MetricCard;
    });
  }

  private mapTrendDirection(dir: string): 'up' | 'down' | 'stable' {
    switch ((dir || '').toLowerCase()) {
      case 'up':
        return 'up';
      case 'down':
        return 'down';
      default:
        return 'stable';
    }
  }

  private mapKpiIcon(icon: string, title: string): string {
    if (icon) return this.normalizeMaterialIcon(icon);
    // Fallback by keyword
    const t = title.toLowerCase();
    if (t.includes('critical')) return 'error';
    if (t.includes('signal')) return 'device_hub';
    if (t.includes('detection')) return 'visibility';
    if (t.includes('active')) return 'bolt';
    return 'insights';
  }

  private normalizeMaterialIcon(icon: string): string {
    // Backend used font-awesome style names (e.g., chart-line, exclamation-triangle). Map to Material icons.
    const map: Record<string, string> = {
      'chart-line': 'show_chart',
      'exclamation-triangle': 'warning',
      signal: 'device_hub',
    };
    return map[icon] || icon || 'insights';
  }

  private mapKpiColor(color: string): string {
    const c = (color || '').toLowerCase();
    if (c === 'success' || c === 'info' || c === 'primary') return 'primary';
    if (c === 'danger' || c === 'error' || c === 'warn') return 'warn';
    if (c === 'warning' || c === 'accent') return 'accent';
    return 'primary';
  }

  applyFilters(): void {
    this.loadDashboardData();
  }

  refreshData(): void {
    this.loadDashboardData();
  }

  exportDashboard(): void {
    this.dashboardService
      .exportDashboardData('pdf')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: blob => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `dashboard-report-${new Date().toISOString().split('T')[0]}.pdf`;
          a.click();
          window.URL.revokeObjectURL(url);
        },
        error: error => {
          console.error('Error exporting dashboard:', error);
          this.snackBar.open('エクスポートに失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  getTrendIcon(trend: TrendData): string {
    switch (trend.direction) {
      case 'up':
        return 'trending_up';
      case 'down':
        return 'trending_down';
      case 'stable':
        return 'trending_flat';
      default:
        return 'trending_flat';
    }
  }

  getTrendClass(trend: TrendData): string {
    switch (trend.direction) {
      case 'up':
        return 'trend-up';
      case 'down':
        return 'trend-down';
      case 'stable':
        return 'trend-stable';
      default:
        return 'trend-stable';
    }
  }
}
