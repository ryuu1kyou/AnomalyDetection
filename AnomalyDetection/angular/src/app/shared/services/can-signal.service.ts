import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

export interface OemCodeDto {
  code: string;
  name: string;
}

export interface CanSignalDto {
  id: string;
  tenantId?: string;

  // Signal Identity
  signalName: string;
  canId: string;

  // Signal Specification
  startBit: number;
  length: number;
  dataType: number;
  minValue: number;
  maxValue: number;
  byteOrder: number;

  // Physical Value Conversion
  factor: number;
  offset: number;
  unit: string;

  // Signal Timing
  cycleTime: number;
  timeoutTime: number;

  // Entity Attributes
  systemType: number;
  description: string;
  oemCode: OemCodeDto;
  isStandard: boolean;
  version: string;
  effectiveDate?: string;
  status: number;

  // Metadata
  sourceDocument: string;
  notes: string;

  // トレサビ
  featureId?: string;
  decisionId?: string;

  // 資産共通化分類
  commonalityStatus: number;
  unknownResolutionDueDate?: string;

  // Audit
  creationTime?: string;
  creatorId?: string;
  lastModificationTime?: string;
  lastModifierId?: string;
  isDeleted?: boolean;

  /** @deprecated use length */
  bitLength?: number;
}

export interface CanSignalLookup {
  id: string;
  label: string;
  systemType: number;
}

export interface CreateCanSignalDto {
  signalName: string;
  canId: string;
  startBit: number;
  length: number;
  dataType: number;
  minValue: number;
  maxValue: number;
  byteOrder: number;
  factor: number;
  offset: number;
  unit: string;
  cycleTime: number;
  timeoutTime: number;
  systemType: number;
  description: string;
  oemCode: OemCodeDto;
  isStandard: boolean;
  effectiveDate?: string | null;
  sourceDocument: string;
  notes: string;
}

export interface UpdateCanSignalDto {
  signalName: string;
  canId: string;
  startBit: number;
  length: number;
  dataType: number;
  minValue: number;
  maxValue: number;
  byteOrder: number;
  factor: number;
  offset: number;
  unit: string;
  cycleTime: number;
  timeoutTime: number;
  systemType: number;
  description: string;
  isStandard: boolean;
  effectiveDate?: string | null;
  sourceDocument: string;
  notes: string;
  changeReason: string;
  featureId?: string | null;
  decisionId?: string | null;
  commonalityStatus?: number | null;
  unknownResolutionDueDate?: string | null;
}

@Injectable({ providedIn: 'root' })
export class CanSignalService {
  private readonly http = inject(HttpClient);
  private readonly apiBase = '/api/app/can-signals';

  getCanSignals(params?: {
    skipCount?: number;
    maxResultCount?: number;
    sorting?: string;
    filter?: string;
  }): Observable<PagedResult<CanSignalDto>> {
    let httpParams = new HttpParams();
    if (params?.skipCount !== undefined) {
      httpParams = httpParams.set('SkipCount', params.skipCount.toString());
    }
    if (params?.maxResultCount !== undefined) {
      httpParams = httpParams.set('MaxResultCount', params.maxResultCount.toString());
    }
    if (params?.sorting) {
      httpParams = httpParams.set('Sorting', params.sorting);
    }
    if (params?.filter) {
      httpParams = httpParams.set('Filter', params.filter);
    }

    return this.http.get<PagedResult<CanSignalDto>>(this.apiBase, { params: httpParams });
  }

  searchCanSignals(term: string): Observable<CanSignalLookup[]> {
    const params = new HttpParams()
      .set('Filter', term ?? '')
      .set('SkipCount', 0)
      .set('MaxResultCount', 20);

    return this.http.get<PagedResult<CanSignalDto>>(this.apiBase, { params }).pipe(
      map(result =>
        (result.items ?? []).map(item => ({
          id: item.id,
          label: `${item.signalName} (${item.canId})`,
          systemType: item.systemType,
        }))
      )
    );
  }

  getCanSignal(id: string): Observable<CanSignalDto> {
    return this.http.get<CanSignalDto>(`${this.apiBase}/${id}`);
  }

  createCanSignal(dto: CreateCanSignalDto): Observable<CanSignalDto> {
    return this.http.post<CanSignalDto>(this.apiBase, dto);
  }

  updateCanSignal(id: string, dto: UpdateCanSignalDto): Observable<CanSignalDto> {
    return this.http.put<CanSignalDto>(`${this.apiBase}/${id}`, dto);
  }

  deleteCanSignal(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBase}/${id}`);
  }
}
