import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import { SelectionModel } from '@angular/cdk/collections';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

import { ProjectService } from '../services/project.service';
import {
  AnomalyDetectionProject,
  GetProjectsInput,
  ProjectStatus,
  ProjectPriority,
} from '../models/project.model';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatCardModule,
    MatProgressBarModule,
    MatTooltipModule,
    MatMenuModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatExpansionModule,
  ],
  template: `
    <div class="project-list-container">
      <!-- Header -->
      <div class="header">
        <div class="header-info">
          <h2>プロジェクト管理</h2>
          <p class="header-description">異常検出プロジェクトの作成、管理、進捗追跡を行います</p>
        </div>
        <div class="header-actions">
          <button mat-raised-button color="primary" (click)="createProject()">
            <mat-icon>add</mat-icon>
            新規プロジェクト
          </button>
          <button mat-raised-button (click)="refreshData()">
            <mat-icon>refresh</mat-icon>
            更新
          </button>
          <button mat-button (click)="exportProjects()">
            <mat-icon>download</mat-icon>
            エクスポート
          </button>
        </div>
      </div>

      <!-- Statistics Cards -->
      <div class="stats-cards">
        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <div class="stat-icon">
                <mat-icon>folder</mat-icon>
              </div>
              <div class="stat-info">
                <div class="stat-value">{{ statistics?.totalProjects || 0 }}</div>
                <div class="stat-label">総プロジェクト数</div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <div class="stat-icon active">
                <mat-icon>play_circle</mat-icon>
              </div>
              <div class="stat-info">
                <div class="stat-value">{{ statistics?.activeProjects || 0 }}</div>
                <div class="stat-label">進行中</div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <div class="stat-icon completed">
                <mat-icon>check_circle</mat-icon>
              </div>
              <div class="stat-info">
                <div class="stat-value">{{ statistics?.completedProjects || 0 }}</div>
                <div class="stat-label">完了</div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <div class="stat-icon delayed">
                <mat-icon>warning</mat-icon>
              </div>
              <div class="stat-info">
                <div class="stat-value">{{ statistics?.delayedProjects || 0 }}</div>
                <div class="stat-label">遅延</div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Filters -->
      <mat-expansion-panel class="filter-panel" [expanded]="showFilters">
        <mat-expansion-panel-header>
          <mat-panel-title>
            <mat-icon>filter_list</mat-icon>
            フィルター
          </mat-panel-title>
        </mat-expansion-panel-header>

        <form [formGroup]="filterForm" class="filter-form">
          <div class="filter-row">
            <mat-form-field appearance="outline">
              <mat-label>検索</mat-label>
              <input matInput formControlName="filter" placeholder="プロジェクト名、コードで検索" />
              <mat-icon matSuffix>search</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>ステータス</mat-label>
              <mat-select formControlName="status">
                <mat-option value="">すべて</mat-option>
                <mat-option [value]="ProjectStatus.Planning">計画中</mat-option>
                <mat-option [value]="ProjectStatus.Active">進行中</mat-option>
                <mat-option [value]="ProjectStatus.OnHold">保留</mat-option>
                <mat-option [value]="ProjectStatus.Completed">完了</mat-option>
                <mat-option [value]="ProjectStatus.Cancelled">キャンセル</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>優先度</mat-label>
              <mat-select formControlName="priority">
                <mat-option value="">すべて</mat-option>
                <mat-option [value]="ProjectPriority.Low">低</mat-option>
                <mat-option [value]="ProjectPriority.Medium">中</mat-option>
                <mat-option [value]="ProjectPriority.High">高</mat-option>
                <mat-option [value]="ProjectPriority.Critical">緊急</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>車両モデル</mat-label>
              <input matInput formControlName="vehicleModel" placeholder="車両モデル名" />
            </mat-form-field>
          </div>

          <div class="filter-row">
            <mat-form-field appearance="outline">
              <mat-label>主要システム</mat-label>
              <input matInput formControlName="primarySystem" placeholder="システム名" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>開始日（開始）</mat-label>
              <input matInput [matDatepicker]="startFromPicker" formControlName="startDateFrom" />
              <mat-datepicker-toggle matSuffix [for]="startFromPicker"></mat-datepicker-toggle>
              <mat-datepicker #startFromPicker></mat-datepicker>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>開始日（終了）</mat-label>
              <input matInput [matDatepicker]="startToPicker" formControlName="startDateTo" />
              <mat-datepicker-toggle matSuffix [for]="startToPicker"></mat-datepicker-toggle>
              <mat-datepicker #startToPicker></mat-datepicker>
            </mat-form-field>
          </div>

          <div class="filter-actions">
            <button mat-button type="button" (click)="clearFilters()">クリア</button>
            <button mat-raised-button color="primary" type="button" (click)="applyFilters()">
              適用
            </button>
          </div>
        </form>
      </mat-expansion-panel>

      <!-- Bulk Actions -->
      <div class="bulk-actions" *ngIf="selection.hasValue()">
        <span>{{ selection.selected.length }} 件選択中</span>
        <button mat-button (click)="bulkStart()">
          <mat-icon>play_arrow</mat-icon>
          開始
        </button>
        <button mat-button (click)="bulkPause()">
          <mat-icon>pause</mat-icon>
          一時停止
        </button>
        <button mat-button (click)="bulkComplete()">
          <mat-icon>check</mat-icon>
          完了
        </button>
        <button mat-button color="warn" (click)="bulkDelete()">
          <mat-icon>delete</mat-icon>
          削除
        </button>
      </div>

      <!-- Projects Table -->
      <div class="table-container">
        <mat-table [dataSource]="dataSource" matSort class="projects-table">
          <!-- Checkbox Column -->
          <ng-container matColumnDef="select">
            <mat-header-cell *matHeaderCellDef>
              <mat-checkbox
                (change)="$event ? masterToggle() : null"
                [checked]="selection.hasValue() && isAllSelected()"
                [indeterminate]="selection.hasValue() && !isAllSelected()"
              >
              </mat-checkbox>
            </mat-header-cell>
            <mat-cell *matCellDef="let row">
              <mat-checkbox
                (click)="$event.stopPropagation()"
                (change)="$event ? selection.toggle(row) : null"
                [checked]="selection.isSelected(row)"
              >
              </mat-checkbox>
            </mat-cell>
          </ng-container>

          <!-- Project Code Column -->
          <ng-container matColumnDef="projectCode">
            <mat-header-cell *matHeaderCellDef mat-sort-header>プロジェクトコード</mat-header-cell>
            <mat-cell *matCellDef="let project">
              <div class="project-code">{{ project.projectCode }}</div>
            </mat-cell>
          </ng-container>

          <!-- Project Name Column -->
          <ng-container matColumnDef="projectName">
            <mat-header-cell *matHeaderCellDef mat-sort-header>プロジェクト名</mat-header-cell>
            <mat-cell *matCellDef="let project">
              <div class="project-info">
                <div class="project-name">{{ project.projectName }}</div>
                <div class="vehicle-info">{{ project.vehicleModel }} ({{ project.modelYear }})</div>
              </div>
            </mat-cell>
          </ng-container>

          <!-- Status Column -->
          <ng-container matColumnDef="status">
            <mat-header-cell *matHeaderCellDef mat-sort-header>ステータス</mat-header-cell>
            <mat-cell *matCellDef="let project">
              <mat-chip [class]="'status-' + getStatusClass(project.status)">
                {{ getStatusText(project.status) }}
              </mat-chip>
            </mat-cell>
          </ng-container>

          <!-- Priority Column -->
          <ng-container matColumnDef="priority">
            <mat-header-cell *matHeaderCellDef mat-sort-header>優先度</mat-header-cell>
            <mat-cell *matCellDef="let project">
              <mat-chip [class]="'priority-' + getPriorityClass(project.priority)">
                {{ getPriorityText(project.priority) }}
              </mat-chip>
            </mat-cell>
          </ng-container>

          <!-- Progress Column -->
          <ng-container matColumnDef="progress">
            <mat-header-cell *matHeaderCellDef mat-sort-header>進捗</mat-header-cell>
            <mat-cell *matCellDef="let project">
              <div class="progress-container">
                <mat-progress-bar
                  mode="determinate"
                  [value]="project.progressPercentage"
                  [class]="'progress-' + getProgressClass(project.progressPercentage)"
                >
                </mat-progress-bar>
                <span class="progress-text">{{ project.progressPercentage }}%</span>
              </div>
            </mat-cell>
          </ng-container>

          <!-- Primary System Column -->
          <ng-container matColumnDef="primarySystem">
            <mat-header-cell *matHeaderCellDef mat-sort-header>主要システム</mat-header-cell>
            <mat-cell *matCellDef="let project">{{ project.primarySystem }}</mat-cell>
          </ng-container>

          <!-- Start Date Column -->
          <ng-container matColumnDef="startDate">
            <mat-header-cell *matHeaderCellDef mat-sort-header>開始日</mat-header-cell>
            <mat-cell *matCellDef="let project">
              {{ project.startDate | date : 'yyyy/MM/dd' }}
            </mat-cell>
          </ng-container>

          <!-- End Date Column -->
          <ng-container matColumnDef="plannedEndDate">
            <mat-header-cell *matHeaderCellDef mat-sort-header>予定終了日</mat-header-cell>
            <mat-cell *matCellDef="let project">
              <div class="end-date" [class.overdue]="isOverdue(project)">
                {{ project.plannedEndDate | date : 'yyyy/MM/dd' }}
                <mat-icon *ngIf="isOverdue(project)" class="overdue-icon" matTooltip="期限超過"
                  >warning</mat-icon
                >
              </div>
            </mat-cell>
          </ng-container>

          <!-- OEM Column -->
          <ng-container matColumnDef="oemName">
            <mat-header-cell *matHeaderCellDef mat-sort-header>OEM</mat-header-cell>
            <mat-cell *matCellDef="let project">{{ project.oemName }}</mat-cell>
          </ng-container>

          <!-- Actions Column -->
          <ng-container matColumnDef="actions">
            <mat-header-cell *matHeaderCellDef>操作</mat-header-cell>
            <mat-cell *matCellDef="let project">
              <button mat-icon-button [matMenuTriggerFor]="actionMenu">
                <mat-icon>more_vert</mat-icon>
              </button>
              <mat-menu #actionMenu="matMenu">
                <button mat-menu-item (click)="viewProject(project)">
                  <mat-icon>visibility</mat-icon>
                  詳細表示
                </button>
                <button mat-menu-item (click)="editProject(project)">
                  <mat-icon>edit</mat-icon>
                  編集
                </button>
                <button mat-menu-item (click)="manageMembers(project)">
                  <mat-icon>people</mat-icon>
                  メンバー管理
                </button>
                <button mat-menu-item (click)="manageMilestones(project)">
                  <mat-icon>flag</mat-icon>
                  マイルストーン
                </button>
                <mat-divider></mat-divider>
                <button
                  mat-menu-item
                  (click)="startProject(project)"
                  *ngIf="project.status === ProjectStatus.Planning"
                >
                  <mat-icon>play_arrow</mat-icon>
                  開始
                </button>
                <button
                  mat-menu-item
                  (click)="pauseProject(project)"
                  *ngIf="project.status === ProjectStatus.Active"
                >
                  <mat-icon>pause</mat-icon>
                  一時停止
                </button>
                <button
                  mat-menu-item
                  (click)="completeProject(project)"
                  *ngIf="project.status === ProjectStatus.Active"
                >
                  <mat-icon>check</mat-icon>
                  完了
                </button>
                <button mat-menu-item (click)="generateReport(project)">
                  <mat-icon>assessment</mat-icon>
                  レポート生成
                </button>
                <mat-divider></mat-divider>
                <button mat-menu-item (click)="deleteProject(project)" color="warn">
                  <mat-icon>delete</mat-icon>
                  削除
                </button>
              </mat-menu>
            </mat-cell>
          </ng-container>

          <mat-header-row *matHeaderRowDef="displayedColumns"></mat-header-row>
          <mat-row
            *matRowDef="let row; columns: displayedColumns"
            (click)="viewProject(row)"
            class="clickable-row"
          ></mat-row>
        </mat-table>

        <!-- Loading Spinner -->
        <div class="loading-container" *ngIf="loading">
          <mat-spinner diameter="50"></mat-spinner>
        </div>

        <!-- No Data -->
        <div class="no-data" *ngIf="!loading && dataSource.data.length === 0">
          <mat-icon>folder_off</mat-icon>
          <p>プロジェクトが見つかりませんでした</p>
          <button mat-raised-button color="primary" (click)="createProject()">
            新規プロジェクト作成
          </button>
        </div>
      </div>

      <!-- Paginator -->
      <mat-paginator
        [length]="totalCount"
        [pageSize]="pageSize"
        [pageSizeOptions]="[10, 25, 50, 100]"
        (page)="onPageChange($event)"
        showFirstLastButtons
      >
      </mat-paginator>
    </div>
  `,
  styleUrls: ['./project-list.component.scss'],
})
export class ProjectListComponent implements OnInit, OnDestroy {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  // Enums for template
  ProjectStatus = ProjectStatus;
  ProjectPriority = ProjectPriority;

