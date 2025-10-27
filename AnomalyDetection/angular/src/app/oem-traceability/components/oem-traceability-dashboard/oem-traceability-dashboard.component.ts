import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { OemTraceabilityService } from '../../services/oem-traceability.service';
import {
  OemTraceabilityResult,
  OemUsageInfo,
  OemParameterDifference,
  UsagePatternDifference
} from '../../models/oem-traceability.models';

@Component({
  selector: 'app-oem-traceability-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatTabsModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './oem-traceability-dashboard.component.html',
  styleUrls: ['./oem-traceability-dashboard.component.scss']
})
export class OemTraceabilityDashboardComponent implements OnInit {
  entityId = '';
  entityType = 'CanSignal';
  entityTypes = ['CanSignal', 'DetectionLogic', 'Project'];
  
  traceabilityResult: OemTraceabilityResult | null = null;
  loading = false;
  
  // Table columns
  oemUsageColumns = ['oemCode', 'usageCount', 'vehicles', 'customizations', 'approvals'];
  parameterDiffColumns = ['oemCode', 'parameterName', 'originalValue', 'customValue', 'difference'];
  patternDiffColumns = ['oemCode', 'patternType', 'description', 'frequency', 'impact'];

  constructor(
    private oemTraceabilityService: OemTraceabilityService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    // Initialize component
  }

  searchTraceability(): void {
    if (!this.entityId || !this.entityType) {
      this.snackBar.open('エンティティIDとタイプを入力してください', '閉じる', { duration: 3000 });
      return;
    }

    this.loading = true;
    this.oemTraceabilityService.getOemTraceability(this.entityId, this.entityType)
      .subscribe({
        next: (result) => {
          this.traceabilityResult = result;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error fetching traceability:', error);
          this.snackBar.open('トレーサビリティの取得に失敗しました', '閉じる', { duration: 3000 });
          this.loading = false;
        }
      });
  }

  getParameterDifferences(): OemParameterDifference[] {
    if (!this.traceabilityResult?.crossOemDifferences.parameterDifferences) {
      return [];
    }

    const allDifferences: OemParameterDifference[] = [];
    Object.values(this.traceabilityResult.crossOemDifferences.parameterDifferences)
      .forEach(differences => allDifferences.push(...differences));
    
    return allDifferences;
  }

  getUsagePatternDifferences(): UsagePatternDifference[] {
    return this.traceabilityResult?.crossOemDifferences.usagePatternDifferences || [];
  }

  getRecommendations(): string[] {
    return this.traceabilityResult?.crossOemDifferences.recommendations || [];
  }

  getImpactColor(impact: string): string {
    switch (impact.toLowerCase()) {
      case 'high': return 'warn';
      case 'medium': return 'accent';
      case 'low': return 'primary';
      default: return '';
    }
  }

  formatDifferencePercentage(percentage: number): string {
    return `${percentage.toFixed(1)}%`;
  }

  formatVehicleList(vehicles: string[]): string {
    if (vehicles.length <= 3) {
      return vehicles.join(', ');
    }
    return `${vehicles.slice(0, 3).join(', ')} (+${vehicles.length - 3}件)`;
  }
}