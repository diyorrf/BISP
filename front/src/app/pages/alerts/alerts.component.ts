import { Component, OnInit, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Bell, FileText, AlertTriangle, CheckCircle, X, Eye, Calendar, Scale } from 'lucide-angular';
import { AlertService } from '../../services/alert.service';
import { RegulatoryAlertDto } from '../../models/alert.model';

type FilterTab = 'all' | 'unread' | 'read';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [LucideAngularModule, DatePipe, RouterLink],
  templateUrl: './alerts.component.html'
})
export class AlertsComponent implements OnInit {
  readonly icons = { Bell, FileText, AlertTriangle, CheckCircle, X, Eye, Calendar, Scale };

  alerts = signal<RegulatoryAlertDto[]>([]);
  activeTab = signal<FilterTab>('all');
  loading = signal(false);

  filteredAlerts = computed(() => {
    const tab = this.activeTab();
    const all = this.alerts();
    if (tab === 'unread') return all.filter(a => !a.isRead);
    if (tab === 'read') return all.filter(a => a.isRead);
    return all;
  });

  unreadCount = computed(() => this.alerts().filter(a => !a.isRead).length);

  tabs: { key: FilterTab; label: string }[] = [
    { key: 'all', label: 'All' },
    { key: 'unread', label: 'Unread' },
    { key: 'read', label: 'Read' }
  ];

  constructor(private alertService: AlertService) {}

  ngOnInit(): void {
    this.loadAlerts();
  }

  loadAlerts(): void {
    this.loading.set(true);
    this.alertService.getAlerts().subscribe({
      next: alerts => {
        this.alerts.set(alerts);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  setTab(tab: FilterTab): void {
    this.activeTab.set(tab);
  }

  markAsRead(alert: RegulatoryAlertDto, event: Event): void {
    event.stopPropagation();
    this.alertService.markAsRead(alert.id).subscribe({
      next: () => {
        this.alerts.update(all =>
          all.map(a => a.id === alert.id ? { ...a, isRead: true } : a)
        );
      }
    });
  }

  markAllAsRead(): void {
    this.alertService.markAllAsRead().subscribe({
      next: () => {
        this.alerts.update(all => all.map(a => ({ ...a, isRead: true })));
      }
    });
  }

  dismiss(alert: RegulatoryAlertDto, event: Event): void {
    event.stopPropagation();
    this.alertService.dismiss(alert.id).subscribe({
      next: () => {
        this.alerts.update(all => all.filter(a => a.id !== alert.id));
      }
    });
  }
}
