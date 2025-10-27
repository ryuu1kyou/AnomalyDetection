import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';

import { TenantService } from './tenant.service';
import { ExtendedTenantDto } from '../models/tenant.model';
import * as TenantActions from '../../store/tenant/tenant.actions';

describe('TenantService', () => {
  let service: TenantService;
  let httpMock: HttpTestingController;
  let mockStore: jasmine.SpyObj<Store>;

  const mockTenant: ExtendedTenantDto = {
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
  };

  beforeEach(() => {
    const storeSpy = jasmine.createSpyObj('Store', ['select', 'dispatch']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        TenantService,
        { provide: Store, useValue: storeSpy }
      ]
    });

    service = TestBed.inject(TenantService);
    httpMock = TestBed.inject(HttpTestingController);
    mockStore = TestBed.inject(Store) as jasmine.SpyObj<Store>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get current tenant from store', () => {
    mockStore.select.and.returnValue(of(mockTenant));

    service.getCurrentTenant().subscribe(tenant => {
      expect(tenant).toEqual(mockTenant);
    });

    expect(mockStore.select).toHaveBeenCalled();
  });

  it('should get all tenants from store', () => {
    const mockTenants = [mockTenant];
    mockStore.select.and.returnValue(of(mockTenants));

    service.getAllTenants().subscribe(tenants => {
      expect(tenants).toEqual(mockTenants);
    });

    expect(mockStore.select).toHaveBeenCalled();
  });

  it('should switch tenant successfully', () => {
    const tenantId = '1';
    
    service.switchTenant(tenantId).subscribe(result => {
      expect(result).toBe(true);
    });

    const req = httpMock.expectOne(`/api/app/tenant/switch/${tenantId}`);
    expect(req.request.method).toBe('POST');
    req.flush({ success: true });

    expect(mockStore.dispatch).toHaveBeenCalledWith(
      TenantActions.switchTenant({ tenantId })
    );
  });

  it('should handle switch tenant error', () => {
    const tenantId = '1';
    
    service.switchTenant(tenantId).subscribe(result => {
      expect(result).toBe(false);
    });

    const req = httpMock.expectOne(`/api/app/tenant/switch/${tenantId}`);
    req.error(new ErrorEvent('Network error'));
  });

  it('should load tenants and dispatch action', () => {
    const mockTenants = [mockTenant];
    
    service.loadTenants();

    const req = httpMock.expectOne('/api/app/tenant');
    expect(req.request.method).toBe('GET');
    req.flush({ items: mockTenants, totalCount: 1 });

    expect(mockStore.dispatch).toHaveBeenCalledWith(
      TenantActions.loadTenantsSuccess({ tenants: mockTenants })
    );
  });

  it('should handle load tenants error', () => {
    service.loadTenants();

    const req = httpMock.expectOne('/api/app/tenant');
    req.error(new ErrorEvent('Network error'));

    expect(mockStore.dispatch).toHaveBeenCalledWith(
      TenantActions.loadTenantsFailure({ error: jasmine.any(String) })
    );
  });

  it('should get tenant by id', () => {
    const tenantId = '1';
    
    service.getTenantById(tenantId).subscribe(tenant => {
      expect(tenant).toEqual(mockTenant);
    });

    const req = httpMock.expectOne(`/api/app/tenant/${tenantId}`);
    expect(req.request.method).toBe('GET');
    req.flush(mockTenant);
  });

  it('should create new tenant', () => {
    const createTenantDto = {
      name: 'Honda',
      oemCode: 'HONDA',
      oemName: 'Honda Motor Co., Ltd.',
      description: 'Honda tenant'
    };
    
    service.createTenant(createTenantDto).subscribe(tenant => {
      expect(tenant).toEqual(mockTenant);
    });

    const req = httpMock.expectOne('/api/app/tenant');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(createTenantDto);
    req.flush(mockTenant);
  });

  it('should update existing tenant', () => {
    const tenantId = '1';
    const updateTenantDto = {
      name: 'Toyota Updated',
      description: 'Updated Toyota tenant'
    };
    
    service.updateTenant(tenantId, updateTenantDto).subscribe(tenant => {
      expect(tenant).toEqual(mockTenant);
    });

    const req = httpMock.expectOne(`/api/app/tenant/${tenantId}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(updateTenantDto);
    req.flush(mockTenant);
  });

  it('should delete tenant', () => {
    const tenantId = '1';
    
    service.deleteTenant(tenantId).subscribe();

    const req = httpMock.expectOne(`/api/app/tenant/${tenantId}`);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });

  it('should check if user can switch tenant', () => {
    expect(service.canSwitchTenant()).toBe(true);
  });

  it('should get tenant features', () => {
    const tenantId = '1';
    const mockFeatures = [
      { featureName: 'AdvancedAnalytics', featureValue: 'true' }
    ];
    
    service.getTenantFeatures(tenantId).subscribe(features => {
      expect(features).toEqual(mockFeatures);
    });

    const req = httpMock.expectOne(`/api/app/tenant/${tenantId}/features`);
    expect(req.request.method).toBe('GET');
    req.flush(mockFeatures);
  });

  it('should update tenant features', () => {
    const tenantId = '1';
    const features = [
      { featureName: 'AdvancedAnalytics', featureValue: 'false' }
    ];
    
    service.updateTenantFeatures(tenantId, features).subscribe();

    const req = httpMock.expectOne(`/api/app/tenant/${tenantId}/features`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(features);
    req.flush({});
  });

  it('should validate tenant switch permissions', () => {
    const tenantId = '1';
    
    service.validateTenantSwitchPermissions(tenantId).subscribe(canSwitch => {
      expect(canSwitch).toBe(true);
    });

    const req = httpMock.expectOne(`/api/app/tenant/${tenantId}/can-switch`);
    expect(req.request.method).toBe('GET');
    req.flush({ canSwitch: true });
  });

  it('should get tenant statistics', () => {
    const tenantId = '1';
    const mockStats = {
      signalCount: 150,
      logicCount: 25,
      resultCount: 1000,
      lastActivity: new Date()
    };
    
    service.getTenantStatistics(tenantId).subscribe(stats => {
      expect(stats).toEqual(mockStats);
    });

    const req = httpMock.expectOne(`/api/app/tenant/${tenantId}/statistics`);
    expect(req.request.method).toBe('GET');
    req.flush(mockStats);
  });
});