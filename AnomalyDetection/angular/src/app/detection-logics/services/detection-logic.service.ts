import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  AsilLevel,
  CanAnomalyDetectionLogicDto,
  DetectionLogicStatus,
  DetectionType,
  GetDetectionLogicsQuery,
  PagedResultDto,
  SharingLevel,
} from '../models/detection-logic.model';

type TemplateParameterType = 'string' | 'number' | 'integer' | 'boolean';

export interface TemplateParameterDefinition {
  name: string;
  type: TemplateParameterType;
  description: string;
  required: boolean;
  defaultValue?: unknown;
  minValue?: number;
  maxValue?: number;
  minLength?: number;
  maxLength?: number;
  allowedValues?: string[];
}

export interface DetectionTemplateSummary {
  type: number;
  name: string;
  description: string;
  detectionType: DetectionType;
  defaultParameters: Record<string, unknown>;
  parameterDefinitions: TemplateParameterDefinition[];
}

export interface CreateDetectionLogicFromTemplateRequest {
  logicName: string;
  canSignalId: string;
  templateParameters: Record<string, unknown>;
}

@Injectable({ providedIn: 'root' })
export class DetectionLogicService {
  private readonly http = inject(HttpClient);
  private readonly apiBase = '/api/app/can-anomaly-detection-logics';

  getDetectionLogics(
    query: GetDetectionLogicsQuery = {}
  ): Observable<PagedResultDto<CanAnomalyDetectionLogicDto>> {
    const {
      skipCount = 0,
      maxResultCount = 20,
      sorting = 'CreationTime DESC',
      filter,
      detectionType,
      status,
      asilLevel,
      sharingLevel,
    } = query;

    let params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString())
      .set('Sorting', sorting);

    if (filter) {
      params = params.set('Filter', filter);
    }
    if (detectionType !== undefined && detectionType !== null) {
      params = params.set('DetectionType', detectionType.toString());
    }
    if (status !== undefined && status !== null) {
      params = params.set('Status', status.toString());
    }
    if (asilLevel !== undefined && asilLevel !== null) {
      params = params.set('AsilLevel', asilLevel.toString());
    }
    if (sharingLevel !== undefined && sharingLevel !== null) {
      params = params.set('SharingLevel', sharingLevel.toString());
    }

    return this.http
      .get<PagedResultDto<CanAnomalyDetectionLogicDto>>(this.apiBase, { params })
      .pipe(
        map(response => ({
          items: response?.items ?? [],
          totalCount: response?.totalCount ?? 0,
        }))
      );
  }

  getDetectionLogic(id: string): Observable<CanAnomalyDetectionLogicDto> {
    return this.http.get<CanAnomalyDetectionLogicDto>(`${this.apiBase}/${id}`).pipe(
      map(dto => ({
        ...dto,
        parameters: dto.parameters ?? [],
        signalMappings: dto.signalMappings ?? [],
      }))
    );
  }

  getTemplates(detectionType: DetectionType): Observable<DetectionTemplateSummary[]> {
    const params = new HttpParams().set('detectionType', detectionType.toString());
    return this.http
      .get<DetectionTemplateSummary[]>(`${this.apiBase}/templates`, { params })
      .pipe(map(templates => templates ?? []));
  }

  createFromTemplate(
    detectionType: DetectionType,
    request: CreateDetectionLogicFromTemplateRequest
  ): Observable<CanAnomalyDetectionLogicDto> {
    const params = new HttpParams().set('detectionType', detectionType.toString());
    const payload = {
      logicName: request.logicName,
      canSignalId: request.canSignalId,
      templateParameters: request.templateParameters ?? {},
    };

    return this.http
      .post<CanAnomalyDetectionLogicDto>(`${this.apiBase}/create-from-template`, payload, {
        params,
      })
      .pipe(
        map(dto => ({
          ...dto,
          parameters: dto.parameters ?? [],
          signalMappings: dto.signalMappings ?? [],
        }))
      );
  }
}
