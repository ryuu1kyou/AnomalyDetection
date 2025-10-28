import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-can-signal-create',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>新規CAN信号作成</h2>
      <p>CAN信号の作成機能は実装中です。</p>
    </div>
  `
})
export class CanSignalCreateComponent {
}