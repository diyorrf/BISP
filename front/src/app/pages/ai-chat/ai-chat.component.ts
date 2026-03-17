import { Component, signal, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Send, Bot, User, BookOpen, Loader } from 'lucide-angular';
import { DocumentService } from '../../services/document.service';
import { QuestionService } from '../../services/question.service';
import { DocumentDto } from '../../models/document.model';

interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  references?: Array<{ title: string; article: string }>;
  timestamp: Date;
}

const GREETING: ChatMessage = {
  id: 'greeting',
  role: 'assistant',
  content: "Hello! I'm your LegalGuard AI assistant. I can help you understand Uzbekistan's business and legal regulations. Please select a document first, then ask your question.",
  timestamp: new Date()
};

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './ai-chat.component.html'
})
export class AiChatComponent implements AfterViewChecked {
  @ViewChild('messagesEnd') messagesEnd!: ElementRef;

  readonly icons = { Send, Bot, User, BookOpen, Loader };

  messages = signal<ChatMessage[]>([GREETING]);
  input = '';
  isTyping = signal(false);
  isLoadingHistory = signal(false);
  documents = signal<DocumentDto[]>([]);
  selectedDocumentId = signal<string>('');

  private chatCache = new Map<string, ChatMessage[]>();

  suggestedQuestions = [
    'What are the key risks in this document?',
    'Does this contract comply with labor laws?',
    'What clauses need revision?',
    'Summarize the main obligations in this document.',
  ];

  constructor(
    private docService: DocumentService,
    private questionService: QuestionService
  ) {
    this.loadDocuments();
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  loadDocuments(): void {
    this.docService.getAll().subscribe({
      next: (docs) => this.documents.set(docs)
    });
  }

  onDocumentChange(docId: string): void {
    this.selectedDocumentId.set(docId);

    if (!docId) {
      this.messages.set([GREETING]);
      return;
    }

    const cached = this.chatCache.get(docId);
    if (cached) {
      this.messages.set(cached);
      return;
    }

    this.isLoadingHistory.set(true);
    this.messages.set([GREETING]);

    this.questionService.getChatHistory(docId).subscribe({
      next: (history) => {
        const msgs: ChatMessage[] = [GREETING];
        for (const item of history) {
          msgs.push({
            id: item.id + '-q',
            role: 'user',
            content: item.questionText,
            timestamp: new Date(item.askedAt)
          });
          if (item.answer) {
            msgs.push({
              id: item.id + '-a',
              role: 'assistant',
              content: item.answer,
              timestamp: new Date(item.answeredAt ?? item.askedAt)
            });
          }
        }
        this.chatCache.set(docId, msgs);
        if (this.selectedDocumentId() === docId) {
          this.messages.set(msgs);
        }
        this.isLoadingHistory.set(false);
      },
      error: () => {
        this.isLoadingHistory.set(false);
      }
    });
  }

  sendMessage(): void {
    if (!this.input.trim()) return;

    const userMsg: ChatMessage = {
      id: Date.now().toString(),
      role: 'user',
      content: this.input,
      timestamp: new Date()
    };

    this.messages.update(msgs => [...msgs, userMsg]);
    const question = this.input;
    this.input = '';
    this.isTyping.set(true);

    const docId = this.selectedDocumentId();
    if (!docId) {
      this.addAssistantMessage('Please select a document first to ask questions about it.');
      return;
    }

    const allMsgs = this.messages()
      .filter(m => m.id !== 'greeting')
      .slice(0, -1)
      .slice(-20)
      .map(m => ({ role: m.role, content: m.content }));

    this.questionService.ask({ documentId: docId, questionText: question, history: allMsgs }).subscribe({
      next: (res) => {
        this.addAssistantMessage(res.answer);
        this.chatCache.set(docId, [...this.messages()]);
      },
      error: (err) => {
        if (err.status === 402) {
          this.addAssistantMessage('You have used all your tokens for today. To get more tokens, please upgrade your plan or wait until tomorrow.');
        } else {
          this.addAssistantMessage('Sorry, I encountered an error processing your question. Please try again.');
        }
      }
    });
  }

  selectQuestion(question: string): void {
    this.input = question;
  }

  formatTime(date: Date): string {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  showSuggestions(): boolean {
    const docId = this.selectedDocumentId();
    if (!docId) return this.messages().length === 1;
    const docMessages = this.messages().filter(m => m.id !== 'greeting');
    return docMessages.length === 0;
  }

  private addAssistantMessage(content: string): void {
    this.isTyping.set(false);
    const msg: ChatMessage = {
      id: (Date.now() + 1).toString(),
      role: 'assistant',
      content,
      timestamp: new Date()
    };
    this.messages.update(msgs => [...msgs, msg]);
  }

  private scrollToBottom(): void {
    try {
      this.messagesEnd?.nativeElement?.scrollIntoView({ behavior: 'smooth' });
    } catch { /* noop */ }
  }
}
