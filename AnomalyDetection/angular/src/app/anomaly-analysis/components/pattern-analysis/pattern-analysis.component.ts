import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
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
import { AnomalyPatternAnalysisDto, AnomalyAnalysisRequestDto } from '../../models/anomaly-analysis.models';

@Component({
  selector: 'app-pattern-analysis',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
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
  templateUrl: './pattern-analysis.component.html',
  styleUrls: ['./pattern-analysis.component.scss']
})
export class PatternAnalysisComponent implements OnInit {
  analysisForm: FormGroup;
  isLoading = false;
  analysisResult: AnomalyPatternAnalysisDto | null = null;

  // Chart configurations
  public pieChartType: ChartType = 'pie';
  public pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    plugins: {
      legend: {
        display: true,
        position: 'top',
      }
    }
  };

  public barChartType: ChartType = 'bar';
  public barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    scales: {
      y: {
        beginAtZero: true
      }
    }
  };

  // Chart data
  public anomalyTypeChartData: ChartData<'pie'> = {
    labels: [],
    datasets: []
  };

  public anomalyLevelChartData: ChartData<'pie'> = {
    labels: [],
    datasets: []
  };

  public frequencyPatternChartData: ChartData<'bar'> = {
    labels: [],
    datasets: []
  };

  // Table columns
  displayedColumnsPatterns = ['patternName', 'timeInterval', 'frequency', 'confidence'];
  displayedColumnsCorrelations = ['relatedSignalName', 'correlationCoefficient', 'correlationType'];

  constructor(
    private fb: FormBuilder,
    private anomalyAnalysisService: AnomalyAnalysisService
  ) {
    this.analysisForm = this.fb.group({
      canSignalId: ['', Validators.required],
      analysisStartDate: ['', Validators.required],
      analysisEndDate: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    // Set default date range (last 30 days)
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(endDate.getDate() - 30);

    this.analysisForm.patchValue({
      analysisStartDate: startDate,
      analysisEndDate: endDate
    });
  }

  onAnalyze(): void {
    if (this.analysisForm.valid) {
      this.isLoading = true;
      
      const request: AnomalyAnalysisRequestDto = {
        canSignalId: this.analysisForm.value.canSignalId,
        analysisStartDate: this.analysisForm.value.analysisStartDate,
        analysisEndDate: this.analysisForm.value.analysisEndDate
      };

      this.anomalyAnalysisService.analyzeAnomalyPattern(request).subscribe({
        next: (result) => {
          this.analysisResult = result;
          this.updateCharts();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Analysis failed:', error);
          this.isLoading = false;
        }
      });
    }
  }

  private updateCharts(): void {
    if (!this.analysisResult) return;

    // Update anomaly type chart
    const typeLabels = Object.keys(this.analysisResult.anomalyTypeDistribution);
    const typeData = Object.values(this.analysisResult.anomalyTypeDistribution);
    
    this.anomalyTypeChartData = {
      labels: typeLabels,
      datasets: [{
        data: typeData,
        backgroundColor: [
          '#FF6384',
          '#36A2EB',
          '#FFCE56',
          '#4BC0C0',
          '#9966FF',
          '#FF9F40'
        ]
      }]
    };

    // Update anomaly level chart
    const levelLabels = Object.keys(this.analysisResult.anomalyLevelDistribution);
    const levelData = Object.values(this.analysisResult.anomalyLevelDistribution);
    
    this.anomalyLevelChartData = {
      labels: levelLabels,
      datasets: [{
        data: levelData,
        backgroundColor: [
          '#28a745',
          '#ffc107',
          '#fd7e14',
          '#dc3545',
          '#6f42c1'
        ]
      }]
    };

    // Update frequency pattern chart
    const patternLabels = this.analysisResult.frequencyPatterns.map(p => p.patternName);
    const patternFrequencies = this.analysisResult.frequencyPatterns.map(p => p.frequency);
    
    this.frequencyPatternChartData = {
      labels: patternLabels,
      datasets: [{
        label: 'Frequency',
        data: patternFrequencies,
        backgroundColor: '#36A2EB'
      }]
    };
  }

  getConfidenceColor(confidence: number): string {
    if (confidence >= 0.8) return 'success';
    if (confidence >= 0.6) return 'warning';
    return 'danger';
  }

  getCorrelationStrength(coefficient: number): string {
    const abs = Math.abs(coefficient);
    if (abs >= 0.8) return 'Very Strong';
    if (abs >= 0.6) return 'Strong';
    if (abs >= 0.4) return 'Moderate';
    if (abs >= 0.2) return 'Weak';
    return 'Very Weak';
  }

  getCorrelationColor(coefficient: number): string {
    const abs = Math.abs(coefficient);
    if (abs >= 0.8) return 'success';
    if (abs >= 0.6) return 'primary';
    if (abs >= 0.4) return 'warning';
    return 'secondary';
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

  formatPercentage(value: number): string {
    return `${(value * 100).toFixed(2)}%`;
  }
}