import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { DashboardStatsDto } from '../models/dashboard.model';
import { RecentActivityItemDto } from '../models/recent-activity.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private url = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getStats(): Observable<DashboardStatsDto> {
    return this.http.get<DashboardStatsDto>(`${this.url}/stats`);
  }

  getRecentActivity(count = 10): Observable<RecentActivityItemDto[]> {
    return this.http.get<RecentActivityItemDto[]>(`${this.url}/recent-activity`, {
      params: { count: count.toString() }
    });
  }
}
