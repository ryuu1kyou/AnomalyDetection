import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatPaginatorModule } from '@angular/material/paginator';
import { OemTraceabilityService } from '../../services/oem-traceability.service';
import {
  OemCustomization,
  CreateOemCustomization,
  CustomizationType,
  CustomizationStatus
} from '../../models/oem-traceability.models';

@Component({
  selector: 'app-oem-customization-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatDialogModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatPaginatorModule
  ],
  templateUrl: './oem-customization-management.component.html',
  styleUrls: ['./oem-customization-management.component.scss']
})
export class OemCustomizationManagementComponent implements OnInit {
  customizations: OemCustomization[] = [];
  loading = false;
  
  // Filters
  selectedOemCode = '';
  selectedEntityType = '';
  selectedStatus: CustomizationStatus | null = null;
  
  // Form
  customizationForm: FormGroup;
  isEditing = false;
  editingId: string | null = null;
  
  // Table columns
  displayedColumns = ['entityId', 'entityType', 'oemCode', 'type', 'status', 'creationTime', 'actions'];
  
  // Enums for templates
  CustomizationType = CustomizationType;
  CustomizationStatus = CustomizationStatus;
  
  customizationTypes = [
    { value: CustomizationType.ParameterAdjustment, label: 'パラメータ調整' },
    { value: CustomizationType.ThresholdChange, label: '閾値変更' },
    { value: CustomizationType.LogicModification, label: 'ロジック修正' },
    { value: CustomizationType.SpecificationChange, label: '仕様変更' },
    { value: CustomizationType.Other, label: 'その他' }
  ];
  
  customizationStatuses = [
    { value: CustomizationStatus.Draft, label: '下書き' },
    { value: CustomizationStatus.PendingApproval, label: '承認待ち' },
    { value: CustomizationStatus.Approved, label: '承認済み' },
    { value: CustomizationStatus.Rejected, label: '却下' },
    { value: CustomizationStatus.Obsolete, label: '廃止' }
  ];

  constructor(
    private oemTraceabilityService: OemTraceabilityService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private fb: FormBuilder
  ) {
    this.customizationForm = this.fb.group({
      entityId: ['', Validators.required],
      entityType: ['', Validators.required],
      oemCode: ['', Validators.required],
      type: [CustomizationType.ParameterAdjustment, Validators.required],
      customizationReason: ['', Validators.required],
      customParameters: ['{}'],
      originalParameters: ['{}']
    });
  }

  ngOnInit(): void {
    this.loadCustomizations();
  }

