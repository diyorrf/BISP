import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { QuestionRequest, QuestionResponse, StreamChunk } from '../models/question.model';

@Injectable({ providedIn: 'root' })
export class QuestionService {
  private url = `${environment.apiUrl}/questions`;

  constructor(private http: HttpClient) {}

  ask(request: QuestionRequest): Observable<QuestionResponse> {
    return this.http.post<QuestionResponse>(this.url, request);
  }

  askStream(request: QuestionRequest): Observable<StreamChunk> {
    const subject = new Subject<StreamChunk>();

    const ws = new WebSocket(`${environment.wsUrl}/questions`);

    ws.onopen = () => {
      ws.send(JSON.stringify(request));
    };

    ws.onmessage = (event) => {
      const chunk: StreamChunk = JSON.parse(event.data);
      subject.next(chunk);
      if (chunk.isComplete) {
        ws.close();
        subject.complete();
      }
    };

    ws.onerror = () => {
      subject.error(new Error('WebSocket connection failed'));
    };

    ws.onclose = () => {
      subject.complete();
    };

    return subject.asObservable();
  }
}
