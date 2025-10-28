import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-detection-logic-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>異常検出ロジック一覧</h2>
      <p>異常検出ロジックの一覧表示機能は実装中です。</p>
    </div>
  `
})
export class DetectionLogicListComponent {
}