  loadCustomizations(): void {
    this.loading = true;
    this.oemTraceabilityService.getOemCustomizations(
      this.selectedOemCode || undefined,
      this.selectedEntityType || undefined,
      this.selectedStatus || undefined
    ).subscribe({
      next: (customizations) => {
        this.customizations = customizations;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading customizations:', error);
        this.snackBar.open('カスタマイズの読み込みに失敗しました', '閉じる', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.loadCustomizations();
  }

  clearFilters(): void {
    this.selectedOemCode = '';
    this.selectedEntityType = '';
    this.selectedStatus = null;
    this.loadCustomizations();
  }

  createCustomization(): void {
    if (this.customizationForm.valid) {
      const formValue = this.customizationForm.value;
      
      try {
        const customParameters = JSON.parse(formValue.customParameters);
        const originalParameters = JSON.parse(formValue.originalParameters);
        
        const createDto: CreateOemCustomization = {
          entityId: formValue.entityId,
          entityType: formValue.entityType,
          oemCode: formValue.oemCode,
          type: formValue.type,
          customizationReason: formValue.customizationReason,
          customParameters,
          originalParameters
        };

        this.oemTraceabilityService.createOemCustomization(createDto).subscribe({
          next: () => {
            this.snackBar.open('カスタマイズが作成されました', '閉じる', { duration: 3000 });
            this.resetForm();
            this.loadCustomizations();
          },
          error: (error) => {
            console.error('Error creating customization:', error);
            this.snackBar.open('カスタマイズの作成に失敗しました', '閉じる', { duration: 3000 });
          }
        });
      } catch (e) {
        this.snackBar.open('JSONパラメータの形式が正しくありません', '閉じる', { duration: 3000 });
      }
    }
  }

  editCustomization(customization: OemCustomization): void {
    this.isEditing = true;
    this.editingId = customization.id;
    
    this.customizationForm.patchValue({
      entityId: customization.entityId,
      entityType: customization.entityType,
      oemCode: customization.oemCode,
      type: customization.type,
      customizationReason: customization.customizationReason,
      customParameters: JSON.stringify(customization.customParameters, null, 2),
      originalParameters: JSON.stringify(customization.originalParameters, null, 2)
    });
  }

  updateCustomization(): void {
    if (this.customizationForm.valid && this.editingId) {
      const formValue = this.customizationForm.value;
      
      try {
        const customParameters = JSON.parse(formValue.customParameters);
        
        const updateDto = {
          customParameters,
          customizationReason: formValue.customizationReason
        };

        this.oemTraceabilityService.updateOemCustomization(this.editingId, updateDto).subscribe({
          next: () => {
            this.snackBar.open('カスタマイズが更新されました', '閉じる', { duration: 3000 });
            this.resetForm();
            this.loadCustomizations();
          },
          error: (error) => {
            console.error('Error updating customization:', error);
            this.snackBar.open('カスタマイズの更新に失敗しました', '閉じる', { duration: 3000 });
          }
        });
      } catch (e) {
        this.snackBar.open('JSONパラメータの形式が正しくありません', '閉じる', { duration: 3000 });
      }
    }
  }

  submitForApproval(id: string): void {
    this.oemTraceabilityService.submitForApproval(id).subscribe({
      next: () => {
        this.snackBar.open('承認申請が送信されました', '閉じる', { duration: 3000 });
        this.loadCustomizations();
      },
      error: (error) => {
        console.error('Error submitting for approval:', error);
        this.snackBar.open('承認申請に失敗しました', '閉じる', { duration: 3000 });
      }
    });
  }

  approveCustomization(id: string): void {
    const approvalNotes = prompt('承認コメントを入力してください（オプション）:');
    
    this.oemTraceabilityService.approveCustomization(id, approvalNotes || undefined).subscribe({
      next: () => {
        this.snackBar.open('カスタマイズが承認されました', '閉じる', { duration: 3000 });
        this.loadCustomizations();
      },
      error: (error) => {
        console.error('Error approving customization:', error);
        this.snackBar.open('承認に失敗しました', '閉じる', { duration: 3000 });
      }
    });
  }

  rejectCustomization(id: string): void {
    const rejectionNotes = prompt('却下理由を入力してください:');
    
    if (rejectionNotes) {
      this.oemTraceabilityService.rejectCustomization(id, rejectionNotes).subscribe({
        next: () => {
          this.snackBar.open('カスタマイズが却下されました', '閉じる', { duration: 3000 });
          this.loadCustomizations();
        },
        error: (error) => {
          console.error('Error rejecting customization:', error);
          this.snackBar.open('却下に失敗しました', '閉じる', { duration: 3000 });
        }
      });
    }
  }

  resetForm(): void {
    this.isEditing = false;
    this.editingId = null;
    this.customizationForm.reset({
      type: CustomizationType.ParameterAdjustment,
      customParameters: '{}',
      originalParameters: '{}'
    });
  }

  getStatusLabel(status: CustomizationStatus): string {
    const statusObj = this.customizationStatuses.find(s => s.value === status);
    return statusObj ? statusObj.label : status.toString();
  }

  getTypeLabel(type: CustomizationType): string {
    const typeObj = this.customizationTypes.find(t => t.value === type);
    return typeObj ? typeObj.label : type.toString();
  }

  getStatusColor(status: CustomizationStatus): string {
    switch (status) {
      case CustomizationStatus.Draft: return '';
      case CustomizationStatus.PendingApproval: return 'accent';
      case CustomizationStatus.Approved: return 'primary';
      case CustomizationStatus.Rejected: return 'warn';
      case CustomizationStatus.Obsolete: return '';
      default: return '';
    }
  }

  canEdit(customization: OemCustomization): boolean {
    return customization.status === CustomizationStatus.Draft;
  }

  canSubmitForApproval(customization: OemCustomization): boolean {
    return customization.status === CustomizationStatus.Draft;
  }

  canApprove(customization: OemCustomization): boolean {
    return customization.status === CustomizationStatus.PendingApproval;
  }
}