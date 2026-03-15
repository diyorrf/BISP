import { Injectable, signal, computed, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, interval, Subscription } from 'rxjs';
import { environment } from '../../environments/environment';
import { RegulatoryAlertDto } from '../models/alert.model';

@Injectable({ providedIn: 'root' })
export class AlertService implements OnDestroy {
  private url = `${environment.apiUrl}/alerts`;
  private unreadCountSignal = signal(0);
  private pollSub?: Subscription;

  unreadCount = computed(() => this.unreadCountSignal());

  constructor(private http: HttpClient) {}

  startPolling(): void {
    if (this.pollSub) return;
    this.refreshUnreadCount();
    this.pollSub = interval(60_000).subscribe(() => this.refreshUnreadCount());
  }

  stopPolling(): void {
    this.pollSub?.unsubscribe();
    this.pollSub = undefined;
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  refreshUnreadCount(): void {
    this.http.get<number>(`${this.url}/unread-count`).subscribe({
      next: count => this.unreadCountSignal.set(count),
      error: () => {}
    });
  }

  getAlerts(isRead?: boolean): Observable<RegulatoryAlertDto[]> {
    const params: Record<string, string> = {};
    if (isRead !== undefined) params['isRead'] = String(isRead);
    return this.http.get<RegulatoryAlertDto[]>(this.url, { params });
  }

  markAsRead(id: string): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}/read`, {}).pipe(
      tap(() => this.refreshUnreadCount())
    );
  }

  markAllAsRead(): Observable<void> {
    return this.http.put<void>(`${this.url}/mark-all-read`, {}).pipe(
      tap(() => this.unreadCountSignal.set(0))
    );
  }

  dismiss(id: string): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}/dismiss`, {}).pipe(
      tap(() => this.refreshUnreadCount())
    );
  }
}
