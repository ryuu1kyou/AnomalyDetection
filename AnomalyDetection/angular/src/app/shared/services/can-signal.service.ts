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