  // Data
  dataSource = new MatTableDataSource<AnomalyDetectionProject>([]);
  selection = new SelectionModel<AnomalyDetectionProject>(true, []);
  statistics: any = {};
  selectedForStats?: AnomalyDetectionProject; // project whose stats we show
  totalCount = 0;
  pageSize = 25;
  currentPage = 0;
  loading = false;

  // Filters
  filterForm: FormGroup;
  showFilters = true;

  // Columns
  displayedColumns: string[] = [
    'select',
    'projectCode',
    'projectName',
    'status',
    'priority',
    'progress',
    'primarySystem',
    'startDate',
    'plannedEndDate',
    'oemName',
    'actions',
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private projectService: ProjectService,
    private fb: FormBuilder,
    private router: Router,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.filterForm = this.createFilterForm();
  }

  ngOnInit(): void {
    this.setupFilterSubscription();
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createFilterForm(): FormGroup {
    return this.fb.group({
      filter: [''],
      status: [''],
      priority: [''],
      vehicleModel: [''],
      primarySystem: [''],
      startDateFrom: [null],
      startDateTo: [null],
    });
  }

  private setupFilterSubscription(): void {
    this.filterForm.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 0;
        this.loadData();
      });
  }

  loadData(): void {
    this.loading = true;

    const input: GetProjectsInput = {
      ...this.filterForm.value,
      skipCount: this.currentPage * this.pageSize,
      maxResultCount: this.pageSize,
      sorting: this.sort?.active
        ? `${this.sort.active} ${this.sort.direction}`
        : 'creationTime desc',
    };

    this.projectService
      .getList(input)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.dataSource.data = result.items;
          this.totalCount = result.totalCount;
          this.computeAggregateStatistics();
          this.loading = false;
          this.selection.clear();
        },
        error: error => {
          console.error('Error loading projects:', error);
          this.snackBar.open('データの読み込みに失敗しました', '閉じる', { duration: 3000 });
          this.loading = false;
        },
      });
  }

  loadStatistics(): void {
    if (!this.selectedForStats) {
      this.statistics = {};
      return;
    }
    this.projectService
      .getStatistics(this.selectedForStats.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: stats => (this.statistics = stats),
        error: error => console.error('Error loading statistics:', error),
      });
  }

  refreshData(): void {
    this.loadData();
    this.loadStatistics();
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadData();
  }

  applyFilters(): void {
    this.currentPage = 0;
    this.loadData();
  }

  clearFilters(): void {
    this.filterForm.reset();
    this.currentPage = 0;
    this.loadData();
  }

  // Selection methods
  isAllSelected(): boolean {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  masterToggle(): void {
    this.isAllSelected()
      ? this.selection.clear()
      : this.dataSource.data.forEach(row => this.selection.select(row));
  }

  // Navigation methods
  createProject(): void {
    this.router.navigate(['/projects/create']);
  }

  viewProject(project: AnomalyDetectionProject): void {
    this.router.navigate(['/projects', project.id]);
  }

  // When user clicks a row we may also want to load its statistics without navigating away
  selectForStatistics(project: AnomalyDetectionProject): void {
    this.selectedForStats = project;
    this.loadStatistics();
  }

  private computeAggregateStatistics(): void {
    const list = this.dataSource.data;
    if (!list || list.length === 0) {
      this.statistics = {
        totalProjects: 0,
        activeProjects: 0,
        completedProjects: 0,
        delayedProjects: 0,
      };
      return;
    }
    const now = new Date();
    const delayed = list.filter(
      p =>
        p.plannedEndDate &&
        new Date(p.plannedEndDate) < now &&
        p.status !== ProjectStatus.Completed &&
        p.status !== ProjectStatus.Cancelled
    );
    this.statistics = {
      totalProjects: list.length,
      activeProjects: list.filter(p => p.status === ProjectStatus.Active).length,
      completedProjects: list.filter(p => p.status === ProjectStatus.Completed).length,
      delayedProjects: delayed.length,
    };
  }

  editProject(project: AnomalyDetectionProject): void {
    this.router.navigate(['/projects', project.id, 'edit']);
  }

  manageMembers(project: AnomalyDetectionProject): void {
    this.router.navigate(['/projects', project.id, 'members']);
  }

  manageMilestones(project: AnomalyDetectionProject): void {
    this.router.navigate(['/projects', project.id, 'milestones']);
  }

  // Project operations
  startProject(project: AnomalyDetectionProject): void {
    this.projectService
      .startProject(project.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('プロジェクトを開始しました', '閉じる', { duration: 2000 });
          this.loadData();
        },
        error: error => {
          console.error('Error starting project:', error);
          this.snackBar.open('プロジェクトの開始に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  pauseProject(project: AnomalyDetectionProject): void {
    // TODO: Open dialog to get reason
    this.projectService
      .pauseProject(project.id, '一時停止')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('プロジェクトを一時停止しました', '閉じる', { duration: 2000 });
          this.loadData();
        },
        error: error => {
          console.error('Error pausing project:', error);
          this.snackBar.open('プロジェクトの一時停止に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  completeProject(project: AnomalyDetectionProject): void {
    // TODO: Open dialog to get completion notes
    this.projectService
      .completeProject(project.id, '完了')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('プロジェクトを完了しました', '閉じる', { duration: 2000 });
          this.loadData();
        },
        error: error => {
          console.error('Error completing project:', error);
          this.snackBar.open('プロジェクトの完了に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  deleteProject(project: AnomalyDetectionProject): void {
    if (confirm(`プロジェクト "${project.projectName}" を削除しますか？`)) {
      this.projectService
        .delete(project.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.snackBar.open('プロジェクトを削除しました', '閉じる', { duration: 2000 });
            this.loadData();
          },
          error: error => {
            console.error('Error deleting project:', error);
            this.snackBar.open('プロジェクトの削除に失敗しました', '閉じる', { duration: 3000 });
          },
        });
    }
  }

  generateReport(project: AnomalyDetectionProject): void {
    this.projectService
      .generateProgressReport(project.id, 'pdf')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: blob => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `project-report-${project.projectCode}-${
            new Date().toISOString().split('T')[0]
          }.pdf`;
          a.click();
          window.URL.revokeObjectURL(url);
        },
        error: error => {
          console.error('Error generating report:', error);
          this.snackBar.open('レポート生成に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  // Bulk operations
  bulkStart(): void {
    const ids = this.selection.selected.map(p => p.id);
    this.projectService
      .bulkUpdateStatus(ids, ProjectStatus.Active)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open(`${ids.length}件のプロジェクトを開始しました`, '閉じる', {
            duration: 2000,
          });
          this.loadData();
        },
        error: error => {
          console.error('Error bulk starting projects:', error);
          this.snackBar.open('一括開始に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  bulkPause(): void {
    const ids = this.selection.selected.map(p => p.id);
    this.projectService
      .bulkUpdateStatus(ids, ProjectStatus.OnHold, '一括一時停止')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open(`${ids.length}件のプロジェクトを一時停止しました`, '閉じる', {
            duration: 2000,
          });
          this.loadData();
        },
        error: error => {
          console.error('Error bulk pausing projects:', error);
          this.snackBar.open('一括一時停止に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  bulkComplete(): void {
    const ids = this.selection.selected.map(p => p.id);
    this.projectService
      .bulkUpdateStatus(ids, ProjectStatus.Completed, '一括完了')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open(`${ids.length}件のプロジェクトを完了しました`, '閉じる', {
            duration: 2000,
          });
          this.loadData();
        },
        error: error => {
          console.error('Error bulk completing projects:', error);
          this.snackBar.open('一括完了に失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  bulkDelete(): void {
    if (confirm(`選択した${this.selection.selected.length}件のプロジェクトを削除しますか？`)) {
      const ids = this.selection.selected.map(p => p.id);
      this.projectService
        .bulkDelete(ids)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.snackBar.open(`${ids.length}件のプロジェクトを削除しました`, '閉じる', {
              duration: 2000,
            });
            this.loadData();
          },
          error: error => {
            console.error('Error bulk deleting projects:', error);
            this.snackBar.open('一括削除に失敗しました', '閉じる', { duration: 3000 });
          },
        });
    }
  }

  exportProjects(): void {
    const input: GetProjectsInput = {
      ...this.filterForm.value,
      skipCount: 0,
      maxResultCount: this.totalCount,
    };

    this.projectService
      .export(input, 'csv')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: blob => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `projects-${new Date().toISOString().split('T')[0]}.csv`;
          a.click();
          window.URL.revokeObjectURL(url);
        },
        error: error => {
          console.error('Error exporting projects:', error);
          this.snackBar.open('エクスポートに失敗しました', '閉じる', { duration: 3000 });
        },
      });
  }

  // Helper methods
  getStatusText(status: ProjectStatus): string {
    const texts = {
      [ProjectStatus.Planning]: '計画中',
      [ProjectStatus.Active]: '進行中',
      [ProjectStatus.OnHold]: '保留',
      [ProjectStatus.Completed]: '完了',
      [ProjectStatus.Cancelled]: 'キャンセル',
    };
    return texts[status] || 'Unknown';
  }

  getStatusClass(status: ProjectStatus): string {
    const classes = {
      [ProjectStatus.Planning]: 'planning',
      [ProjectStatus.Active]: 'active',
      [ProjectStatus.OnHold]: 'on-hold',
      [ProjectStatus.Completed]: 'completed',
      [ProjectStatus.Cancelled]: 'cancelled',
    };
    return classes[status] || 'default';
  }

  getPriorityText(priority: ProjectPriority): string {
    const texts = {
      [ProjectPriority.Low]: '低',
      [ProjectPriority.Medium]: '中',
      [ProjectPriority.High]: '高',
      [ProjectPriority.Critical]: '緊急',
    };
    return texts[priority] || 'Unknown';
  }

  getPriorityClass(priority: ProjectPriority): string {
    const classes = {
      [ProjectPriority.Low]: 'low',
      [ProjectPriority.Medium]: 'medium',
      [ProjectPriority.High]: 'high',
      [ProjectPriority.Critical]: 'critical',
    };
    return classes[priority] || 'default';
  }

  getProgressClass(progress: number): string {
    if (progress >= 80) return 'high';
    if (progress >= 50) return 'medium';
    if (progress >= 20) return 'low';
    return 'very-low';
  }

  isOverdue(project: AnomalyDetectionProject): boolean {
    const now = new Date();
    const endDate = new Date(project.plannedEndDate);
    return now > endDate && project.status !== ProjectStatus.Completed;
  }
}
