export interface ChatMessageDto {
  role: 'user' | 'assistant';
  content: string;
}

export interface QuestionRequest {
  documentId: string;
  questionText: string;
  history?: ChatMessageDto[];
}

export interface QuestionResponse {
  questionId: string;
  answer: string;
  tokensUsed: number;
  processingTime: string;
}

export interface StreamChunk {
  content: string;
  isComplete: boolean;
}

export interface ChatHistoryItem {
  id: string;
  questionText: string;
  answer: string;
  askedAt: string;
  answeredAt: string | null;
}
