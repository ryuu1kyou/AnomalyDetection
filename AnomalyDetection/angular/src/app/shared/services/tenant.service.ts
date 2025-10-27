import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  ExtendedTenantDto,
  CreateExtendedTenantDto,
  UpdateExtendedTenantDto,
  TenantSwitchDto,
  PagedResultDto,
  PagedAndSortedResultRequestDto,
  ListResultDto,
  TenantFeatureDto,
  CreateTenantFeatureDto,
  UpdateTenantFeatureDto
} from '../models/tenant.model';

export interface CreateTenantFeatureDto {
  featureName: string;
  featureValue: string;
  isEnabled?: boolean;
}

export interface UpdateTenantFeatureDto {
  featureValue: string;
  isEnabled: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  private readonly apiUrl = '/api/app/extended-tenant';
  private currentTenantSubject = new BehaviorSubject<TenantSwitchDto | null>(null);
  public currentTenant$ = this.currentTenantSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadCurrentTenant();
  }

  // Tenant CRUD operations
  getList(input: PagedAndSortedResultRequestDto): Observable<PagedResultDto<ExtendedTenantDto>> {
    return this.http.get<PagedResultDto<ExtendedTenantDto>>(`${this.apiUrl}`, { params: input as any });
  }

  get(id: string): Observable<ExtendedTenantDto> {
    return this.http.get<ExtendedTenantDto>(`${this.apiUrl}/${id}`);
  }

  create(input: CreateExtendedTenantDto): Observable<ExtendedTenantDto> {
    return this.http.post<ExtendedTenantDto>(`${this.apiUrl}`, input);
  }

  update(id: string, input: UpdateExtendedTenantDto): Observable<ExtendedTenantDto> {
    return this.http.put<ExtendedTenantDto>(`${this.apiUrl}/${id}`, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  activate(id: string): Observable<ExtendedTenantDto> {
    return this.http.post<ExtendedTenantDto>(`${this.apiUrl}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<ExtendedTenantDto> {
    return this.http.post<ExtendedTenantDto>(`${this.apiUrl}/${id}/deactivate`, {});
  }

  // Feature management
  addFeature(id: string, input: CreateTenantFeatureDto): Observable<ExtendedTenantDto> {
    return this.http.post<ExtendedTenantDto>(`${this.apiUrl}/${id}/features`, input);
  }

  updateFeature(id: string, featureName: string, input: UpdateTenantFeatureDto): Observable<ExtendedTenantDto> {
    return this.http.put<ExtendedTenantDto>(`${this.apiUrl}/${id}/features/${featureName}`, input);
  }

  removeFeature(id: string, featureName: string): Observable<ExtendedTenantDto> {
    return this.http.delete<ExtendedTenantDto>(`${this.apiUrl}/${id}/features/${featureName}`);
  }

  // Tenant switching
  getAvailableTenants(): Observable<TenantSwitchDto[]> {
    return this.http.get<TenantSwitchDto[]>(`${this.apiUrl}/available-tenants`);
  }

  getCurrentTenant(): Observable<TenantSwitchDto> {
    return this.http.get<TenantSwitchDto>(`${this.apiUrl}/current-tenant`);
  }

  switchTenant(tenantId?: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/switch-tenant`, { tenantId }).pipe(
      tap(() => {
        // Reload current tenant after switching
        this.loadCurrentTenant();
        // Reload the page to apply tenant context
        window.location.reload();
      })
    );
  }

  // Lookup methods
  getActiveTenants(): Observable<ListResultDto<ExtendedTenantDto>> {
    return this.http.get<ListResultDto<ExtendedTenantDto>>(`${this.apiUrl}/active-tenants`);
  }

  getByName(name: string): Observable<ExtendedTenantDto> {
    return this.http.get<ExtendedTenantDto>(`${this.apiUrl}/by-name/${name}`);
  }

  // Current tenant management
  private loadCurrentTenant(): void {
    this.getCurrentTenant().subscribe({
      next: (tenant) => {
        this.currentTenantSubject.next(tenant);
      },
      error: (error) => {
        console.error('Failed to load current tenant:', error);
        this.currentTenantSubject.next(null);
      }
    });
  }

  getCurrentTenantValue(): TenantSwitchDto | null {
    return this.currentTenantSubject.value;
  }
}