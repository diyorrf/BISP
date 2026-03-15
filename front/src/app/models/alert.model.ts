export interface RegulatoryAlertDto {
  id: string;
  documentId: string;
  documentName: string;
  updateTitle: string;
  updateDescription: string;
  lawReference: string;
  effectiveDate: string | null;
  isRead: boolean;
  createdAt: string;
}
