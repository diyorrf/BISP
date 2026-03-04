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

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './ai-chat.component.html'
})
export class AiChatComponent implements AfterViewChecked {
  @ViewChild('messagesEnd') messagesEnd!: ElementRef;

  readonly icons = { Send, Bot, User, BookOpen, Loader };

  messages = signal<ChatMessage[]>([
    {
      id: '1',
      role: 'assistant',
      content: "Hello! I'm your LegalGuard AI assistant. I can help you understand Uzbekistan's business and legal regulations. Please select a document first, then ask your question.",
      timestamp: new Date()
    }
  ]);

  input = '';
  isTyping = signal(false);
  documents = signal<DocumentDto[]>([]);
  selectedDocumentId = signal<string>('');

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

    this.questionService.ask({ documentId: docId, questionText: question }).subscribe({
      next: (res) => {
        this.addAssistantMessage(res.answer);
      },
      error: () => {
        this.addAssistantMessage('Sorry, I encountered an error processing your question. Please try again.');
      }
    });
  }

  selectQuestion(question: string): void {
    this.input = question;
  }

  formatTime(date: Date): string {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
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
