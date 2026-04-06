import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AdminStats, AdminUser, AdminUserDetail, AdminDocument,
  AdminRegulatoryUpdate, CreateRegulatoryUpdate, ProcessResult,
  AdminPayment
} from '../models/admin.model';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private url = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  getStats(): Observable<AdminStats> {
    return this.http.get<AdminStats>(`${this.url}/stats`);
  }

  getUsers(): Observable<AdminUser[]> {
    return this.http.get<AdminUser[]>(`${this.url}/users`);
  }

  getUser(id: number): Observable<AdminUserDetail> {
    return this.http.get<AdminUserDetail>(`${this.url}/users/${id}`);
  }

  updateUserPlan(id: number, plan: string): Observable<unknown> {
    return this.http.put(`${this.url}/users/${id}/plan`, { plan });
  }

  toggleUserActive(id: number): Observable<{ isActive: boolean }> {
    return this.http.put<{ isActive: boolean }>(`${this.url}/users/${id}/toggle-active`, {});
  }

  setUserTokens(id: number, tokens: number): Observable<unknown> {
    return this.http.put(`${this.url}/users/${id}/tokens`, { tokens });
  }

  getDocuments(): Observable<AdminDocument[]> {
    return this.http.get<AdminDocument[]>(`${this.url}/documents`);
  }

  getRegulatoryUpdates(): Observable<AdminRegulatoryUpdate[]> {
    return this.http.get<AdminRegulatoryUpdate[]>(`${this.url}/regulatory-updates`);
  }

  createRegulatoryUpdate(dto: CreateRegulatoryUpdate, file?: File): Observable<AdminRegulatoryUpdate> {
    const formData = new FormData();
    formData.append('title', dto.title);
    formData.append('lawIdentifier', dto.lawIdentifier);
    if (dto.description) formData.append('description', dto.description);
    if (dto.sourceUrl) formData.append('sourceUrl', dto.sourceUrl);
    if (dto.effectiveDate) formData.append('effectiveDate', dto.effectiveDate);
    if (dto.publishedAt) formData.append('publishedAt', dto.publishedAt);
    if (file) formData.append('file', file);
    return this.http.post<AdminRegulatoryUpdate>(`${this.url}/regulatory-updates`, formData);
  }

  processRegulatoryUpdate(id: string): Observable<ProcessResult> {
    return this.http.post<ProcessResult>(`${this.url}/regulatory-updates/${id}/process`, {});
  }

  deleteRegulatoryUpdate(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/regulatory-updates/${id}`);
  }

  getPayments(): Observable<AdminPayment[]> {
    return this.http.get<AdminPayment[]>(`${this.url}/payments`);
  }
}
