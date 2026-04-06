import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Plus, Play, Trash2, CheckCircle, Clock, X, Loader, Upload } from 'lucide-angular';
import { AdminService } from '../../services/admin.service';
import { AdminRegulatoryUpdate, CreateRegulatoryUpdate } from '../../models/admin.model';

@Component({
  selector: 'app-regulatory',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './regulatory.component.html'
})
export class RegulatoryComponent implements OnInit {
  readonly icons = { Plus, Play, Trash2, CheckCircle, Clock, X, Loader, Upload };

  updates = signal<AdminRegulatoryUpdate[]>([]);
  loading = signal(true);
  showForm = signal(false);
  processingId = signal<string | null>(null);
  processResult = signal<string | null>(null);
  creating = signal(false);

  form: CreateRegulatoryUpdate = {
    title: '',
    lawIdentifier: '',
    description: '',
    sourceUrl: '',
    effectiveDate: '',
    publishedAt: ''
  };

  selectedFile: File | null = null;

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadUpdates();
  }

  loadUpdates(): void {
    this.adminService.getRegulatoryUpdates().subscribe({
      next: (u) => { this.updates.set(u); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    }
  }

  removeFile(): void {
    this.selectedFile = null;
  }

  createUpdate(): void {
    if (!this.form.title || !this.form.lawIdentifier) return;

    this.creating.set(true);

    const dto: CreateRegulatoryUpdate = {
      title: this.form.title,
      lawIdentifier: this.form.lawIdentifier,
      description: this.form.description || undefined,
      sourceUrl: this.form.sourceUrl || undefined,
      effectiveDate: this.form.effectiveDate || undefined,
      publishedAt: this.form.publishedAt || undefined
    };

    this.adminService.createRegulatoryUpdate(dto, this.selectedFile ?? undefined).subscribe({
      next: (created) => {
        this.updates.update(list => [created, ...list]);
        this.resetForm();
        this.showForm.set(false);
        this.creating.set(false);
      },
      error: () => {
        this.creating.set(false);
      }
    });
  }

  processUpdate(id: string): void {
    this.processingId.set(id);
    this.processResult.set(null);

    this.adminService.processRegulatoryUpdate(id).subscribe({
      next: (result) => {
        this.processingId.set(null);
        this.processResult.set(result.message);
        this.updates.update(list =>
          list.map(u => u.id === id ? { ...u, isProcessed: true, alertsCount: u.alertsCount + result.alertsCreated } : u)
        );
        setTimeout(() => this.processResult.set(null), 5000);
      },
      error: () => {
        this.processingId.set(null);
        this.processResult.set('Processing failed. Please try again.');
      }
    });
  }

  deleteUpdate(id: string): void {
    this.adminService.deleteRegulatoryUpdate(id).subscribe({
      next: () => {
        this.updates.update(list => list.filter(u => u.id !== id));
      }
    });
  }

  formatDate(date: string | null): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString();
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  private resetForm(): void {
    this.form = { title: '', lawIdentifier: '', description: '', sourceUrl: '', effectiveDate: '', publishedAt: '' };
    this.selectedFile = null;
  }
}
