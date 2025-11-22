import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DetectionTemplateSummary {
  templateType: number;
  name: string;
  description?: string;
  parameters: TemplateParameterDescriptor[];
  version?: number;
  lastUsedAt?: string;
  useCount?: number;
}

export interface TemplateParameterDescriptor {
  name: string;
  displayName?: string;
  type: 'number' | 'string' | 'boolean';
  defaultValue?: any;
  required?: boolean;
  min?: number;
  max?: number;
}

export interface CreateTemplateFromTypeInput {
  templateType: number;
  overrides?: Record<string, any>;
}

export interface ValidateTemplateInput {
  templateType: number;
  parameters: Record<string, any>;
}

export interface TemplateValidationResult {
  isValid: boolean;
  errors: string[];
  warnings: string[];
}

@Injectable({ providedIn: 'root' })
export class DetectionTemplatesService {
  private http = inject(HttpClient);
  private apiBase = '/api/detection-templates'; // TODO: confirm server-side route prefix

  getAvailable(): Observable<DetectionTemplateSummary[]> {
    return this.http.get<DetectionTemplateSummary[]>(`${this.apiBase}/available`);
  }

  getByType(templateType: number): Observable<DetectionTemplateSummary> {
    return this.http.get<DetectionTemplateSummary>(`${this.apiBase}/${templateType}`);
  }

  createFromTemplate(input: CreateTemplateFromTypeInput): Observable<any> {
    return this.http.post<any>(`${this.apiBase}/create-from-template`, input);
  }

  validateParameters(input: ValidateTemplateInput): Observable<TemplateValidationResult> {
    // NOTE: Backend method name might differ (ABP convention). Adjust if necessary.
    return this.http.post<TemplateValidationResult>(`${this.apiBase}/validate`, input);
  }
}
