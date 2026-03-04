export interface QuestionRequest {
  documentId: string;
  questionText: string;
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
