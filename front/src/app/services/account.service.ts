import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AccountDto } from '../models/account.model';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private url = `${environment.apiUrl}/account`;
  private accountSignal = signal<AccountDto | null>(null);

  account = computed(() => this.accountSignal());

  constructor(private http: HttpClient) {}

  load(): Observable<AccountDto> {
    return this.http.get<AccountDto>(`${this.url}/me`).pipe(
      tap(a => this.accountSignal.set(a))
    );
  }

  clear(): void {
    this.accountSignal.set(null);
  }
}
