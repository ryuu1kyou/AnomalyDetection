import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormsModule,
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
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
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';

import { AnomalyAnalysisService } from '../../services/anomaly-analysis.service';
import {
  ThresholdRecommendationResultDto,
  ThresholdRecommendationRequestDto,
} from '../../models/anomaly-analysis.models';

@Component({
  selector: 'app-threshold-recommendations',
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
    MatProgressBarModule,
    MatSlideToggleModule,
    BaseChartDirective,
  ],
  templateUrl: './threshold-recommendations.component.html',
  styleUrls: ['./threshold-recommendations.component.scss'],
})
export class ThresholdRecommendationsComponent implements OnInit {
  recommendationForm: FormGroup;
  isLoading = false;
  recommendationResult: ThresholdRecommendationResultDto | null = null;
  useAdvancedRecommendations = false; // NEW: toggle for ML-based recommendations

  // Chart configurations
  public radarChartType: ChartType = 'radar';
  public radarChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    scales: {
      r: {
        beginAtZero: true,
        max: 1,
      },
    },
  };

  public metricsComparisonChartData: ChartData<'radar'> = {
    labels: ['Detection Rate', 'Precision', 'Recall', 'F1 Score', 'Specificity'],
    datasets: [],
  };

  // Table columns
  displayedColumns = [
    'parameterName',
    'currentValue',
    'recommendedValue',
    'priority',
    'confidenceLevel',
    'reason',
  ];

  constructor(private fb: FormBuilder, private anomalyAnalysisService: AnomalyAnalysisService) {
    this.recommendationForm = this.fb.group({
      detectionLogicId: ['', Validators.required],
      analysisStartDate: ['', Validators.required],
      analysisEndDate: ['', Validators.required],
      useAdvancedRecommendations: [false], // NEW: form control for toggle
    });
  }

  ngOnInit(): void {
    // Set default date range (last 30 days)
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(endDate.getDate() - 30);

    this.recommendationForm.patchValue({
      analysisStartDate: startDate,
      analysisEndDate: endDate,
    });
  }

  onGenerateRecommendations(): void {
    if (this.recommendationForm.valid) {
      this.isLoading = true;

      const request: ThresholdRecommendationRequestDto = {
        detectionLogicId: this.recommendationForm.value.detectionLogicId,
        analysisStartDate: this.recommendationForm.value.analysisStartDate,
        analysisEndDate: this.recommendationForm.value.analysisEndDate,
      };

      // Use ML-based advanced recommendations or standard recommendations
      const serviceCall = this.recommendationForm.value.useAdvancedRecommendations
        ? this.anomalyAnalysisService.getAdvancedThresholdRecommendations(request)
        : this.anomalyAnalysisService.getThresholdRecommendations(request);

      serviceCall.subscribe({
        next: result => {
          this.recommendationResult = result;
          this.updateMetricsChart();
          this.isLoading = false;
        },
        error: error => {
          console.error('Recommendation generation failed:', error);
          this.isLoading = false;
        },
      });
    }
  }

  private updateMetricsChart(): void {
    if (!this.recommendationResult) return;

    const current = this.recommendationResult.currentMetrics;
    const predicted = this.recommendationResult.predictedMetrics;

    this.metricsComparisonChartData = {
      labels: ['Detection Rate', 'Precision', 'Recall', 'F1 Score', 'Specificity'],
      datasets: [
        {
          label: 'Current Metrics',
          data: [
            current.detectionRate,
            current.precision,
            current.recall,
            current.f1Score,
            1 - current.falsePositiveRate, // Specificity approximation
          ],
          borderColor: '#ff6384',
          backgroundColor: 'rgba(255, 99, 132, 0.2)',
          pointBackgroundColor: '#ff6384',
        },
        {
          label: 'Predicted Metrics',
          data: [
            predicted.detectionRate,
            predicted.precision,
            predicted.recall,
            predicted.f1Score,
            1 - predicted.falsePositiveRate, // Specificity approximation
          ],
          borderColor: '#36a2eb',
          backgroundColor: 'rgba(54, 162, 235, 0.2)',
          pointBackgroundColor: '#36a2eb',
        },
      ],
    };
  }

  getPriorityColor(priority: number): string {
    if (priority >= 0.8) return 'warn';
    if (priority >= 0.6) return 'accent';
    return 'primary';
  }

  getPriorityLabel(priority: number): string {
    if (priority >= 0.8) return 'High';
    if (priority >= 0.6) return 'Medium';
    return 'Low';
  }

  getConfidenceColor(confidence: number): string {
    if (confidence >= 0.8) return 'success';
    if (confidence >= 0.6) return 'warning';
    return 'danger';
  }

  getImprovementColor(improvement: number): string {
    if (improvement > 0.1) return 'success';
    if (improvement > 0.05) return 'warning';
    if (improvement > 0) return 'primary';
    return 'warn';
  }

  formatPercentage(value: number): string {
    return `${(value * 100).toFixed(2)}%`;
  }

  formatImprovement(improvement: number): string {
    const sign = improvement >= 0 ? '+' : '';
    return `${sign}${this.formatPercentage(improvement)}`;
  }

  getMetricValue(metrics: any, metricName: string): number {
    switch (metricName) {
      case 'Detection Rate':
        return metrics.detectionRate;
      case 'False Positive Rate':
        return metrics.falsePositiveRate;
      case 'False Negative Rate':
        return metrics.falseNegativeRate;
      case 'Precision':
        return metrics.precision;
      case 'Recall':
        return metrics.recall;
      case 'F1 Score':
        return metrics.f1Score;
      case 'Avg Detection Time':
        return metrics.averageDetectionTimeMs;
      default:
        return 0;
    }
  }

  getMetricImprovement(metricName: string): number {
    if (!this.recommendationResult) return 0;

    const current = this.getMetricValue(this.recommendationResult.currentMetrics, metricName);
    const predicted = this.getMetricValue(this.recommendationResult.predictedMetrics, metricName);

    // For rates that should be lower (false positive/negative), improvement is negative change
    if (metricName.includes('False') || metricName.includes('Time')) {
      return current - predicted;
    }

    return predicted - current;
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
}
