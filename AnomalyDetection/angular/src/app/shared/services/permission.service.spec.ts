import { TestBed } from '@angular/core/testing';
import { PermissionService } from './permission.service';
import { ConfigStateService, PermissionService as AbpPermissionService } from '@abp/ng.core';

describe('PermissionService', () => {
  let service: PermissionService;
  let mockConfigStateService: jasmine.SpyObj<ConfigStateService>;
  let mockAbpPermissionService: jasmine.SpyObj<AbpPermissionService>;

  beforeEach(() => {
    const configStateSpy = jasmine.createSpyObj('ConfigStateService', ['getAll$']);
    const abpPermissionSpy = jasmine.createSpyObj('AbpPermissionService', ['getGrantedPolicy', 'getGrantedPolicy$']);

    TestBed.configureTestingModule({
      providers: [
        PermissionService,
        { provide: ConfigStateService, useValue: configStateSpy },
        { provide: AbpPermissionService, useValue: abpPermissionSpy }
      ]
    });

    service = TestBed.inject(PermissionService);
    mockConfigStateService = TestBed.inject(ConfigStateService) as jasmine.SpyObj<ConfigStateService>;
    mockAbpPermissionService = TestBed.inject(AbpPermissionService) as jasmine.SpyObj<AbpPermissionService>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should check permission using ABP permission service', () => {
    const permission = 'AnomalyDetection.CanSignals';
    mockAbpPermissionService.getGrantedPolicy.and.returnValue(true);

    const result = service.hasPermission(permission);

    expect(result).toBe(true);
    expect(mockAbpPermissionService.getGrantedPolicy).toHaveBeenCalledWith(permission);
  });

  it('should check if user has any permission', () => {
    const permissions = ['AnomalyDetection.CanSignals', 'AnomalyDetection.DetectionLogics'];
    mockAbpPermissionService.getGrantedPolicy.and.returnValues(false, true);

    const result = service.hasAnyPermission(permissions);

    expect(result).toBe(true);
  });

  it('should check if user has all permissions', () => {
    const permissions = ['AnomalyDetection.CanSignals', 'AnomalyDetection.DetectionLogics'];
    mockAbpPermissionService.getGrantedPolicy.and.returnValues(true, true);

    const result = service.hasAllPermissions(permissions);

    expect(result).toBe(true);
  });

  it('should return false when user does not have all permissions', () => {
    const permissions = ['AnomalyDetection.CanSignals', 'AnomalyDetection.DetectionLogics'];
    mockAbpPermissionService.getGrantedPolicy.and.returnValues(true, false);

    const result = service.hasAllPermissions(permissions);

    expect(result).toBe(false);
  });
});