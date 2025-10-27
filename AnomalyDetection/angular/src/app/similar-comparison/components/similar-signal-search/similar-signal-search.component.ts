import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatSliderModule } from '@angular/material/slider';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SimilarComparisonService } from '../../services/similar-comparison.service';
import { SimilaritySearchCriteria, SimilarSignalResult } from '../../models/similar-comparison.models';

@Component({
  selector: 'app-similar-signal-search',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatSelectModule,
    MatSliderModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './similar-signal-search.component.html',
  styleUrls: ['./similar-signal-search.component.scss']
})
export class SimilarSignalSearchComponent implements OnInit {
  searchForm: FormGroup;
  searchResults: SimilarSignalResult[] = [];
  loading = false;
  displayedColumns: string[] = ['signalName', 'canId', 'systemType', 'similarityScore', 'matchedAttributes', 'oemCode', 'actions'];

  systemTypes = [
    { value: 1, label: 'Engine' },
    { value: 2, label: 'Brake' },
    { value: 3, label: 'Steering' },
    { value: 4, label: 'Transmission' },
    { value: 5, label: 'Body' },
    { value: 6, label: 'Chassis' },
    { value: 7, label: 'HVAC' },
    { value: 8, label: 'Lighting' },
    { value: 9, label: 'Infotainment' },
    { value: 10, label: 'Safety' },
    { value: 11, label: 'Powertrain' },
    { value: 12, label: 'Gateway' }
  ];

  constructor(
    private fb: FormBuilder,
    private similarComparisonService: SimilarComparisonService
  ) {
    this.searchForm = this.fb.group({
      targetSignalName: [''],
      targetCanId: [''],
      targetSystemType: [null],
      compareCanId: [true],
      compareSignalName: [true],
      compareSystemType: [true],
      comparePhysicalRange: [false],
      minimumSimilarity: [0.7, [Validators.min(0), Validators.max(1)]],
      maxResults: [20, [Validators.min(1), Validators.max(100)]]
    });
  }

  ngOnInit(): void {}

  onSearch(): void {
    if (this.searchForm.invalid) {
      return;
    }

    this.loading = true;
    const formValue = this.searchForm.value;

    const criteria: SimilaritySearchCriteria = {
      compareCanId: formValue.compareCanId,
      compareSignalName: formValue.compareSignalName,
      compareSystemType: formValue.compareSystemType,
      comparePhysicalRange: formValue.comparePhysicalRange,
      minimumSimilarity: formValue.minimumSimilarity,
      maxResults: formValue.maxResults,
      targetSignalName: formValue.targetSignalName || undefined,
      targetCanSignalId: formValue.targetCanId || undefined,
      targetSystemType: formValue.targetSystemType || undefined
    };

    this.similarComparisonService.searchSimilarSignals(criteria).subscribe({
      next: (results) => {
        this.searchResults = results;
        this.loading = false;
      },
      error: (error) => {
        console.error('Search failed:', error);
        this.loading = false;
      }
    });
  }

  onReset(): void {
    this.searchForm.reset({
      compareCanId: true,
      compareSignalName: true,
      compareSystemType: true,
      comparePhysicalRange: false,
      minimumSimilarity: 0.7,
      maxResults: 20
    });
    this.searchResults = [];
  }

  getSimilarityColor(score: number): string {
    if (score >= 0.9) return 'success';
    if (score >= 0.7) return 'primary';
    if (score >= 0.5) return 'accent';
    return 'warn';
  }

  formatSimilarityScore(score: number): string {
    return `${(score * 100).toFixed(1)}%`;
  }

  getSystemTypeName(systemType: number): string {
    const type = this.systemTypes.find(t => t.value === systemType);
    return type ? type.label : 'Unknown';
  }
}
