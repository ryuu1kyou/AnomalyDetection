import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { Subject, takeUntil, switchMap, forkJoin } from 'rxjs';

import { ProjectService } from '../services/project.service';
import { 
  AnomalyDetectionProject,
  ProjectMilestone,
  ProjectMember,
  ProjectStatus,
  ProjectPriority,
  MilestoneStatus
} from '../models/project.model';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTabsModule,
    MatProgressBarModule,
    MatTooltipModule,
    MatMenuModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatDividerModule,
    MatListModule
  ],
  template: `
    <div class="project-detail" *ngIf="project">
      <!-- Header -->
      <div class="header">
        <div class="header-info">
          <button mat-icon-button (click)="goBack()" class="back-button">
            <mat-icon>arrow_back</mat-icon>
          </button>
          <div class="title-section">
            <h2>{{ project.projectName }}</h2>
            <div class="project-code">{{ project.projectCode }}</div>
          </div>
        </div>
        <div class="header-actions">
          <button mat-raised-button color="primary" (click)="refreshData()">
            <mat-icon>refresh</mat-icon>
            更新
          </button>
          <button mat-button [matMenuTriggerFor]="actionMenu">
            <mat-icon>more_vert</mat-icon>
            操作
          </button>
          <mat-menu #actionMenu="matMenu">
            <button mat-menu-item (click)="editProject()">
              <mat-icon>edit</mat-icon>
              編集
            </button>
            <button mat-menu-item (click)="manageMembers()">
              <mat-icon>people</mat-icon>
              メンバー管理
            </button>
            <button mat-menu-item (click)="manageMilestones()">
              <mat-icon>flag</mat-icon>
              マイルストーン管理
            </button>
            <mat-divider></mat-divider>
            <button mat-menu-item (click)="startProject()" 
                    *ngIf="project.status === ProjectStatus.Planning">
              <mat-icon>play_arrow</mat-icon>
              開始
            </button>
            <button mat-menu-item (click)="pauseProject()"
                    *ngIf="project.status === ProjectStatus.Active">
              <mat-icon>pause</mat-icon>
              一時停止
            </button>
            <button mat-menu-item (click)="completeProject()"
                    *ngIf="project.status === ProjectStatus.Active">
              <mat-icon>check</mat-icon>
              完了
            </button>
            <button mat-menu-item (click)="generateReport()">
              <mat-icon>assessment</mat-icon>
              レポート生成
            </button>
          </mat-menu>
        </div>
      </div>

      <!-- Status Cards -->
      <div class="status-cards">
        <mat-card class="status-card">
          <mat-card-header>
            <mat-card-title>ステータス</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <mat-chip [class]="'status-' + getStatusClass(project.status)">
              {{ getStatusText(project.status) }}
            </mat-chip>
          </mat-card-content>
        </mat-card>

        <mat-card class="status-card">
          <mat-card-header>
            <mat-card-title>優先度</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <mat-chip [class]="'priority-' + getPriorityClass(project.priority)">
              {{ getPriorityText(project.priority) }}
            </mat-chip>
          </mat-card-content>
        </mat-card>

        <mat-card class="status-card">
          <mat-card-header>
            <mat-card-title>進捗率</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="progress-display">
              <div class="progress-value">{{ project.progressPercentage }}%</div>
              <mat-progress-bar 
                mode="determinate" 
                [value]="project.progressPercentage"
                [class]="'progress-' + getProgressClass(project.progressPercentage)">
              </mat-progress-bar>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="status-card">
          <mat-card-header>
            <mat-card-title>期限</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="deadline" [class.overdue]="isOverdue()">
              {{ project.plannedEndDate | date:'yyyy/MM/dd' }}
              <mat-icon *ngIf="isOverdue()" matTooltip="期限超過">warning</mat-icon>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Main Content Tabs -->
      <mat-tab-group class="detail-tabs">
        <!-- Overview Tab -->
        <mat-tab label="概要">
          <div class="tab-content">
            <div class="overview-grid">
              <mat-card>
                <mat-card-header>
                  <mat-card-title>基本情報</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="info-row">
                    <span class="label">プロジェクト名:</span>
                    <span class="value">{{ project.projectName }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">プロジェクトコード:</span>
                    <span class="value project-code">{{ project.projectCode }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">説明:</span>
                    <span class="value">{{ project.description }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">OEM:</span>
                    <span class="value">{{ project.oemName }}</span>
                  </div>
                </mat-card-content>
              </mat-card>

              <mat-card>
                <mat-card-header>
                  <mat-card-title>車両情報</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="info-row">
                    <span class="label">車両モデル:</span>
                    <span class="value">{{ project.vehicleModel }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">モデル年:</span>
                    <span class="value">{{ project.modelYear }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">プラットフォーム:</span>
                    <span class="value">{{ project.platform }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">主要システム:</span>
                    <span class="value">{{ project.primarySystem }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">対象市場:</span>
                    <span class="value">{{ project.targetMarket }}</span>
                  </div>
                </mat-card-content>
              </mat-card>

              <mat-card>
                <mat-card-header>
                  <mat-card-title>スケジュール</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="info-row">
                    <span class="label">開始日:</span>
                    <span class="value">{{ project.startDate | date:'yyyy/MM/dd' }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">予定終了日:</span>
                    <span class="value">{{ project.plannedEndDate | date:'yyyy/MM/dd' }}</span>
                  </div>
                  <div class="info-row" *ngIf="project.actualEndDate">
                    <span class="label">実際の終了日:</span>
                    <span class="value">{{ project.actualEndDate | date:'yyyy/MM/dd' }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">期間:</span>
                    <span class="value">{{ calculateDuration() }}日</span>
                  </div>
                </mat-card-content>
              </mat-card>

              <mat-card>
                <mat-card-header>
                  <mat-card-title>統計情報</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="info-row">
                    <span class="label">検出ロジック数:</span>
                    <span class="value">{{ project.totalDetectionLogics }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">CAN信号数:</span>
                    <span class="value">{{ project.totalCanSignals }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">総異常数:</span>
                    <span class="value">{{ project.totalAnomalies }}</span>
                  </div>
                  <div class="info-row">
                    <span class="label">解決済み異常:</span>
                    <span class="value">{{ project.resolvedAnomalies }}</span>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
          </div>
        </mat-tab>

        <!-- Progress Tab -->
        <mat-tab label="進捗管理">
          <div class="tab-content">
            <mat-card>
              <mat-card-header>
                <mat-card-title>進捗ダッシュボード</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="progress-dashboard">
                  <div class="progress-chart">
                    <!-- TODO: Implement Gantt chart or timeline visualization -->
                    <div class="chart-placeholder">
                      <mat-icon>timeline</mat-icon>
                      <p>ガントチャート表示予定</p>
                    </div>
                  </div>
                  
                  <div class="progress-metrics">
                    <div class="metric-item">
                      <div class="metric-label">全体進捗</div>
                      <div class="metric-value">{{ project.progressPercentage }}%</div>
                      <mat-progress-bar 
                        mode="determinate" 
                        [value]="project.progressPercentage">
                      </mat-progress-bar>
                    </div>
                    
                    <div class="metric-item" *ngIf="progressData">
                      <div class="metric-label">マイルストーン達成率</div>
                      <div class="metric-value">{{ progressData.milestoneCompletionRate }}%</div>
                      <mat-progress-bar 
                        mode="determinate" 
                        [value]="progressData.milestoneCompletionRate">
                      </mat-progress-bar>
                    </div>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>

        <!-- Milestones Tab -->
        <mat-tab label="マイルストーン">
          <div class="tab-content">
            <mat-card>
              <mat-card-header>
                <mat-card-title>マイルストーン一覧</mat-card-title>
                <div class="card-actions">
                  <button mat-button color="primary" (click)="addMilestone()">
                    <mat-icon>add</mat-icon>
                    追加
                  </button>
                </div>
              </mat-card-header>
              <mat-card-content>
                <div class="milestones-list" *ngIf="milestones.length > 0; else noMilestones">
                  <div class="milestone-item" *ngFor="let milestone of milestones">
                    <div class="milestone-header">
                      <div class="milestone-info">
                        <h4>{{ milestone.name }}</h4>
                        <p>{{ milestone.description }}</p>
                      </div>
                      <div class="milestone-actions">
                        <mat-chip [class]="'milestone-' + getMilestoneStatusClass(milestone.status)">
                          {{ getMilestoneStatusText(milestone.status) }}
                        </mat-chip>
                        <button mat-icon-button [matMenuTriggerFor]="milestoneMenu">
                          <mat-icon>more_vert</mat-icon>
                        </button>
                        <mat-menu #milestoneMenu="matMenu">
                          <button mat-menu-item (click)="editMilestone(milestone)">
                            <mat-icon>edit</mat-icon>
                            編集
                          </button>
                          <button mat-menu-item (click)="completeMilestone(milestone)"
                                  *ngIf="milestone.status !== MilestoneStatus.Completed">
                            <mat-icon>check</mat-icon>
                            完了
                          </button>
                          <button mat-menu-item (click)="deleteMilestone(milestone)">
                            <mat-icon>delete</mat-icon>
                            削除
                          </button>
                        </mat-menu>
                      </div>
                    </div>
                    
                    <div class="milestone-details">
                      <div class="milestone-dates">
                        <span class="date-item">
                          <mat-icon>schedule</mat-icon>
                          予定: {{ milestone.plannedDate | date:'yyyy/MM/dd' }}
                        </span>
                        <span class="date-item" *ngIf="milestone.actualDate">
                          <mat-icon>event_available</mat-icon>
                          実績: {{ milestone.actualDate | date:'yyyy/MM/dd' }}
                        </span>
                      </div>
                      
                      <div class="milestone-progress">
                        <span class="progress-label">進捗: {{ milestone.progressPercentage }}%</span>
                        <mat-progress-bar 
                          mode="determinate" 
                          [value]="milestone.progressPercentage">
                        </mat-progress-bar>
                      </div>
                    </div>
                  </div>
                </div>
                
                <ng-template #noMilestones>
                  <div class="no-milestones">
                    <mat-icon>flag_off</mat-icon>
                    <p>マイルストーンが設定されていません</p>
                    <button mat-raised-button color="primary" (click)="addMilestone()">
                      マイルストーンを追加
                    </button>
                  </div>
                </ng-template>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>

        <!-- Members Tab -->
        <mat-tab label="メンバー">
          <div class="tab-content">
            <mat-card>
              <mat-card-header>
                <mat-card-title>プロジェクトメンバー</mat-card-title>
                <div class="card-actions">
                  <button mat-button color="primary" (click)="addMember()">
                    <mat-icon>person_add</mat-icon>
                    追加
                  </button>
                </div>
              </mat-card-header>
              <mat-card-content>
                <div class="members-list" *ngIf="members.length > 0; else noMembers">
                  <mat-list>
                    <mat-list-item *ngFor="let member of members">
                      <div matListItemTitle>{{ member.userName }}</div>
                      <div matListItemLine>{{ member.role }} - {{ member.email }}</div>
                      <div matListItemMeta class="member-actions">
                        <mat-chip [class.inactive]="!member.isActive">
                          {{ member.isActive ? 'アクティブ' : '非アクティブ' }}
                        </mat-chip>
                        <button mat-icon-button [matMenuTriggerFor]="memberMenu">
                          <mat-icon>more_vert</mat-icon>
                        </button>
                        <mat-menu #memberMenu="matMenu">
                          <button mat-menu-item (click)="editMember(member)">
                            <mat-icon>edit</mat-icon>
                            編集
                          </button>
                          <button mat-menu-item (click)="removeMember(member)">
                            <mat-icon>remove</mat-icon>
                            削除
                          </button>
                        </mat-menu>
                      </div>
                    </mat-list-item>
                  </mat-list>
                </div>
                
                <ng-template #noMembers>
                  <div class="no-members">
                    <mat-icon>people_off</mat-icon>
                    <p>メンバーが登録されていません</p>
                    <button mat-raised-button color="primary" (click)="addMember()">
                      メンバーを追加
                    </button>
                  </div>
                </ng-template>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>

        <!-- Related Data Tab -->
        <mat-tab label="関連データ">
          <div class="tab-content">
            <div class="related-data-grid">
              <mat-card>
                <mat-card-header>
                  <mat-card-title>検出ロジック</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="related-count">{{ project.totalDetectionLogics }} 件</div>
                  <button mat-button (click)="viewDetectionLogics()">詳細を表示</button>
                </mat-card-content>
              </mat-card>

              <mat-card>
                <mat-card-header>
                  <mat-card-title>CAN信号</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="related-count">{{ project.totalCanSignals }} 件</div>
                  <button mat-button (click)="viewCanSignals()">詳細を表示</button>
                </mat-card-content>
              </mat-card>

              <mat-card>
                <mat-card-header>
                  <mat-card-title>異常検出結果</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="related-count">{{ project.totalAnomalies }} 件</div>
                  <div class="resolved-count">解決済み: {{ project.resolvedAnomalies }} 件</div>
                  <button mat-button (click)="viewAnomalies()">詳細を表示</button>
                </mat-card-content>
              </mat-card>
            </div>
          </div>
        </mat-tab>
      </mat-tab-group>
    </div>

    <!-- Loading Spinner -->
    <div class="loading-container" *ngIf="loading">
      <mat-spinner diameter="50"></mat-spinner>
    </div>
  `,
  styleUrls: ['./project-detail.component.scss']
})
export class ProjectDetailComponent implements OnInit, OnDestroy {
  @Input() projectId?: string;

