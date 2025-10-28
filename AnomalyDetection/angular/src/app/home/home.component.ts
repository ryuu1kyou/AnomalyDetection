import { Component, inject } from '@angular/core';
import { AuthService, LocalizationPipe } from '@abp/ng.core';
import { Router } from '@angular/router';
import { PermissionService } from '../shared/services/permission.service';
import { HasPermissionDirective } from '../shared/directives/has-permission.directive';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  imports: [LocalizationPipe, HasPermissionDirective, CommonModule]
})
export class HomeComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  public permissionService = inject(PermissionService);

  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated
  }

  login() {
    this.authService.navigateToLogin();
  }

  navigateTo(path: string) {
    this.router.navigate([path]);
  }

  get moduleCards() {
    return [
      {
        title: 'CAN信号管理',
        description: 'CAN信号の定義と仕様を管理します',
        icon: 'fas fa-signal',
        path: '/can-signals',
        permission: this.permissionService.permissions.CAN_SIGNALS.DEFAULT,
        color: 'primary'
      },
      {
        title: '異常検出ロジック',
        description: '異常検出アルゴリズムを作成・管理します',
        icon: 'fas fa-brain',
        path: '/detection-logics',
        permission: this.permissionService.permissions.DETECTION_LOGICS.DEFAULT,
        color: 'success'
      },
      {
        title: '検出結果',
        description: '異常検出の実行結果を確認・分析します',
        icon: 'fas fa-exclamation-triangle',
        path: '/detection-results',
        permission: this.permissionService.permissions.DETECTION_RESULTS.DEFAULT,
        color: 'warning'
      },
      {
        title: 'プロジェクト管理',
        description: '異常検出プロジェクトを管理します',
        icon: 'fas fa-project-diagram',
        path: '/projects',
        permission: this.permissionService.permissions.PROJECTS.DEFAULT,
        color: 'info'
      },
      {
        title: 'OEMトレーサビリティ',
        description: 'OEM固有のカスタマイズと承認を管理します',
        icon: 'fas fa-history',
        path: '/oem-traceability',
        permission: this.permissionService.permissions.OEM_TRACEABILITY.DEFAULT,
        color: 'secondary'
      },
      {
        title: '類似パターン検索',
        description: '類似CAN信号と検出パターンを検索します',
        icon: 'fas fa-search',
        path: '/similar-comparison',
        permission: this.permissionService.permissions.SIMILAR_COMPARISON.DEFAULT,
        color: 'dark'
      },
      {
        title: '異常分析',
        description: '異常検出パターンと精度を分析します',
        icon: 'fas fa-chart-bar',
        path: '/anomaly-analysis',
        permission: this.permissionService.permissions.ANOMALY_ANALYSIS.DEFAULT,
        color: 'danger'
      }
    ];
  }
}
