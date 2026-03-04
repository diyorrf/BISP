export interface RecentActivityItemDto {
  id: string;
  documentId: string | null;
  type: 'document' | 'question';
  title: string;
  description: string;
  at: string;
}
