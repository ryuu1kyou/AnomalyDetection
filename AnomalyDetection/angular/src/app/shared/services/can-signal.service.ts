import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

export interface CanSignalDto {
  id: string;
  signalName: string;
  canId: string;
  systemType: number;
  description: string;
  dataType?: number;
  byteOrder?: number;
  startBit?: number;
  bitLength?: number;
  factor?: number;
  offset?: number;
  minValue?: number;
  maxValue?: number;
  unit?: string;
  creationTime?: string;
  creatorId?: string;
  lastModificationTime?: string;
  lastModifierId?: string;
}

export interface CanSignalLookup {
  id: string;
  label: string;
  systemType: number;
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
}
