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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, distinctUntilChanged, map, takeUntil } from 'rxjs';
import { DetectionLogicService } from '../services/detection-logic.service';
import {
  CanAnomalyDetectionLogicDto,
  DetectionLogicStatus,
  OemCodeDto,
  getAsilLevelLabel,
  getDetectionLogicStatusLabel,
  getDetectionTypeLabel,
  getSharingLevelLabel,
} from '../models/detection-logic.model';

@Component({
  selector: 'app-detection-logic-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './detection-logic-list.component.html',
  styleUrls: ['./detection-logic-list.component.scss'],
})
export class DetectionLogicListComponent implements OnInit, OnDestroy {
  protected detectionLogics: CanAnomalyDetectionLogicDto[] = [];
  protected totalCount = 0;
  protected isLoading = false;
  protected errorMessage?: string;
  protected highlightedLogicId?: string;

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

  private readonly detectionLogicService = inject(DetectionLogicService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  private readonly destroy$ = new Subject<void>();
  private readonly pageSize = 20;
  private highlightedInjected = false;
  private hasLoadedOnce = false;

  constructor() {
    this.route.queryParamMap
      .pipe(
        map(params => params.get('created') ?? undefined),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(id => {
        this.highlightedLogicId = id;
        this.highlightedInjected = !id;
        this.ensureHighlightedPresence();
        this.cdr.markForCheck();
      });
  }

  ngOnInit(): void {
    this.loadDetectionLogics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  protected createLogic(): void {
    void this.router.navigate(['/detection-logics/create']);
  }

  protected viewDetail(logic: CanAnomalyDetectionLogicDto): void {
    void this.router.navigate(['/detection-logics', logic.id]);
  }

  protected reload(): void {
    this.loadDetectionLogics();
  }

  protected dismissCreatedBanner(): void {
    if (!this.highlightedLogicId) {
      return;
    }

    this.highlightedLogicId = undefined;
    this.highlightedInjected = true;
    this.removeCreatedQueryParam();
    this.cdr.markForCheck();
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

  protected trackByLogicId(_: number, item: CanAnomalyDetectionLogicDto): string {
    return item.id;
  }

  private loadDetectionLogics(): void {
    this.isLoading = true;
    this.errorMessage = undefined;
    this.highlightedInjected = !this.highlightedLogicId;
    this.hasLoadedOnce = false;
    this.cdr.markForCheck();

    this.detectionLogicService
      .getDetectionLogics({
        skipCount: 0,
        maxResultCount: this.pageSize,
        sorting: 'CreationTime DESC',
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.detectionLogics = response.items;
          this.totalCount = response.totalCount;
          this.isLoading = false;
          this.hasLoadedOnce = true;
          this.cdr.markForCheck();
          this.ensureHighlightedPresence();
        },
        error: error => {
          this.detectionLogics = [];
          this.isLoading = false;
          this.hasLoadedOnce = true;
          this.errorMessage = error?.error?.message ?? '検出ロジック一覧の取得に失敗しました。';
          this.cdr.markForCheck();
        },
      });
  }

  private ensureHighlightedPresence(): void {
    const highlightId = this.highlightedLogicId;
    if (!highlightId || this.highlightedInjected || this.isLoading || !this.hasLoadedOnce) {
      return;
    }

    const exists = this.detectionLogics.some(item => item.id === highlightId);
    if (exists) {
      this.highlightedInjected = true;
      return;
    }

    this.detectionLogicService
      .getDetectionLogic(highlightId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: detail => {
          this.detectionLogics = [detail, ...this.detectionLogics];
          this.highlightedInjected = true;
          this.totalCount = Math.max(this.totalCount, this.detectionLogics.length);
          this.cdr.markForCheck();
        },
        error: () => {
          this.highlightedInjected = true;
          this.highlightedLogicId = undefined;
          this.removeCreatedQueryParam();
          this.cdr.markForCheck();
        },
      });
  }

  private removeCreatedQueryParam(): void {
    const currentParams = { ...this.route.snapshot.queryParams };
    if (!('created' in currentParams)) {
      return;
    }

    delete currentParams['created'];
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: currentParams,
      replaceUrl: true,
    });
  }
}
