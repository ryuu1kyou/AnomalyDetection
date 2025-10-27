import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  OemTraceabilityResult,
  OemCustomization,
  OemApproval,
  CreateOemCustomization,
  CreateOemApproval,
  CustomizationType,
  CustomizationStatus,
  ApprovalStatus,
  OemTraceabilityReport,
  GenerateOemTraceabilityReport
} from '../models/oem-traceability.models';

@Injectable({
  providedIn: 'root'
})
export class OemTraceabilityService {
  private readonly baseUrl = '/api/app/oem-traceability';

  constructor(private http: HttpClient) {}

  getOemTraceability(entityId: string, entityType: string): Observable<OemTraceabilityResult> {
    return this.http.get<OemTraceabilityResult>(
      `${this.baseUrl}/oem-traceability`,
      {
        params: { entityId, entityType }
      }
    );
  }

  createOemCustomization(input: CreateOemCustomization): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/oem-customization`, input);
  }

  updateOemCustomization(id: string, input: Partial<CreateOemCustomization>): Observable<OemCustomization> {
    return this.http.put<OemCustomization>(`${this.baseUrl}/oem-customization/${id}`, input);
  }

  getOemCustomization(id: string): Observable<OemCustomization> {
    return this.http.get<OemCustomization>(`${this.baseUrl}/oem-customization/${id}`);
  }

  getOemCustomizations(
    oemCode?: string,
    entityType?: string,
    status?: CustomizationStatus
  ): Observable<OemCustomization[]> {
    let params = new HttpParams();
    if (oemCode) params = params.set('oemCode', oemCode);
    if (entityType) params = params.set('entityType', entityType);
    if (status !== undefined) params = params.set('status', status.toString());

    return this.http.get<OemCustomization[]>(`${this.baseUrl}/oem-customizations`, { params });
  }

  submitForApproval(id: string): Observable<OemCustomization> {
    return this.http.post<OemCustomization>(`${this.baseUrl}/oem-customization/${id}/submit-for-approval`, {});
  }

  approveCustomization(id: string, approvalNotes?: string): Observable<OemCustomization> {
    return this.http.post<OemCustomization>(
      `${this.baseUrl}/oem-customization/${id}/approve`,
      { approvalNotes }
    );
  }

  rejectCustomization(id: string, rejectionNotes: string): Observable<OemCustomization> {
    return this.http.post<OemCustomization>(
      `${this.baseUrl}/oem-customization/${id}/reject`,
      { rejectionNotes }
    );
  }

  createOemApproval(input: CreateOemApproval): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/oem-approval`, input);
  }

  getOemApproval(id: string): Observable<OemApproval> {
    return this.http.get<OemApproval>(`${this.baseUrl}/oem-approval/${id}`);
  }

  getPendingApprovals(oemCode: string): Observable<OemApproval[]> {
    return this.http.get<OemApproval[]>(
      `${this.baseUrl}/pending-approvals`,
      { params: { oemCode } }
    );
  }

  approve(id: string, approvalNotes?: string): Observable<OemApproval> {
    return this.http.post<OemApproval>(`${this.baseUrl}/oem-approval/${id}/approve`, { approvalNotes });
  }

  rejectApproval(id: string, rejectionNotes: string): Observable<OemApproval> {
    return this.http.post<OemApproval>(`${this.baseUrl}/oem-approval/${id}/reject`, { rejectionNotes });
  }

  getUrgentApprovals(oemCode?: string): Observable<OemApproval[]> {
    let params = new HttpParams();
    if (oemCode) params = params.set('oemCode', oemCode);
    return this.http.get<OemApproval[]>(`${this.baseUrl}/urgent-approvals`, { params });
  }

  getOverdueApprovals(oemCode?: string): Observable<OemApproval[]> {
    let params = new HttpParams();
    if (oemCode) params = params.set('oemCode', oemCode);
    return this.http.get<OemApproval[]>(`${this.baseUrl}/overdue-approvals`, { params });
  }

  generateOemTraceabilityReport(input: GenerateOemTraceabilityReport): Observable<OemTraceabilityReport> {
    return this.http.post<OemTraceabilityReport>(`${this.baseUrl}/generate-report`, input);
  }

  getCustomizationStatistics(oemCode?: string): Observable<{ [key: number]: number }> {
    let params = new HttpParams();
    if (oemCode) params = params.set('oemCode', oemCode);
    return this.http.get<{ [key: number]: number }>(`${this.baseUrl}/customization-statistics`, { params });
  }

  getApprovalStatistics(oemCode?: string): Observable<{ [key: number]: number }> {
    let params = new HttpParams();
    if (oemCode) params = params.set('oemCode', oemCode);
    return this.http.get<{ [key: number]: number }>(`${this.baseUrl}/approval-statistics`, { params });
  }

  downloadReport(report: OemTraceabilityReport): void {
    const blob = new Blob([report.content], { type: report.contentType });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = report.fileName;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}