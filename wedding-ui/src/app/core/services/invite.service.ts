import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

const STORAGE_KEY = 'weddingInviteToken';

@Injectable({ providedIn: 'root' })
export class InviteService {
  private tokenSubject = new BehaviorSubject<string | null>(sessionStorage.getItem(STORAGE_KEY));
  token$ = this.tokenSubject.asObservable();

  setToken(token: string): void {
    sessionStorage.setItem(STORAGE_KEY, token);
    this.tokenSubject.next(token);
  }

  getToken(): string | null {
    return this.tokenSubject.value;
  }

  clearToken(): void {
    sessionStorage.removeItem(STORAGE_KEY);
    this.tokenSubject.next(null);
  }
}
