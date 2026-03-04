import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { DocumentDto, DocumentDetailDto } from '../models/document.model';

@Injectable({ providedIn: 'root' })
export class DocumentService {
  private url = `${environment.apiUrl}/documents`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<DocumentDto[]> {
    return this.http.get<DocumentDto[]>(this.url);
  }

  getById(id: string): Observable<DocumentDto> {
    return this.http.get<DocumentDto>(`${this.url}/${id}`);
  }

  getDetail(id: string): Observable<DocumentDetailDto> {
    return this.http.get<DocumentDetailDto>(`${this.url}/${id}/detail`);
  }

  upload(file: File): Observable<DocumentDto> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<DocumentDto>(this.url, formData);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
