import { Component, OnInit, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, FileText, Download, Trash2, Eye, Filter, Search, Calendar, AlertTriangle, Upload } from 'lucide-angular';
import { DocumentService } from '../../services/document.service';
import { DocumentDto } from '../../models/document.model';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-document-library',
  standalone: true,
  imports: [FormsModule, LucideAngularModule, DatePipe, RouterLink],
  templateUrl: './document-library.component.html'
})
export class DocumentLibraryComponent implements OnInit {
  readonly icons = { FileText, Download, Trash2, Eye, Filter, Search, Calendar, AlertTriangle, Upload };

  documents = signal<DocumentDto[]>([]);
  searchQuery = signal('');
  loading = signal(false);

  filteredDocuments = computed(() => {
    const query = this.searchQuery().toLowerCase();
    return this.documents().filter(d =>
      d.fileName.toLowerCase().includes(query)
    );
  });

  totalSize = computed(() =>
    this.formatSize(this.documents().reduce((sum, d) => sum + d.sizeInBytes, 0))
  );

  constructor(private docService: DocumentService) {}

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.loading.set(true);
    this.docService.getAll().subscribe({
      next: (docs) => {
        this.documents.set(docs);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onFileUpload(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.loading.set(true);
      this.docService.upload(input.files[0]).subscribe({
        next: () => {
          this.loadDocuments();
          input.value = '';
        },
        error: () => this.loading.set(false)
      });
    }
  }

  deleteDocument(id: string): void {
    if (confirm('Are you sure you want to delete this document?')) {
      this.docService.delete(id).subscribe({
        next: () => this.loadDocuments()
      });
    }
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    return (bytes / 1024).toFixed(1) + ' KB';
  }

  onSearchInput(event: Event): void {
    this.searchQuery.set((event.target as HTMLInputElement).value);
  }
}
