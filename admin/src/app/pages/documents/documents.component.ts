import { Component, OnInit, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Search, FileText } from 'lucide-angular';
import { AdminService } from '../../services/admin.service';
import { AdminDocument } from '../../models/admin.model';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './documents.component.html'
})
export class DocumentsComponent implements OnInit {
  readonly icons = { Search, FileText };

  documents = signal<AdminDocument[]>([]);
  search = signal('');
  loading = signal(true);

  filtered = computed(() => {
    const q = this.search().toLowerCase();
    return this.documents().filter(d =>
      d.fileName.toLowerCase().includes(q) ||
      (d.userEmail?.toLowerCase().includes(q) ?? false)
    );
  });

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.adminService.getDocuments().subscribe({
      next: (d) => { this.documents.set(d); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1048576) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / 1048576).toFixed(1) + ' MB';
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString();
  }
}
