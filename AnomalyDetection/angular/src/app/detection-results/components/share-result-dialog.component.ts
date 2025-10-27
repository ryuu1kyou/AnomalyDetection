import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';

import { 
  AnomalyDetectionResult, 
  SharingLevel, 
  ShareDetectionResultDto 
} from '../models/detection-result.model';

export interface ShareResultDialogData {
  result: AnomalyDetectionResult;
}

@Component({
  selector: 'app-share-result-dialog',
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
    MatDividerModule
  ],
  template: `
    <div class="share-dialog">
      <h2 mat-dialog-title>
        <mat-icon>share</mat-icon>
        異常検出結果の共有
      </h2>
      
      <mat-dialog-content>
        <!-- Result Summary -->
        <div class="result-summary">
          <h3>共有対象の結果</h3>
          <div class="summary-card">
            <div class="summary-row">
              <span class="label">信号名:</span>
              <span class="value">{{ data.result.signalName }}</span>
            </div>
            <div class="summary-row">
              <span class="label">CAN ID:</span>
              <span class="value can-id">{{ data.result.canId }}</span>
            </div>
            <div class="summary-row">
              <span class="label">検出時刻:</span>
              <span class="value">{{ data.result.detectedAt | date:'yyyy/MM/dd HH:mm:ss' }}</span>
            </div>
            <div class="summary-row">
              <span class="label">異常レベル:</span>
              <mat-chip [class]="'level-' + getAnomalyLevelClass(data.result.anomalyLevel)">
                {{ getAnomalyLevelText(data.result.anomalyLevel) }}
              </mat-chip>
            </div>
          </div>
        </div>

        <mat-divider></mat-divider>

        <!-- Sharing Form -->
        <form [formGroup]="shareForm" class="share-form">
          <h3>共有設定</h3>
          
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>共有レベル</mat-label>
            <mat-select formControlName="sharingLevel" required>
              <mat-option [value]="SharingLevel.OemPartner">
                <div class="sharing-option">
                  <div class="option-title">OEMパートナー</div>
                  <div class="option-description">同じOEMグループ内でのみ共有</div>
                </div>
              </mat-option>
              <mat-option [value]="SharingLevel.Industry">
                <div class="sharing-option">
                  <div class="option-title">業界共通</div>
                  <div class="option-description">自動車業界全体で共有</div>
                </div>
              </mat-option>
              <mat-option [value]="SharingLevel.Public">
                <div class="sharing-option">
                  <div class="option-title">パブリック</div>
                  <div class="option-description">一般公開（匿名化）</div>
                </div>
              </mat-option>
            </mat-select>
            <mat-hint>共有レベルによって閲覧可能な範囲が決まります</mat-hint>
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>共有理由</mat-label>
            <textarea matInput formControlName="shareReason" rows="3" 
                     placeholder="この結果を共有する理由を入力してください（例：業界共通の課題として情報共有）"></textarea>
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>補足メモ</mat-label>
            <textarea matInput formControlName="shareNotes" rows="2" 
                     placeholder="共有時の補足情報があれば入力してください"></textarea>
          </mat-form-field>

          <div class="checkbox-section">
            <mat-checkbox formControlName="requireApproval">
              承認プロセスを必要とする
            </mat-checkbox>
            <div class="checkbox-hint">
              チェックすると、管理者の承認後に共有されます
            </div>
          </div>

          <!-- Sharing Impact Warning -->
          <div class="warning-section" *ngIf="getSelectedSharingLevel() !== null">
            <mat-icon class="warning-icon">warning</mat-icon>
            <div class="warning-content">
              <div class="warning-title">共有時の注意事項</div>
              <div class="warning-text">
                {{ getSharingWarningText() }}
              </div>
            </div>
          </div>

          <!-- Data Anonymization Info -->
          <div class="anonymization-info" *ngIf="getSelectedSharingLevel() === SharingLevel.Public">
            <h4>匿名化される情報</h4>
            <ul>
              <li>OEM固有の識別情報</li>
              <li>車両モデル名</li>
              <li>プロジェクト固有の情報</li>
              <li>個人識別可能な情報</li>
            </ul>
            <h4>共有される情報</h4>
            <ul>
              <li>異常検出パターン</li>
              <li>検出ロジックの種類</li>
              <li>信号の技術的特性</li>
              <li>解決方法（匿名化済み）</li>
            </ul>
          </div>
        </form>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">キャンセル</button>
        <button mat-raised-button color="primary" 
                [disabled]="shareForm.invalid"
                (click)="onShare()">
          {{ getSelectedSharingLevel() === SharingLevel.Public ? '匿名化して共有' : '共有する' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styleUrls: ['./share-result-dialog.component.scss']
})
export class ShareResultDialogComponent implements OnInit {
  SharingLevel = SharingLevel;
  shareForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ShareResultDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ShareResultDialogData
  ) {
    this.shareForm = this.createForm();
  }

  ngOnInit(): void {
    // Set default values based on result characteristics
    this.setDefaultSharingLevel();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      sharingLevel: [SharingLevel.OemPartner, Validators.required],
      shareReason: ['', [Validators.required, Validators.minLength(10)]],
      shareNotes: [''],
      requireApproval: [true]
    });
  }

  private setDefaultSharingLevel(): void {
    // Set default sharing level based on anomaly level and other factors
    const result = this.data.result;
    
    if (result.anomalyLevel >= 4) { // Critical or Fatal
      this.shareForm.patchValue({
        sharingLevel: SharingLevel.Industry,
        shareReason: '重要度の高い異常として業界共有',
        requireApproval: true
      });
    } else {
      this.shareForm.patchValue({
        sharingLevel: SharingLevel.OemPartner,
        requireApproval: false
      });
    }
  }

  getSelectedSharingLevel(): SharingLevel | null {
    return this.shareForm.get('sharingLevel')?.value || null;
  }

  getSharingWarningText(): string {
    const level = this.getSelectedSharingLevel();
    
    switch (level) {
      case SharingLevel.OemPartner:
        return 'この結果は同じOEMグループ内の関係者のみが閲覧できます。機密情報が含まれていないことを確認してください。';
      case SharingLevel.Industry:
        return 'この結果は自動車業界全体で共有されます。競合他社も閲覧可能になるため、機密情報が含まれていないことを確認してください。';
      case SharingLevel.Public:
        return 'この結果は一般公開されます。すべての識別可能な情報は自動的に匿名化されますが、技術的な詳細は公開されます。';
      default:
        return '';
    }
  }

  onShare(): void {
    if (this.shareForm.valid) {
      const shareData: ShareDetectionResultDto = {
        sharingLevel: this.shareForm.value.sharingLevel,
        shareReason: this.shareForm.value.shareReason,
        shareNotes: this.shareForm.value.shareNotes,
        requireApproval: this.shareForm.value.requireApproval
      };
      
      this.dialogRef.close(shareData);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  // Helper methods (same as other components)
  getAnomalyLevelText(level: number): string {
    const texts: Record<number, string> = {
      1: 'Info',
      2: 'Warning', 
      3: 'Error',
      4: 'Critical',
      5: 'Fatal'
    };
    return texts[level] || 'Unknown';
  }

  getAnomalyLevelClass(level: number): string {
    const classes: Record<number, string> = {
      1: 'info',
      2: 'warning',
      3: 'error', 
      4: 'critical',
      5: 'fatal'
    };
    return classes[level] || 'default';
  }
}