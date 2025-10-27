import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';

import { 
  ProjectMilestone,
  CreateProjectMilestoneDto,
  UpdateProjectMilestoneDto,
  MilestoneStatus
} from '../models/project.model';

export interface MilestoneDialogData {
  projectId: string;
  milestone?: ProjectMilestone;
  mode: 'create' | 'edit';
}

@Component({
  selector: 'app-milestone-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule
  ],
  template: `
    <div class="milestone-dialog">
      <h2 mat-dialog-title>
        <mat-icon>flag</mat-icon>
        {{ data.mode === 'create' ? 'マイルストーン作成' : 'マイルストーン編集' }}
      </h2>
      
      <mat-dialog-content>
        <form [formGroup]="milestoneForm" class="milestone-form">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>マイルストーン名</mat-label>
            <input matInput formControlName="name" placeholder="マイルストーン名を入力">
            <mat-error *ngIf="milestoneForm.get('name')?.hasError('required')">
              マイルストーン名は必須です
            </mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>説明</mat-label>
            <textarea matInput formControlName="description" rows="3" 
                     placeholder="マイルストーンの詳細説明を入力"></textarea>
          </mat-form-field>

          <div class="form-row">
            <mat-form-field appearance="outline">
              <mat-label>予定日</mat-label>
              <input matInput [matDatepicker]="plannedPicker" formControlName="plannedDate">
              <mat-datepicker-toggle matSuffix [for]="plannedPicker"></mat-datepicker-toggle>
              <mat-datepicker #plannedPicker></mat-datepicker>
              <mat-error *ngIf="milestoneForm.get('plannedDate')?.hasError('required')">
                予定日は必須です
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" *ngIf="data.mode === 'edit'">
              <mat-label>実績日</mat-label>
              <input matInput [matDatepicker]="actualPicker" formControlName="actualDate">
              <mat-datepicker-toggle matSuffix [for]="actualPicker"></mat-datepicker-toggle>
              <mat-datepicker #actualPicker></mat-datepicker>
            </mat-form-field>
          </div>

          <div class="form-row" *ngIf="data.mode === 'edit'">
            <mat-form-field appearance="outline">
              <mat-label>ステータス</mat-label>
              <mat-select formControlName="status">
                <mat-option [value]="MilestoneStatus.NotStarted">未開始</mat-option>
                <mat-option [value]="MilestoneStatus.InProgress">進行中</mat-option>
                <mat-option [value]="MilestoneStatus.Completed">完了</mat-option>
                <mat-option [value]="MilestoneStatus.Delayed">遅延</mat-option>
                <mat-option [value]="MilestoneStatus.Cancelled">キャンセル</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>進捗率 (%)</mat-label>
              <input matInput type="number" formControlName="progressPercentage" 
                     min="0" max="100" placeholder="0-100">
              <mat-error *ngIf="milestoneForm.get('progressPercentage')?.hasError('min')">
                0以上の値を入力してください
              </mat-error>
              <mat-error *ngIf="milestoneForm.get('progressPercentage')?.hasError('max')">
                100以下の値を入力してください
              </mat-error>
            </mat-form-field>
          </div>

          <mat-divider></mat-divider>

          <!-- Dependencies Section -->
          <div class="dependencies-section">
            <h3>依存関係</h3>
            <div formArrayName="dependencies">
              <div *ngFor="let dep of dependenciesArray.controls; let i = index" class="dependency-item">
                <mat-form-field appearance="outline" class="dependency-input">
                  <mat-label>依存マイルストーン {{ i + 1 }}</mat-label>
                  <input matInput [formControlName]="i" placeholder="依存するマイルストーンID">
                </mat-form-field>
                <button mat-icon-button type="button" (click)="removeDependency(i)" color="warn">
                  <mat-icon>remove_circle</mat-icon>
                </button>
              </div>
            </div>
            <button mat-button type="button" (click)="addDependency()" class="add-button">
              <mat-icon>add</mat-icon>
              依存関係を追加
            </button>
          </div>

          <mat-divider></mat-divider>

          <!-- Deliverables Section -->
          <div class="deliverables-section">
            <h3>成果物</h3>
            <div formArrayName="deliverables">
              <div *ngFor="let del of deliverablesArray.controls; let i = index" class="deliverable-item">
                <mat-form-field appearance="outline" class="deliverable-input">
                  <mat-label>成果物 {{ i + 1 }}</mat-label>
                  <input matInput [formControlName]="i" placeholder="成果物名を入力">
                </mat-form-field>
                <button mat-icon-button type="button" (click)="removeDeliverable(i)" color="warn">
                  <mat-icon>remove_circle</mat-icon>
                </button>
              </div>
            </div>
            <button mat-button type="button" (click)="addDeliverable()" class="add-button">
              <mat-icon>add</mat-icon>
              成果物を追加
            </button>
          </div>
        </form>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">キャンセル</button>
        <button mat-raised-button color="primary" 
                [disabled]="milestoneForm.invalid"
                (click)="onSave()">
          {{ data.mode === 'create' ? '作成' : '更新' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styleUrls: ['./milestone-dialog.component.scss']
})
export class MilestoneDialogComponent implements OnInit {
  MilestoneStatus = MilestoneStatus;
  milestoneForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<MilestoneDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: MilestoneDialogData
  ) {
    this.milestoneForm = this.createForm();
  }

  ngOnInit(): void {
    if (this.data.mode === 'edit' && this.data.milestone) {
      this.populateForm(this.data.milestone);
    }
  }

  private createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(1000)]],
      plannedDate: [null, Validators.required],
      actualDate: [null],
      status: [MilestoneStatus.NotStarted],
      progressPercentage: [0, [Validators.min(0), Validators.max(100)]],
      dependencies: this.fb.array([]),
      deliverables: this.fb.array([])
    });
  }

  private populateForm(milestone: ProjectMilestone): void {
    this.milestoneForm.patchValue({
      name: milestone.name,
      description: milestone.description,
      plannedDate: new Date(milestone.plannedDate),
      actualDate: milestone.actualDate ? new Date(milestone.actualDate) : null,
      status: milestone.status,
      progressPercentage: milestone.progressPercentage
    });

    // Populate dependencies
    const dependenciesArray = this.dependenciesArray;
    dependenciesArray.clear();
    milestone.dependencies.forEach(dep => {
      dependenciesArray.push(this.fb.control(dep));
    });

    // Populate deliverables
    const deliverablesArray = this.deliverablesArray;
    deliverablesArray.clear();
    milestone.deliverables.forEach(del => {
      deliverablesArray.push(this.fb.control(del));
    });
  }

  get dependenciesArray(): FormArray {
    return this.milestoneForm.get('dependencies') as FormArray;
  }

  get deliverablesArray(): FormArray {
    return this.milestoneForm.get('deliverables') as FormArray;
  }

  addDependency(): void {
    this.dependenciesArray.push(this.fb.control(''));
  }

  removeDependency(index: number): void {
    this.dependenciesArray.removeAt(index);
  }

  addDeliverable(): void {
    this.deliverablesArray.push(this.fb.control(''));
  }

  removeDeliverable(index: number): void {
    this.deliverablesArray.removeAt(index);
  }

  onSave(): void {
    if (this.milestoneForm.valid) {
      const formValue = this.milestoneForm.value;
      
      // Filter out empty dependencies and deliverables
      const dependencies = formValue.dependencies.filter((dep: string) => dep.trim() !== '');
      const deliverables = formValue.deliverables.filter((del: string) => del.trim() !== '');

      if (this.data.mode === 'create') {
        const createDto: CreateProjectMilestoneDto = {
          projectId: this.data.projectId,
          name: formValue.name,
          description: formValue.description,
          plannedDate: formValue.plannedDate,
          dependencies,
          deliverables
        };
        this.dialogRef.close(createDto);
      } else {
        const updateDto: UpdateProjectMilestoneDto = {
          name: formValue.name,
          description: formValue.description,
          plannedDate: formValue.plannedDate,
          actualDate: formValue.actualDate,
          status: formValue.status,
          progressPercentage: formValue.progressPercentage,
          dependencies,
          deliverables
        };
        this.dialogRef.close(updateDto);
      }
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}