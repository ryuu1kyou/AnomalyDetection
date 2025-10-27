import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { SimilarComparisonService } from '../../services/similar-comparison.service';
import {
  TestDataComparison,
  ThresholdDifference,
  ComparisonRecommendation,
  DifferenceType,
  ImpactLevel,
  RecommendationType,
  RecommendationPriority
} from '../../models/similar-comparison.models';

@Component({
  selector: 'app-comparison-analysis',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTabsModule,
    MatExpansionModule
  ],
  templateUrl: './comparison-analysis.component.html',
  styleUrls: ['./comparison-analysis.component.scss']
})
export class ComparisonAnalysisComponent implements OnInit {
  comparison: TestDataComparison | null = null;
  loading = false;
  sourceSignalId: string = '';
  targetSignalId: string = '';

  thresholdColumns: string[] = ['parameterName', 'sourceValue', 'targetValue', 'difference', 'differencePercentage', 'impactLevel'];
  recommendationColumns: string[] = ['type', 'priority', 'description', 'suggestedValue'];

  constructor(
    private route: ActivatedRoute,
    private similarComparisonService: SimilarComparisonService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.sourceSignalId = params['source'];
      this.targetSignalId = params['target'];
      
      if (this.sourceSignalId && this.targetSignalId) {
        this.loadComparison();
      }
    });
  }

  loadComparison(): void {
    this.loading = true;
    this.similarComparisonService.compareTestData(this.sourceSignalId, this.targetSignalId).subscribe({
      next: (comparison) => {
        this.comparison = comparison;
        this.loading = false;
      },
      error: (error) => {
        console.error('Failed to load comparison:', error);
        this.loading = false;
      }
    });
  }

  getImpactLevelColor(level: ImpactLevel): string {
    switch (level) {
      case ImpactLevel.Low: return 'success';
      case ImpactLevel.Medium: return 'primary';
      case ImpactLevel.High: return 'accent';
      case ImpactLevel.Critical: return 'warn';
      default: return 'primary';
    }
  }

  getImpactLevelText(level: ImpactLevel): string {
    switch (level) {
      case ImpactLevel.Low: return '低 / Low';
      case ImpactLevel.Medium: return '中 / Medium';
      case ImpactLevel.High: return '高 / High';
      case ImpactLevel.Critical: return '重大 / Critical';
      default: return 'Unknown';
    }
  }

  getPriorityColor(priority: RecommendationPriority): string {
    switch (priority) {
      case RecommendationPriority.Low: return 'success';
      case RecommendationPriority.Medium: return 'primary';
      case RecommendationPriority.High: return 'accent';
      case RecommendationPriority.Critical: return 'warn';
      default: return 'primary';
    }
  }

  getPriorityText(priority: RecommendationPriority): string {
    switch (priority) {
      case RecommendationPriority.Low: return '低 / Low';
      case RecommendationPriority.Medium: return '中 / Medium';
      case RecommendationPriority.High: return '高 / High';
      case RecommendationPriority.Critical: return '重大 / Critical';
      default: return 'Unknown';
    }
  }

  getRecommendationTypeText(type: RecommendationType): string {
    switch (type) {
      case RecommendationType.AdjustThreshold: return '閾値調整 / Adjust Threshold';
      case RecommendationType.ModifyCondition: return '条件変更 / Modify Condition';
      case RecommendationType.ReviewResult: return '結果確認 / Review Result';
      case RecommendationType.UseAsIs: return 'そのまま使用 / Use As Is';
      case RecommendationType.RequireValidation: return '検証必要 / Require Validation';
      default: return 'Unknown';
    }
  }

  formatPercentage(value: number): string {
    return `${value.toFixed(1)}%`;
  }

  formatSimilarity(value: number): string {
    return `${(value * 100).toFixed(1)}%`;
  }

  onExport(format: 'csv' | 'excel' | 'pdf'): void {
    if (!this.comparison) return;

    const exportFormat = {
      format,
      includeCharts: true,
      includeRecommendations: true
    };

    this.similarComparisonService.exportComparisonResult(this.comparison, exportFormat).subscribe({
      next: (blob) => {
        const filename = `comparison_${this.sourceSignalId}_${this.targetSignalId}.${format}`;
        this.similarComparisonService.downloadExport(blob, filename);
      },
      error: (error) => {
        console.error('Export failed:', error);
      }
    });
  }
}
