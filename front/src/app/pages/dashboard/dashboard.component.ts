import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, FileText, MessageSquare, CheckCircle, AlertTriangle, TrendingUp, Clock } from 'lucide-angular';
import { DashboardService } from '../../services/dashboard.service';
import { RecentActivityItemDto } from '../../models/recent-activity.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  readonly icons = { FileText, MessageSquare, CheckCircle, AlertTriangle, TrendingUp, Clock };

  documentsScanned = signal(0);
  aiConsultations = signal(0);
  highRiskAlerts = signal(0);
  complianceChecks = signal(0);
  loading = signal(true);
  recentActivity = signal<RecentActivityItemDto[]>([]);

  stats = [
    { label: 'Documents Scanned', value: this.documentsScanned, icon: FileText, color: 'blue' },
    { label: 'High Risk Alerts', value: this.highRiskAlerts, icon: AlertTriangle, color: 'red' },
    { label: 'Compliance Checks', value: this.complianceChecks, icon: CheckCircle, color: 'green' },
    { label: 'AI Consultations', value: this.aiConsultations, icon: MessageSquare, color: 'purple' },
  ];

  quickActions = [
    { title: 'Scan New Contract', description: 'Upload and analyze legal documents for risk', icon: FileText, path: '/scanner', color: 'blue' },
    { title: 'Ask AI Assistant', description: 'Get compliance answers with legal references', icon: MessageSquare, path: '/chat', color: 'purple' },
    { title: 'Check Business Plan', description: 'Verify if your initiative complies with Uzbek law', icon: CheckCircle, path: '/business-checker', color: 'green' },
  ];

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getStats().subscribe({
      next: (s) => {
        this.documentsScanned.set(s.documentsScanned);
        this.aiConsultations.set(s.aiConsultations);
        this.highRiskAlerts.set(s.highRiskAlerts);
        this.complianceChecks.set(s.complianceChecks);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });

    this.dashboardService.getRecentActivity(10).subscribe({
      next: (items) => this.recentActivity.set(items),
      error: () => {}
    });
  }

  formatRelativeTime(at: string): string {
    const date = new Date(at);
    const now = new Date();
    const sec = Math.floor((now.getTime() - date.getTime()) / 1000);
    if (sec < 60) return 'Just now';
    const min = Math.floor(sec / 60);
    if (min < 60) return `${min} minute${min === 1 ? '' : 's'} ago`;
    const hr = Math.floor(min / 60);
    if (hr < 24) return `${hr} hour${hr === 1 ? '' : 's'} ago`;
    const day = Math.floor(hr / 24);
    if (day < 7) return `${day} day${day === 1 ? '' : 's'} ago`;
    return date.toLocaleDateString();
  }

  activityIcon(type: string): typeof FileText {
    return type === 'question' ? MessageSquare : FileText;
  }
}
