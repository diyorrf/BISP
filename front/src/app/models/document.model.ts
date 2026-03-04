export interface DocumentDto {
  id: string;
  fileName: string;
  contentType: string;
  sizeInBytes: number;
  uploadedAt: string;
}

export interface DocumentDetailDto extends DocumentDto {
  lastAccessedAt: string | null;
  content: string;
}
