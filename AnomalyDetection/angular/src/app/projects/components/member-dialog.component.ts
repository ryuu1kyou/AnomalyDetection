import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatAutocompleteModule } from '@angular/material/autocomplete';

import { 
  ProjectMember,
  CreateProjectMemberDto,
  UpdateProjectMemberDto
} from '../models/project.model';

export interface MemberDialogData {
  projectId: string;
  member?: ProjectMember;
  mode: 'create' | 'edit';
  availableUsers?: any[]; // User list for selection
}

@Component({
  selector: 'app-member-dialog',
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
    MatCheckboxModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    MatAutocompleteModule
  ],
  template: `
    <div class="member-dialog">
      <h2 mat-dialog-title>
        <mat-icon>person_add</mat-icon>
        {{ data.mode === 'create' ? 'メンバー追加' : 'メンバー編集' }}
      </h2>
      
      <mat-dialog-content>
        <form [formGroup]="memberForm" class="member-form">
          <!-- User Selection (Create mode only) -->
          <div *ngIf="data.mode === 'create'">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>ユーザー</mat-label>
              <input matInput formControlName="userId" placeholder="ユーザーIDを入力"
                     [matAutocomplete]="userAuto">
              <mat-autocomplete #userAuto="matAutocomplete" [displayWith]="displayUser">
                <mat-option *ngFor="let user of filteredUsers" [value]="user.id">
                  <div class="user-option">
                    <div class="user-name">{{ user.userName }}</div>
                    <div class="user-email">{{ user.email }}</div>
                  </div>
                </mat-option>
              </mat-autocomplete>
              <mat-error *ngIf="memberForm.get('userId')?.hasError('required')">
                ユーザーの選択は必須です
              </mat-error>
            </mat-form-field>
          </div>

          <!-- User Info (Edit mode only) -->
          <div *ngIf="data.mode === 'edit' && data.member" class="user-info">
            <h3>ユーザー情報</h3>
            <div class="info-row">
              <span class="label">ユーザー名:</span>
              <span class="value">{{ data.member.userName }}</span>
            </div>
            <div class="info-row">
              <span class="label">メールアドレス:</span>
              <span class="value">{{ data.member.email }}</span>
            </div>
          </div>

          <mat-divider *ngIf="data.mode === 'edit'"></mat-divider>

          <!-- Role and Responsibilities -->
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>役割</mat-label>
            <mat-select formControlName="role">
              <mat-option value="ProjectManager">プロジェクトマネージャー</mat-option>
              <mat-option value="TechnicalLead">テクニカルリード</mat-option>
              <mat-option value="Engineer">エンジニア</mat-option>
              <mat-option value="QualityAssurance">品質保証</mat-option>
              <mat-option value="SystemAnalyst">システムアナリスト</mat-option>
              <mat-option value="TestEngineer">テストエンジニア</mat-option>
              <mat-option value="Reviewer">レビュアー</mat-option>
              <mat-option value="Observer">オブザーバー</mat-option>
            </mat-select>
            <mat-error *ngIf="memberForm.get('role')?.hasError('required')">
              役割の選択は必須です
            </mat-error>
          </mat-form-field>

          <!-- Responsibilities -->
          <div class="responsibilities-section">
            <h3>責任範囲</h3>
            <div formArrayName="responsibilities">
              <div *ngFor="let resp of responsibilitiesArray.controls; let i = index" class="responsibility-item">
                <mat-form-field appearance="outline" class="responsibility-input">
                  <mat-label>責任範囲 {{ i + 1 }}</mat-label>
                  <input matInput [formControlName]="i" placeholder="責任範囲を入力">
                </mat-form-field>
                <button mat-icon-button type="button" (click)="removeResponsibility(i)" color="warn">
                  <mat-icon>remove_circle</mat-icon>
                </button>
              </div>
            </div>
            <button mat-button type="button" (click)="addResponsibility()" class="add-button">
              <mat-icon>add</mat-icon>
              責任範囲を追加
            </button>
          </div>

          <mat-divider></mat-divider>

          <!-- Permissions -->
          <div class="permissions-section">
            <h3>権限設定</h3>
            
            <div class="permission-item">
              <mat-checkbox formControlName="canEdit">
                編集権限
              </mat-checkbox>
              <div class="permission-description">
                プロジェクトの基本情報を編集できます
              </div>
            </div>

            <div class="permission-item">
              <mat-checkbox formControlName="canDelete">
                削除権限
              </mat-checkbox>
              <div class="permission-description">
                プロジェクトのデータを削除できます
              </div>
            </div>

            <div class="permission-item">
              <mat-checkbox formControlName="canManageMembers">
                メンバー管理権限
              </mat-checkbox>
              <div class="permission-description">
                プロジェクトメンバーの追加・削除・編集ができます
              </div>
            </div>
          </div>

          <!-- Status (Edit mode only) -->
          <div *ngIf="data.mode === 'edit'">
            <mat-divider></mat-divider>
            
            <div class="status-section">
              <h3>ステータス</h3>
              
              <div class="permission-item">
                <mat-checkbox formControlName="isActive">
                  アクティブ
                </mat-checkbox>
                <div class="permission-description">
                  このメンバーがプロジェクトでアクティブかどうか
                </div>
              </div>
            </div>
          </div>
        </form>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">キャンセル</button>
        <button mat-raised-button color="primary" 
                [disabled]="memberForm.invalid"
                (click)="onSave()">
          {{ data.mode === 'create' ? '追加' : '更新' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styleUrls: ['./member-dialog.component.scss']
})
export class MemberDialogComponent implements OnInit {
  memberForm: FormGroup;
  filteredUsers: any[] = [];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<MemberDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: MemberDialogData
  ) {
    this.memberForm = this.createForm();
    this.filteredUsers = data.availableUsers || [];
  }

  ngOnInit(): void {
    if (this.data.mode === 'edit' && this.data.member) {
      this.populateForm(this.data.member);
    }

    // Setup user search for create mode
    if (this.data.mode === 'create') {
      this.memberForm.get('userId')?.valueChanges.subscribe(value => {
        this.filterUsers(value);
      });
    }
  }

  private createForm(): FormGroup {
    return this.fb.group({
      userId: ['', this.data.mode === 'create' ? [Validators.required] : []],
      role: ['', [Validators.required]],
      responsibilities: this.fb.array([]),
      canEdit: [false],
      canDelete: [false],
      canManageMembers: [false],
      isActive: [true]
    });
  }

  private populateForm(member: ProjectMember): void {
    this.memberForm.patchValue({
      role: member.role,
      canEdit: member.canEdit,
      canDelete: member.canDelete,
      canManageMembers: member.canManageMembers,
      isActive: member.isActive
    });

    // Populate responsibilities
    const responsibilitiesArray = this.responsibilitiesArray;
    responsibilitiesArray.clear();
    member.responsibilities.forEach(resp => {
      responsibilitiesArray.push(this.fb.control(resp));
    });
  }

  get responsibilitiesArray(): FormArray {
    return this.memberForm.get('responsibilities') as FormArray;
  }

  addResponsibility(): void {
    this.responsibilitiesArray.push(this.fb.control(''));
  }

  removeResponsibility(index: number): void {
    this.responsibilitiesArray.removeAt(index);
  }

  private filterUsers(value: string): void {
    if (!value || typeof value !== 'string') {
      this.filteredUsers = this.data.availableUsers || [];
      return;
    }

    const filterValue = value.toLowerCase();
    this.filteredUsers = (this.data.availableUsers || []).filter(user =>
      user.userName.toLowerCase().includes(filterValue) ||
      user.email.toLowerCase().includes(filterValue)
    );
  }

  displayUser(userId: string): string {
    const user = this.data.availableUsers?.find(u => u.id === userId);
    return user ? user.userName : userId;
  }

  onSave(): void {
    if (this.memberForm.valid) {
      const formValue = this.memberForm.value;
      
      // Filter out empty responsibilities
      const responsibilities = formValue.responsibilities.filter((resp: string) => resp.trim() !== '');

      if (this.data.mode === 'create') {
        const createDto: CreateProjectMemberDto = {
          projectId: this.data.projectId,
          userId: formValue.userId,
          role: formValue.role,
          responsibilities,
          canEdit: formValue.canEdit,
          canDelete: formValue.canDelete,
          canManageMembers: formValue.canManageMembers
        };
        this.dialogRef.close(createDto);
      } else {
        const updateDto: UpdateProjectMemberDto = {
          role: formValue.role,
          responsibilities,
          canEdit: formValue.canEdit,
          canDelete: formValue.canDelete,
          canManageMembers: formValue.canManageMembers,
          isActive: formValue.isActive
        };
        this.dialogRef.close(updateDto);
      }
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}