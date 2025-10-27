import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { SimilarComparisonService } from '../../services/similar-comparison.service';
import { ComparisonVisualizationData } from '../../models/similar-comparison.models';

@Component({
  selector: 'app-data-visualization',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule
  ],
  templateUrl: './data-visualization.component.html',
  styleUrls: ['./data-visualization.component.scss']
})
export class DataVisualizationComponent implements OnInit {
  visualizationData: ComparisonVisualizationData | null = null;
  loading = false;
  sourceSignalId: string = '';
  targetSignalId: string = '';

  constructor(
    private route: ActivatedRoute,
    private similarComparisonService: SimilarComparisonService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.sourceSignalId = params['source'];
      this.targetSignalId = params['target'];
      
      if (this.sourceSignalId && this.targetSignalId) {
        this.loadVisualizationData();
      }
    });
  }

  loadVisualizationData(): void {
    this.loading = true;
    this.similarComparisonService.getVisualizationData(this.sourceSignalId, this.targetSignalId).subscribe({
      next: (data) => {
        this.visualizationData = data;
        this.loading = false;
        this.renderCharts();
      },
      error: (error) => {
        console.error('Failed to load visualization data:', error);
        this.loading = false;
      }
    });
  }

  renderCharts(): void {
    if (!this.visualizationData) return;

    // Render time series chart
    this.renderTimeSeriesChart();
    
    // Render distribution chart
    this.renderDistributionChart();
    
    // Render correlation chart
    this.renderCorrelationChart();
  }

  renderTimeSeriesChart(): void {
    // Implementation would use a charting library like Chart.js or D3.js
    // For now, this is a placeholder
    console.log('Rendering time series chart with data:', this.visualizationData?.timeSeriesData);
  }

  renderDistributionChart(): void {
    // Implementation would use a charting library
    console.log('Rendering distribution chart with data:', this.visualizationData?.distributionData);
  }

  renderCorrelationChart(): void {
    // Implementation would use a charting library
    console.log('Rendering correlation chart with data:', this.visualizationData?.correlationData);
  }
}
