import { Component, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { LucideAngularModule, FileText, Calendar, HardDrive, MessageSquare, Send, Loader, Info } from 'lucide-angular';
import { DocumentService } from '../../services/document.service';
import { QuestionService } from '../../services/question.service';
import { DocumentDetailDto } from '../../models/document.model';

interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  at: string;
}

@Component({
  selector: 'app-document-detail',
  standalone: true,
  imports: [RouterLink, FormsModule, DatePipe, LucideAngularModule],
  templateUrl: './document-detail.component.html'
})
export class DocumentDetailComponent implements OnInit {
  readonly icons = { FileText, Calendar, HardDrive, MessageSquare, Send, Loader, Info };

  docId = signal<string>('');
  loading = signal(true);
  document = signal<DocumentDetailDto | null>(null);

  // Chat (persisted per document in localStorage)
  messages = signal<ChatMessage[]>([]);
  input = '';
  isTyping = signal(false);

  storageKey = computed(() => `lg_chat_${this.docId()}`);

  constructor(
    private route: ActivatedRoute,
    private documentService: DocumentService,
    private questionService: QuestionService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.docId.set(id);
    this.loadDocument();
    this.loadChat();
  }

  loadDocument(): void {
    this.loading.set(true);
    this.documentService.getDetail(this.docId()).subscribe({
      next: (d) => {
        this.document.set(d);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  send(): void {
    const text = this.input.trim();
    if (!text) return;

    const userMsg: ChatMessage = { id: Date.now().toString(), role: 'user', content: text, at: new Date().toISOString() };
    this.messages.update(m => [...m, userMsg]);
    this.input = '';
    this.saveChat();

    this.isTyping.set(true);

    // Streaming keeps the UX responsive for production
    let assistantId = (Date.now() + 1).toString();
    let assistantContent = '';
    this.messages.update(m => [...m, { id: assistantId, role: 'assistant', content: '', at: new Date().toISOString() }]);

    this.questionService.askStream({ documentId: this.docId(), questionText: text }).subscribe({
      next: (chunk) => {
        if (chunk.content) {
          assistantContent += chunk.content;
          this.messages.update(m =>
            m.map(msg => msg.id === assistantId ? { ...msg, content: assistantContent } : msg)
          );
        }
        if (chunk.isComplete) {
          this.isTyping.set(false);
          this.saveChat();
        }
      },
      error: () => {
        this.isTyping.set(false);
        this.messages.update(m =>
          m.map(msg => msg.id === assistantId ? { ...msg, content: 'Sorry, I could not answer that. Please try again.' } : msg)
        );
        this.saveChat();
      }
    });
  }

  clearChat(): void {
    this.messages.set([]);
    localStorage.removeItem(this.storageKey());
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  private loadChat(): void {
    try {
      const raw = localStorage.getItem(this.storageKey());
      if (!raw) return;
      const parsed = JSON.parse(raw) as ChatMessage[];
      if (Array.isArray(parsed)) this.messages.set(parsed);
    } catch {
      // ignore
    }
  }

  private saveChat(): void {
    try {
      localStorage.setItem(this.storageKey(), JSON.stringify(this.messages()));
    } catch {
      // ignore
    }
  }
}

