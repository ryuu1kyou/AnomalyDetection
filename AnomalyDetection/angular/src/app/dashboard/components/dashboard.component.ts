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
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';

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
    BaseChartDirective,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit, OnDestroy {
  loading = false;
  filterForm: FormGroup;
  detectionStats?: DetectionStatistics;
  dashboardStats?: DashboardStatistics;
  overviewMetrics: MetricCard[] = [];
  
  private destroy$ = new Subject<void>();

  // Charts Properties
  public lineChartData: ChartConfiguration<'line'>['data'] = {
    datasets: [
      {
        data: [],
        label: '検出数推移',
        fill: 'origin',
        backgroundColor: 'rgba(79, 70, 229, 0.1)',
        borderColor: '#4f46e5',
        pointBackgroundColor: '#4f46e5',
        pointBorderColor: '#fff',
        pointHoverBackgroundColor: '#fff',
        pointHoverBorderColor: '#4f46e5',
        tension: 0.4,
      }
    ],
    labels: []
  };

  public lineChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        mode: 'index',
        intersect: false,
        backgroundColor: 'rgba(15, 23, 42, 0.9)',
        padding: 12,
        titleFont: { family: "'Outfit', sans-serif" as any, size: 14 },
        bodyFont: { family: "'Inter', sans-serif" as any, size: 13 },
      }
    },
    scales: {
      y: { display: false },
      x: { grid: { display: false } }
    }
  };

  public pieChartData: ChartData<'doughnut'> = {
    labels: ['Critical', 'High', 'Medium', 'Low'],
    datasets: [
      {
        data: [0, 0, 0, 0],
        backgroundColor: ['#ef4444', '#f97316', '#f59e0b', '#10b981'],
        hoverOffset: 4,
        borderWidth: 0,
      }
    ]
  };

  public pieChartOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '75%',
    plugins: {
      legend: { display: false }
    }
  };

  constructor(
    private fb: FormBuilder,
    private dashboardService: DashboardService,
    private snackBar: MatSnackBar
  ) {
    this.filterForm = this.fb.group({
      startDate: [new Date(new Date().setDate(new Date().getDate() - 30))],
      endDate: [new Date()],
      systemType: [''],
    });
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  applyFilters(): void {
    this.loadDashboardData();
  }

  refreshData(): void {
    this.loadDashboardData();
  }

  private loadDashboardData(): void {
    this.loading = true;
    const input: GetDetectionStatisticsInput = {
      startDate: this.filterForm.value.startDate?.toISOString(),
      endDate: this.filterForm.value.endDate?.toISOString(),
      systemType: this.filterForm.value.systemType || undefined,
    };

    this.dashboardService
      .getStatistics(input)
      .pipe(
        takeUntil(this.destroy$),
        catchError(error => {
          console.error('Error loading dashboard data:', error);
          this.snackBar.open('データの読み込みに失敗しました', '閉じる', { duration: 3000 });
          return of(null);
        })
      )
      .subscribe(stats => {
        if (stats) {
          this.dashboardStats = stats;
          this.detectionStats = stats.detectionStatistics;
          this.updateOverviewMetrics();
          this.updateCharts();
        }
        this.loading = false;
      });
  }

  private updateCharts(): void {
    if (!this.detectionStats) return;

    // Update Line Chart
    if (this.detectionStats.dailyDetections) {
      this.lineChartData = {
        ...this.lineChartData,
        labels: this.detectionStats.dailyDetections.map(d => d.date),
        datasets: [{
          ...this.lineChartData.datasets[0],
          data: this.detectionStats.dailyDetections.map(d => d.count)
        }]
      };
    }

    // Update Severity Distribution
    if (this.detectionStats.anomalyLevelDistribution) {
      const severityMap: Record<string, number> = {};
      this.detectionStats.anomalyLevelDistribution.forEach(d => {
        severityMap[d.category] = d.value;
      });

      this.pieChartData = {
        ...this.pieChartData,
        datasets: [{
          ...this.pieChartData.datasets[0],
          data: [
            severityMap['Critical'] || 0,
            severityMap['High'] || 0,
            severityMap['Medium'] || 0,
            severityMap['Low'] || 0
          ]
        }]
      };
    }
  }

  private updateOverviewMetrics(): void {
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
      case 'up': return 'up';
      case 'down': return 'down';
      default: return 'stable';
    }
  }

  private mapKpiIcon(icon: string, title: string): string {
    if (icon) return this.normalizeMaterialIcon(icon);
    const t = title.toLowerCase();
    if (t.includes('critical')) return 'error';
    if (t.includes('signal')) return 'device_hub';
    if (t.includes('detection')) return 'visibility';
    if (t.includes('active')) return 'bolt';
    return 'insights';
  }

  private normalizeMaterialIcon(icon: string): string {
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
      case 'up': return 'trending_up';
      case 'down': return 'trending_down';
      default: return 'trending_flat';
    }
  }

  getTrendClass(trend: TrendData): string {
    switch (trend.direction) {
      case 'up': return 'up';
      case 'down': return 'down';
      default: return 'stable';
    }
  }
}
