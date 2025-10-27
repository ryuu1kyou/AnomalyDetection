import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, takeUntil } from 'rxjs';
import { TenantService } from '../../services/tenant.service';
import { TenantSwitchDto } from '../../models/tenant.model';

@Component({
  selector: 'app-tenant-selector',
  standalone: true,
  imports: [
    CommonModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="tenant-selector-container">
      <mat-form-field appearance="outline" class="tenant-select">
        <mat-label>テナント選択</mat-label>
        <mat-select 
          [value]="currentTenant?.tenantId || null"
          (selectionChange)="onTenantChange($event.value)"
          [disabled]="isLoading">
          <mat-option 
            *ngFor="let tenant of availableTenants" 
            [value]="tenant.tenantId">
            <div class="tenant-option">
              <span class="tenant-name">{{ tenant.tenantName }}</span>
              <span class="tenant-code">({{ tenant.oemCode }})</span>
            </div>
          </mat-option>
        </mat-select>
      </mat-form-field>
      
      <button 
        mat-icon-button 
        (click)="refreshTenants()"
        [disabled]="isLoading"
        matTooltip="テナント一覧を更新">
        <mat-icon>refresh</mat-icon>
      </button>
      
      <mat-spinner 
        *ngIf="isLoading" 
        diameter="20" 
        class="loading-spinner">
      </mat-spinner>
    </div>
  `,
  styles: [`
    .tenant-selector-container {
      display: flex;
      align-items: center;
      gap: 8px;
      min-width: 250px;
    }

    .tenant-select {
      flex: 1;
      min-width: 200px;
    }

    .tenant-option {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .tenant-name {
      font-weight: 500;
    }

    .tenant-code {
      color: #666;
      font-size: 0.9em;
    }

    .loading-spinner {
      margin-left: 8px;
    }

    ::ng-deep .mat-mdc-form-field {
      .mat-mdc-text-field-wrapper {
        padding: 0 12px;
      }
    }
  `]
})
export class TenantSelectorComponent implements OnInit, OnDestroy {
  availableTenants: TenantSwitchDto[] = [];
  currentTenant: TenantSwitchDto | null = null;
  isLoading = false;
  private destroy$ = new Subject<void>();

  constructor(private tenantService: TenantService) {}

  ngOnInit(): void {
    this.loadCurrentTenant();
    this.loadAvailableTenants();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadCurrentTenant(): void {
    this.tenantService.currentTenant$
      .pipe(takeUntil(this.destroy$))
      .subscribe(tenant => {
        this.currentTenant = tenant;
      });
  }

  private loadAvailableTenants(): void {
    this.isLoading = true;
    this.tenantService.getAvailableTenants()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (tenants) => {
          this.availableTenants = tenants;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Failed to load available tenants:', error);
          this.isLoading = false;
        }
      });
  }

  onTenantChange(tenantId: string | null): void {
    if (this.currentTenant?.tenantId === tenantId) {
      return; // No change
    }

    this.isLoading = true;
    this.tenantService.switchTenant(tenantId || undefined)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          // The page will reload automatically in the service
        },
        error: (error) => {
          console.error('Failed to switch tenant:', error);
          this.isLoading = false;
          // Reset selection to current tenant
          // The mat-select will automatically revert due to the binding
        }
      });
  }

  refreshTenants(): void {
    this.loadAvailableTenants();
  }
}