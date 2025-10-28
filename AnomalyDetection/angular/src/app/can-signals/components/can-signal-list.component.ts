import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-can-signal-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>CAN信号一覧</h2>
      <p>CAN信号の一覧表示機能は実装中です。</p>
    </div>
  `
})
export class CanSignalListComponent {
}