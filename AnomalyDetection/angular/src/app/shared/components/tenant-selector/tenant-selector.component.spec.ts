import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';

import { TenantSelectorComponent } from './tenant-selector.component';
import { TenantService } from '../../services/tenant.service';
import { ExtendedTenantDto } from '../../models/tenant.model';

describe('TenantSelectorComponent', () => {
  let component: TenantSelectorComponent;
  let fixture: ComponentFixture<TenantSelectorComponent>;
  let mockStore: jasmine.SpyObj<Store>;
  let mockTenantService: jasmine.SpyObj<TenantService>;

  const mockTenants: ExtendedTenantDto[] = [
    {
      id: '1',
      name: 'Toyota',
      oemCode: 'TOYOTA',
      oemName: 'Toyota Motor Corporation',
      isActive: true,
      databaseConnectionString: 'test-connection',
      description: 'Toyota tenant',
      features: [],
      createdBy: 'system',
      updatedBy: 'system',
      creationTime: new Date(),
      lastModificationTime: new Date()
    },
    {
      id: '2',
      name: 'Honda',
      oemCode: 'HONDA',
      oemName: 'Honda Motor Co., Ltd.',
      isActive: true,
      databaseConnectionString: 'test-connection',
      description: 'Honda tenant',
      features: [],
      createdBy: 'system',
      updatedBy: 'system',
      creationTime: new Date(),
      lastModificationTime: new Date()
    }
  ];

  beforeEach(async () => {
    const storeSpy = jasmine.createSpyObj('Store', ['select', 'dispatch']);
    const tenantServiceSpy = jasmine.createSpyObj('TenantService', ['switchTenant']);

    await TestBed.configureTestingModule({
      declarations: [TenantSelectorComponent],
      imports: [
        MatSelectModule,
        MatFormFieldModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: Store, useValue: storeSpy },
        { provide: TenantService, useValue: tenantServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TenantSelectorComponent);
    component = fixture.componentInstance;
    mockStore = TestBed.inject(Store) as jasmine.SpyObj<Store>;
    mockTenantService = TestBed.inject(TenantService) as jasmine.SpyObj<TenantService>;

    // Setup store selectors
    mockStore.select.and.returnValue(of(mockTenants));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load tenants on init', () => {
    component.ngOnInit();
    expect(mockStore.select).toHaveBeenCalled();
    expect(component.tenants).toEqual(mockTenants);
  });

  it('should switch tenant when selection changes', () => {
    const selectedTenant = mockTenants[0];
    mockTenantService.switchTenant.and.returnValue(of(true));

    component.onTenantChange(selectedTenant.id!);

    expect(mockTenantService.switchTenant).toHaveBeenCalledWith(selectedTenant.id);
  });

  it('should display tenant names in the selector', () => {
    component.tenants = mockTenants;
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('Toyota');
    expect(compiled.textContent).toContain('Honda');
  });

  it('should handle empty tenant list', () => {
    mockStore.select.and.returnValue(of([]));
    component.ngOnInit();
    
    expect(component.tenants).toEqual([]);
  });

  it('should handle tenant switch error', () => {
    const selectedTenant = mockTenants[0];
    mockTenantService.switchTenant.and.returnValue(of(false));
    spyOn(console, 'error');

    component.onTenantChange(selectedTenant.id!);

    expect(console.error).toHaveBeenCalled();
  });

  it('should update selected tenant when current tenant changes', () => {
    const currentTenant = mockTenants[1];
    mockStore.select.and.returnValue(of(currentTenant));

    component.ngOnInit();

    expect(component.selectedTenantId).toBe(currentTenant.id);
  });

  it('should disable selector when no tenants available', () => {
    component.tenants = [];
    fixture.detectChanges();

    const selectElement = fixture.nativeElement.querySelector('mat-select');
    expect(selectElement).toBeTruthy();
  });
});