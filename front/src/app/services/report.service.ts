import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ReportIssue {
  clause: string;
  risk: string;
  description: string;
  reference: string;
}

export interface ContractScannerReportPayload {
  fileName: string;
  riskLevel: string;
  summary: string;
  issues: ReportIssue[];
  recommendations: string[];
}

@Injectable({ providedIn: 'root' })
export class ReportService {
  private url = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  /**
   * Request PDF report for contract scanner analysis result.
   * Returns blob to be used for file download.
   */
  downloadContractScannerPdf(payload: ContractScannerReportPayload): Observable<Blob> {
    return this.http.post(`${this.url}/contract-scanner`, payload, {
      responseType: 'blob',
    });
  }
}
