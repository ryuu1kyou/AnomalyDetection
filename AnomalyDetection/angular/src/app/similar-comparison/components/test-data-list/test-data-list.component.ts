import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { SimilarComparisonService } from '../../services/similar-comparison.service';
import { TestDataRecord, TestDataFilter } from '../../models/similar-comparison.models';

@Component({
  selector: 'app-test-data-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './test-data-list.component.html',
  styleUrls: ['./test-data-list.component.scss']
})
export class TestDataListComponent implements OnInit {
  filterForm: FormGroup;
  testDataRecords: TestDataRecord[] = [];
  filteredRecords: TestDataRecord[] = [];
  displayedRecords: TestDataRecord[] = [];
  loading = false;
  
  displayedColumns: string[] = [
    'canSignalName',
    'vehiclePhase',
    'oemCode',
    'testDate',
    'detectionLogicName',
    'anomalyCount',
    'detectionRate',
    'actions'
  ];

  // Pagination
  pageSize = 10;
  pageIndex = 0;
  totalRecords = 0;

  // Sort
  sortColumn = 'testDate';
  sortDirection: 'asc' | 'desc' = 'desc';

  anomalyTypes = [
    'Timeout',
    'OutOfRange',
    'RateOfChange',
    'Stuck',
    'Periodic',
    'Missing'
  ];

  constructor(
    private fb: FormBuilder,
    private similarComparisonService: SimilarComparisonService
  ) {
    this.filterForm = this.fb.group({
      canSignalId: [''],
      vehiclePhase: [''],
      oemCode: [''],
      startDate: [null],
      endDate: [null],
      anomalyType: [''],
      minDetectionRate: [null],
      maxDetectionRate: [null]
    });
  }

  ngOnInit(): void {
    this.loadTestData();
  }

  loadTestData(): void {
    this.loading = true;
    const filter: TestDataFilter = this.filterForm.value;

    this.similarComparisonService.getTestDataRecords(filter).subscribe({
      next: (records) => {
        this.testDataRecords = records;
        this.applyFilterAndSort();
        this.loading = false;
      },
      error: (error) => {
        console.error('Failed to load test data:', error);
        this.loading = false;
      }
    });
  }

  onFilter(): void {
    this.pageIndex = 0;
    this.loadTestData();
  }

  onReset(): void {
    this.filterForm.reset();
    this.pageIndex = 0;
    this.loadTestData();
  }

  onSort(sort: Sort): void {
    this.sortColumn = sort.active;
    this.sortDirection = sort.direction as 'asc' | 'desc';
    this.applyFilterAndSort();
  }

  onPageChange(event: PageEvent): void {
    this.pageSize = event.pageSize;
    this.pageIndex = event.pageIndex;
    this.updateDisplayedRecords();
  }

  applyFilterAndSort(): void {
    this.filteredRecords = [...this.testDataRecords];

    // Sort
    this.filteredRecords.sort((a, b) => {
      let comparison = 0;
      
      switch (this.sortColumn) {
        case 'canSignalName':
          comparison = a.canSignalName.localeCompare(b.canSignalName);
          break;
        case 'vehiclePhase':
          comparison = a.vehiclePhase.localeCompare(b.vehiclePhase);
          break;
        case 'oemCode':
          comparison = a.oemCode.localeCompare(b.oemCode);
          break;
        case 'testDate':
          comparison = new Date(a.testDate).getTime() - new Date(b.testDate).getTime();
          break;
        case 'anomalyCount':
          comparison = a.anomalyCount - b.anomalyCount;
          break;
        case 'detectionRate':
          comparison = a.detectionRate - b.detectionRate;
          break;
      }

      return this.sortDirection === 'asc' ? comparison : -comparison;
    });

    this.totalRecords = this.filteredRecords.length;
    this.updateDisplayedRecords();
  }

  updateDisplayedRecords(): void {
    const startIndex = this.pageIndex * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.displayedRecords = this.filteredRecords.slice(startIndex, endIndex);
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('ja-JP');
  }

  formatDetectionRate(rate: number): string {
    return `${(rate * 100).toFixed(1)}%`;
  }

  getDetectionRateColor(rate: number): string {
    if (rate >= 0.9) return 'success';
    if (rate >= 0.7) return 'primary';
    if (rate >= 0.5) return 'accent';
    return 'warn';
  }
}
