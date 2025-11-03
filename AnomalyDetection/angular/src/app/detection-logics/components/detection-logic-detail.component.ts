import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, distinctUntilChanged, map, takeUntil } from 'rxjs';
import { DetectionLogicService } from '../services/detection-logic.service';
import {
  CanAnomalyDetectionLogicDto,
  CanSignalMappingDto,
  DetectionLogicStatus,
  DetectionParameterDto,
  OemCodeDto,
  getAsilLevelLabel,
  getCanSystemTypeLabel,
  getDetectionLogicStatusLabel,
  getDetectionTypeLabel,
  getParameterDataTypeLabel,
  getSharingLevelLabel,
} from '../models/detection-logic.model';

@Component({
  selector: 'app-detection-logic-detail',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './detection-logic-detail.component.html',
  styleUrls: ['./detection-logic-detail.component.scss'],
})
export class DetectionLogicDetailComponent implements OnInit, OnDestroy {
  protected logic?: CanAnomalyDetectionLogicDto;
  protected isLoading = true;
  protected errorMessage?: string;

  protected readonly statusClassMap: Record<number, string> = {
    [DetectionLogicStatus.Draft]: 'status-pill--draft',
    [DetectionLogicStatus.PendingApproval]: 'status-pill--pending',
    [DetectionLogicStatus.Approved]: 'status-pill--approved',
    [DetectionLogicStatus.Rejected]: 'status-pill--rejected',
    [DetectionLogicStatus.Deprecated]: 'status-pill--deprecated',
  };

  protected readonly getDetectionTypeLabel = getDetectionTypeLabel;
  protected readonly getDetectionLogicStatusLabel = getDetectionLogicStatusLabel;
  protected readonly getSharingLevelLabel = getSharingLevelLabel;
  protected readonly getAsilLevelLabel = getAsilLevelLabel;
  protected readonly getParameterDataTypeLabel = getParameterDataTypeLabel;
  protected readonly getCanSystemTypeLabel = getCanSystemTypeLabel;

  private readonly detectionLogicService = inject(DetectionLogicService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        map(params => params.get('id')),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(id => {
        if (!id) {
          this.logic = undefined;
          this.isLoading = false;
          this.errorMessage = '検出ロジック ID が指定されていません。';
          this.cdr.markForCheck();
          return;
        }

        this.loadDetail(id);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  protected reload(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.loadDetail(id);
  }

  protected formatOem(oem?: OemCodeDto | null): string {
    if (!oem) {
      return '未設定';
    }

    if (oem.name && oem.code) {
      return `${oem.name} (${oem.code})`;
    }

    return oem.name ?? oem.code ?? '未設定';
  }

  protected trackByParameterId(_: number, item: DetectionParameterDto): string {
    return item.id;
  }

  protected trackBySignalMappingId(_: number, item: CanSignalMappingDto): string {
    return `${item.canSignalId}-${item.signalRole}`;
  }

  private loadDetail(id: string): void {
    this.isLoading = true;
    this.errorMessage = undefined;
    this.cdr.markForCheck();

    this.detectionLogicService
      .getDetectionLogic(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: logic => {
          this.logic = {
            ...logic,
            parameters: logic.parameters ?? [],
            signalMappings: logic.signalMappings ?? [],
          };
          this.isLoading = false;
          this.cdr.markForCheck();
        },
        error: error => {
          this.logic = undefined;
          this.isLoading = false;
          this.errorMessage = error?.error?.message ?? '検出ロジックの取得に失敗しました。';
          this.cdr.markForCheck();
        },
      });
  }
}