  // Enums for template
  ProjectStatus = ProjectStatus;
  ProjectPriority = ProjectPriority;
  MilestoneStatus = MilestoneStatus;

  project?: AnomalyDetectionProject;
  milestones: ProjectMilestone[] = [];
  members: ProjectMember[] = [];
  progressData: any;
  loading = false;
  
  private destroy$ = new Subject<void>();

  constructor(
    private projectService: ProjectService,
    private route: ActivatedRoute,
    private router: Router,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    if (this.projectId) {
      this.loadProject(this.projectId);
    } else {
      this.route.params
        .pipe(takeUntil(this.destroy$))
        .subscribe(params => {
          if (params['id']) {
            this.loadProject(params['id']);
          }
        });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProject(id: string): void {
    this.loading = true;
    
    forkJoin({
      project: this.projectService.get(id),
      milestones: this.projectService.getMilestones(id),
      members: this.projectService.getMembers(id),
      progress: this.projectService.getProjectProgress(id)
    }).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.project = data.project;
          this.milestones = data.milestones;
          this.members = data.members;
          this.progressData = data.progress;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading project:', error);
          this.snackBar.open('データの読み込みに失敗しました', '閉じる', { duration: 3000 });
          this.loading = false;
        }
      });
  }

  refreshData(): void {
    if (this.project) {
      this.loadProject(this.project.id);
    }
  }

  goBack(): void {
    this.router.navigate(['/projects']);
  }

  // Project operations
  editProject(): void {
    if (this.project) {
      this.router.navigate(['/projects', this.project.id, 'edit']);
    }
  }

  manageMembers(): void {
    if (this.project) {
      this.router.navigate(['/projects', this.project.id, 'members']);
    }
  }

  manageMilestones(): void {
    if (this.project) {
      this.router.navigate(['/projects', this.project.id, 'milestones']);
    }
  }

  startProject(): void {
    if (!this.project) return;
    
    this.projectService.startProject(this.project.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('プロジェクトを開始しました', '閉じる', { duration: 2000 });
          this.refreshData();
        },
        error: (error) => {
          console.error('Error starting project:', error);
          this.snackBar.open('プロジェクトの開始に失敗しました', '閉じる', { duration: 3000 });
        }
      });
  }

  pauseProject(): void {
    if (!this.project) return;
    
    this.projectService.pauseProject(this.project.id, '一時停止')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('プロジェクトを一時停止しました', '閉じる', { duration: 2000 });
          this.refreshData();
        },
        error: (error) => {
          console.error('Error pausing project:', error);
          this.snackBar.open('プロジェクトの一時停止に失敗しました', '閉じる', { duration: 3000 });
        }
      });
  }

  completeProject(): void {
    if (!this.project) return;
    
    this.projectService.completeProject(this.project.id, '完了')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('プロジェクトを完了しました', '閉じる', { duration: 2000 });
          this.refreshData();
        },
        error: (error) => {
          console.error('Error completing project:', error);
          this.snackBar.open('プロジェクトの完了に失敗しました', '閉じる', { duration: 3000 });
        }
      });
  }

  generateReport(): void {
    if (!this.project) return;
    
    this.projectService.generateProgressReport(this.project.id, 'pdf')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `project-report-${this.project!.projectCode}-${new Date().toISOString().split('T')[0]}.pdf`;
          a.click();
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          console.error('Error generating report:', error);
          this.snackBar.open('レポート生成に失敗しました', '閉じる', { duration: 3000 });
        }
      });
  }

  // Milestone operations
  addMilestone(): void {
    if (!this.project) return;
    
    import('./milestone-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.MilestoneDialogComponent, {
        width: '600px',
        data: { 
          projectId: this.project!.id,
          mode: 'create'
        }
      });

      dialogRef.afterClosed().subscribe(milestoneData => {
        if (milestoneData) {
          this.projectService.createMilestone(milestoneData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => {
                this.snackBar.open('マイルストーンを作成しました', '閉じる', { duration: 2000 });
                this.refreshData();
              },
              error: (error) => {
                console.error('Error creating milestone:', error);
                this.snackBar.open('マイルストーンの作成に失敗しました', '閉じる', { duration: 3000 });
              }
            });
        }
      });
    });
  }

  editMilestone(milestone: ProjectMilestone): void {
    if (!this.project) return;
    
    import('./milestone-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.MilestoneDialogComponent, {
        width: '600px',
        data: { 
          projectId: this.project!.id,
          milestone,
          mode: 'edit'
        }
      });

      dialogRef.afterClosed().subscribe(milestoneData => {
        if (milestoneData) {
          this.projectService.updateMilestone(milestone.id, milestoneData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => {
                this.snackBar.open('マイルストーンを更新しました', '閉じる', { duration: 2000 });
                this.refreshData();
              },
              error: (error) => {
                console.error('Error updating milestone:', error);
                this.snackBar.open('マイルストーンの更新に失敗しました', '閉じる', { duration: 3000 });
              }
            });
        }
      });
    });
  }

  completeMilestone(milestone: ProjectMilestone): void {
    this.projectService.completeMilestone(milestone.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('マイルストーンを完了しました', '閉じる', { duration: 2000 });
          this.refreshData();
        },
        error: (error) => {
          console.error('Error completing milestone:', error);
          this.snackBar.open('マイルストーンの完了に失敗しました', '閉じる', { duration: 3000 });
        }
      });
  }

  deleteMilestone(milestone: ProjectMilestone): void {
    if (confirm(`マイルストーン "${milestone.name}" を削除しますか？`)) {
      this.projectService.deleteMilestone(milestone.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.snackBar.open('マイルストーンを削除しました', '閉じる', { duration: 2000 });
            this.refreshData();
          },
          error: (error) => {
            console.error('Error deleting milestone:', error);
            this.snackBar.open('マイルストーンの削除に失敗しました', '閉じる', { duration: 3000 });
          }
        });
    }
  }

  // Member operations
  addMember(): void {
    if (!this.project) return;
    
    import('./member-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.MemberDialogComponent, {
        width: '600px',
        data: { 
          projectId: this.project!.id,
          mode: 'create',
          availableUsers: [] // TODO: Load available users
        }
      });

      dialogRef.afterClosed().subscribe(memberData => {
        if (memberData) {
          this.projectService.addMember(memberData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => {
                this.snackBar.open('メンバーを追加しました', '閉じる', { duration: 2000 });
                this.refreshData();
              },
              error: (error) => {
                console.error('Error adding member:', error);
                this.snackBar.open('メンバーの追加に失敗しました', '閉じる', { duration: 3000 });
              }
            });
        }
      });
    });
  }

  editMember(member: ProjectMember): void {
    if (!this.project) return;
    
    import('./member-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.MemberDialogComponent, {
        width: '600px',
        data: { 
          projectId: this.project!.id,
          member,
          mode: 'edit'
        }
      });

      dialogRef.afterClosed().subscribe(memberData => {
        if (memberData) {
          this.projectService.updateMember(member.id, memberData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => {
                this.snackBar.open('メンバー情報を更新しました', '閉じる', { duration: 2000 });
                this.refreshData();
              },
              error: (error) => {
                console.error('Error updating member:', error);
                this.snackBar.open('メンバー情報の更新に失敗しました', '閉じる', { duration: 3000 });
              }
            });
        }
      });
    });
  }

  removeMember(member: ProjectMember): void {
    if (confirm(`メンバー "${member.userName}" を削除しますか？`)) {
      this.projectService.removeMember(member.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.snackBar.open('メンバーを削除しました', '閉じる', { duration: 2000 });
            this.refreshData();
          },
          error: (error) => {
            console.error('Error removing member:', error);
            this.snackBar.open('メンバーの削除に失敗しました', '閉じる', { duration: 3000 });
          }
        });
    }
  }

  // Related data navigation
  viewDetectionLogics(): void {
    if (this.project) {
      this.router.navigate(['/detection-logics'], { 
        queryParams: { projectId: this.project.id } 
      });
    }
  }

  viewCanSignals(): void {
    if (this.project) {
      this.router.navigate(['/can-signals'], { 
        queryParams: { projectId: this.project.id } 
      });
    }
  }

  viewAnomalies(): void {
    if (this.project) {
      this.router.navigate(['/detection-results'], { 
        queryParams: { projectId: this.project.id } 
      });
    }
  }

  // Helper methods
  calculateDuration(): number {
    if (!this.project) return 0;
    
    const start = new Date(this.project.startDate);
    const end = this.project.actualEndDate ? 
      new Date(this.project.actualEndDate) : 
      new Date(this.project.plannedEndDate);
    
    return Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24));
  }

  isOverdue(): boolean {
    if (!this.project) return false;
    
    const now = new Date();
    const endDate = new Date(this.project.plannedEndDate);
    return now > endDate && this.project.status !== ProjectStatus.Completed;
  }

  getStatusText(status: ProjectStatus): string {
    const texts = {
      [ProjectStatus.Planning]: '計画中',
      [ProjectStatus.Active]: '進行中',
      [ProjectStatus.OnHold]: '保留',
      [ProjectStatus.Completed]: '完了',
      [ProjectStatus.Cancelled]: 'キャンセル'
    };
    return texts[status] || 'Unknown';
  }

  getStatusClass(status: ProjectStatus): string {
    const classes = {
      [ProjectStatus.Planning]: 'planning',
      [ProjectStatus.Active]: 'active',
      [ProjectStatus.OnHold]: 'on-hold',
      [ProjectStatus.Completed]: 'completed',
      [ProjectStatus.Cancelled]: 'cancelled'
    };
    return classes[status] || 'default';
  }

  getPriorityText(priority: ProjectPriority): string {
    const texts = {
      [ProjectPriority.Low]: '低',
      [ProjectPriority.Medium]: '中',
      [ProjectPriority.High]: '高',
      [ProjectPriority.Critical]: '緊急'
    };
    return texts[priority] || 'Unknown';
  }

  getPriorityClass(priority: ProjectPriority): string {
    const classes = {
      [ProjectPriority.Low]: 'low',
      [ProjectPriority.Medium]: 'medium',
      [ProjectPriority.High]: 'high',
      [ProjectPriority.Critical]: 'critical'
    };
    return classes[priority] || 'default';
  }

  getProgressClass(progress: number): string {
    if (progress >= 80) return 'high';
    if (progress >= 50) return 'medium';
    if (progress >= 20) return 'low';
    return 'very-low';
  }

  getMilestoneStatusText(status: MilestoneStatus): string {
    const texts = {
      [MilestoneStatus.NotStarted]: '未開始',
      [MilestoneStatus.InProgress]: '進行中',
      [MilestoneStatus.Completed]: '完了',
      [MilestoneStatus.Delayed]: '遅延',
      [MilestoneStatus.Cancelled]: 'キャンセル'
    };
    return texts[status] || 'Unknown';
  }

  getMilestoneStatusClass(status: MilestoneStatus): string {
    const classes = {
      [MilestoneStatus.NotStarted]: 'not-started',
      [MilestoneStatus.InProgress]: 'in-progress',
      [MilestoneStatus.Completed]: 'completed',
      [MilestoneStatus.Delayed]: 'delayed',
      [MilestoneStatus.Cancelled]: 'cancelled'
    };
    return classes[status] || 'default';
  }
}