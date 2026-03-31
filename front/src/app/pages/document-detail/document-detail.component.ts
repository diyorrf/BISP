import { Component, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { LucideAngularModule, FileText, Calendar, HardDrive, MessageSquare, Send, Loader, Info } from 'lucide-angular';
import { DocumentService } from '../../services/document.service';
import { QuestionService } from '../../services/question.service';
import { DocumentDetailDto } from '../../models/document.model';
import { MarkdownPipe } from '../../pipes/markdown.pipe';

interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  at: string;
}

@Component({
  selector: 'app-document-detail',
  standalone: true,
  imports: [RouterLink, FormsModule, DatePipe, LucideAngularModule, MarkdownPipe],
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

    const withContent = this.messages()
      .filter(m => (m.role === 'user' || m.role === 'assistant') && m.content.length > 0);
    const history = withContent
      .slice(0, -1) // exclude the current message (sent separately as questionText)
      .slice(-20)
      .map(m => ({ role: m.role as 'user' | 'assistant', content: m.content }));

    this.questionService.askStream({ documentId: this.docId(), questionText: text, history }).subscribe({
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
      error: (err: any) => {
        this.isTyping.set(false);
        const tokenMsg = 'You have used all your tokens for today. To get more tokens, please upgrade your plan or wait until tomorrow.';
        const errorContent = err?.code === 'TOKENS_EXHAUSTED' ? tokenMsg : 'Sorry, I could not answer that. Please try again.';
        this.messages.update(m =>
          m.map(msg => msg.id === assistantId ? { ...msg, content: errorContent } : msg)
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

