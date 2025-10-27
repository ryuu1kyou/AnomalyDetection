import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  OemMasterDto,
  CreateOemMasterDto,
  UpdateOemMasterDto,
  OemFeatureDto,
  PagedResultDto,
  PagedAndSortedResultRequestDto,
  ListResultDto
} from '../models/tenant.model';

export interface CreateOemFeatureDto {
  featureName: string;
  featureValue: string;
  isEnabled?: boolean;
}

export interface UpdateOemFeatureDto {
  featureValue: string;
  isEnabled: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class OemMasterService {
  private readonly apiUrl = '/api/app/oem-master';

  constructor(private http: HttpClient) {}

  // OEM Master CRUD operations
  getList(input: PagedAndSortedResultRequestDto): Observable<PagedResultDto<OemMasterDto>> {
    return this.http.get<PagedResultDto<OemMasterDto>>(`${this.apiUrl}`, { params: input as any });
  }

  get(id: string): Observable<OemMasterDto> {
    return this.http.get<OemMasterDto>(`${this.apiUrl}/${id}`);
  }

  create(input: CreateOemMasterDto): Observable<OemMasterDto> {
    return this.http.post<OemMasterDto>(`${this.apiUrl}`, input);
  }

  update(id: string, input: UpdateOemMasterDto): Observable<OemMasterDto> {
    return this.http.put<OemMasterDto>(`${this.apiUrl}/${id}`, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  activate(id: string): Observable<OemMasterDto> {
    return this.http.post<OemMasterDto>(`${this.apiUrl}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<OemMasterDto> {
    return this.http.post<OemMasterDto>(`${this.apiUrl}/${id}/deactivate`, {});
  }

  // Feature management
  addFeature(id: string, input: CreateOemFeatureDto): Observable<OemMasterDto> {
    return this.http.post<OemMasterDto>(`${this.apiUrl}/${id}/features`, input);
  }

  updateFeature(id: string, featureName: string, input: UpdateOemFeatureDto): Observable<OemMasterDto> {
    return this.http.put<OemMasterDto>(`${this.apiUrl}/${id}/features/${featureName}`, input);
  }

  removeFeature(id: string, featureName: string): Observable<OemMasterDto> {
    return this.http.delete<OemMasterDto>(`${this.apiUrl}/${id}/features/${featureName}`);
  }

  // Lookup methods
  getActiveOems(): Observable<ListResultDto<OemMasterDto>> {
    return this.http.get<ListResultDto<OemMasterDto>>(`${this.apiUrl}/active-oems`);
  }

  getByOemCode(oemCode: string): Observable<OemMasterDto> {
    return this.http.get<OemMasterDto>(`${this.apiUrl}/by-oem-code/${oemCode}`);
  }
}