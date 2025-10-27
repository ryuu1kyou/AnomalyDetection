import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';

import { AnomalyAnalysisService } from '../../services/anomaly-analysis.service';
import { DetectionAccuracyMetricsDto, DetectionAccuracyRequestDto } from '../../models/anomaly-analysis.models';

@Component({
  selector: 'app-accuracy-metrics',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatChipsModule,
    MatIconModule,
    MatTabsModule,
    BaseChartDirective
  ],
  templateUrl: './accuracy-metrics.component.html',
  styleUrls: ['./accuracy-metrics.component.scss']
})
export class AccuracyMetricsComponent implements OnInit {
  metricsForm: FormGroup;
  isLoading = false;
  metricsResult: DetectionAccuracyMetricsDto | null = null;

  // Chart configurations
  public doughnutChartType: ChartType = 'doughnut';
  public doughnutChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    plugins: {
      legend: {
        display: true,
        position: 'bottom',
      }
    }
  };

  public lineChartType: ChartType = 'line';
  public lineChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    scales: {
      y: {
        beginAtZero: true,
        max: 1
      }
    }
  };

  // Chart data
  public confusionMatrixChartData: ChartData<'doughnut'> = {
    labels: [],
    datasets: []
  };

  public accuracyByTypeChartData: ChartData<'line'> = {
    labels: [],
    datasets: []
  };

  public accuracyByTimeChartData: ChartData<'line'> = {
    labels: [],
    datasets: []
  };

  // Table columns
  displayedColumnsType = ['anomalyType', 'truePositives', 'falsePositives', 'falseNegatives', 'precision', 'recall', 'f1Score'];
  displayedColumnsTime = ['timeRange', 'truePositives', 'falsePositives', 'falseNegatives', 'precision', 'recall', 'f1Score'];

  constructor(
    private fb: FormBuilder,
    private anomalyAnalysisService: AnomalyAnalysisService
  ) {
    this.metricsForm = this.fb.group({
      detectionLogicId: ['', Validators.required],
      analysisStartDate: ['', Validators.required],
      analysisEndDate: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    // Set default date range (last 30 days)
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(endDate.getDate() - 30);

    this.metricsForm.patchValue({
      analysisStartDate: startDate,
      analysisEndDate: endDate
    });
  }

  onCalculateMetrics(): void {
    if (this.metricsForm.valid) {
      this.isLoading = true;
      
      const request: DetectionAccuracyRequestDto = {
        detectionLogicId: this.metricsForm.value.detectionLogicId,
        analysisStartDate: this.metricsForm.value.analysisStartDate,
        analysisEndDate: this.metricsForm.value.analysisEndDate
      };

      this.anomalyAnalysisService.getDetectionAccuracyMetrics(request).subscribe({
        next: (result) => {
          this.metricsResult = result;
          this.updateCharts();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Metrics calculation failed:', error);
          this.isLoading = false;
        }
      });
    }
  }

  private updateCharts(): void {
    if (!this.metricsResult) return;

    // Update confusion matrix chart
    this.confusionMatrixChartData = {
      labels: ['True Positives', 'False Positives', 'True Negatives', 'False Negatives'],
      datasets: [{
        data: [
          this.metricsResult.truePositives,
          this.metricsResult.falsePositives,
          this.metricsResult.trueNegatives,
          this.metricsResult.falseNegatives
        ],
        backgroundColor: [
          '#4caf50',
          '#f44336',
          '#2196f3',
          '#ff9800'
        ]
      }]
    };

    // Update accuracy by type chart
    if (this.metricsResult.accuracyByType.length > 0) {
      const typeLabels = this.metricsResult.accuracyByType.map(a => a.anomalyType);
      
      this.accuracyByTypeChartData = {
        labels: typeLabels,
        datasets: [
          {
            label: 'Precision',
            data: this.metricsResult.accuracyByType.map(a => a.precision),
            borderColor: '#ff6384',
            backgroundColor: 'rgba(255, 99, 132, 0.2)',
            tension: 0.1
          },
          {
            label: 'Recall',
            data: this.metricsResult.accuracyByType.map(a => a.recall),
            borderColor: '#36a2eb',
            backgroundColor: 'rgba(54, 162, 235, 0.2)',
            tension: 0.1
          },
          {
            label: 'F1 Score',
            data: this.metricsResult.accuracyByType.map(a => a.f1Score),
            borderColor: '#4bc0c0',
            backgroundColor: 'rgba(75, 192, 192, 0.2)',
            tension: 0.1
          }
        ]
      };
    }

    // Update accuracy by time chart
    if (this.metricsResult.accuracyByTime.length > 0) {
      const timeLabels = this.metricsResult.accuracyByTime.map(a => 
        new Date(a.startTime).toLocaleDateString()
      );
      
      this.accuracyByTimeChartData = {
        labels: timeLabels,
        datasets: [
          {
            label: 'Precision',
            data: this.metricsResult.accuracyByTime.map(a => a.precision),
            borderColor: '#ff6384',
            backgroundColor: 'rgba(255, 99, 132, 0.2)',
            tension: 0.1
          },
          {
            label: 'Recall',
            data: this.metricsResult.accuracyByTime.map(a => a.recall),
            borderColor: '#36a2eb',
            backgroundColor: 'rgba(54, 162, 235, 0.2)',
            tension: 0.1
          },
          {
            label: 'F1 Score',
            data: this.metricsResult.accuracyByTime.map(a => a.f1Score),
            borderColor: '#4bc0c0',
            backgroundColor: 'rgba(75, 192, 192, 0.2)',
            tension: 0.1
          }
        ]
      };
    }
  }

  getPerformanceColor(value: number): string {
    if (value >= 0.9) return 'success';
    if (value >= 0.8) return 'primary';
    if (value >= 0.7) return 'warning';
    return 'warn';
  }

  getPerformanceLabel(value: number): string {
    if (value >= 0.9) return 'Excellent';
    if (value >= 0.8) return 'Good';
    if (value >= 0.7) return 'Fair';
    return 'Poor';
  }

  formatPercentage(value: number): string {
    return `${(value * 100).toFixed(2)}%`;
  }

  formatDuration(durationMs: number): string {
    if (durationMs < 1000) {
      return `${durationMs.toFixed(1)} ms`;
    } else if (durationMs < 60000) {
      return `${(durationMs / 1000).toFixed(1)} s`;
    } else {
      return `${(durationMs / 60000).toFixed(1)} min`;
    }
  }

  formatTimeRange(startTime: Date, endTime: Date): string {
    const start = new Date(startTime).toLocaleDateString();
    const end = new Date(endTime).toLocaleDateString();
    return `${start} - ${end}`;
  }

  getOverallPerformanceAssessment(): string {
    if (!this.metricsResult) return '';
    
    const f1Score = this.metricsResult.f1Score;
    const accuracy = this.metricsResult.accuracy;
    
    if (f1Score >= 0.9 && accuracy >= 0.9) {
      return 'Excellent performance with high precision and recall';
    } else if (f1Score >= 0.8 && accuracy >= 0.8) {
      return 'Good performance with acceptable precision and recall';
    } else if (f1Score >= 0.7 && accuracy >= 0.7) {
      return 'Fair performance, consider threshold optimization';
    } else {
      return 'Poor performance, threshold optimization recommended';
    }
  }

  getRecommendations(): string[] {
    if (!this.metricsResult) return [];
    
    const recommendations: string[] = [];
    
    if (this.metricsResult.precision < 0.8) {
      recommendations.push('Consider increasing detection thresholds to reduce false positives');
    }
    
    if (this.metricsResult.recall < 0.8) {
      recommendations.push('Consider decreasing detection thresholds to reduce false negatives');
    }
    
    if (this.metricsResult.averageDetectionTimeMs > 1000) {
      recommendations.push('Optimize detection algorithm for better response time');
    }
    
    if (this.metricsResult.f1Score < 0.7) {
      recommendations.push('Review detection logic parameters and consider retraining');
    }
    
    return recommendations;
  }
}