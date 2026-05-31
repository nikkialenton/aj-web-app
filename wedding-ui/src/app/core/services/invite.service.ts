import { Injectable } from '@angular/core';

const STORAGE_KEY = 'weddingInviteToken';

@Injectable({ providedIn: 'root' })
export class InviteService {
  setToken(token: string): void {
    sessionStorage.setItem(STORAGE_KEY, token);
  }

  getToken(): string | null {
    return sessionStorage.getItem(STORAGE_KEY);
  }

  clearToken(): void {
    sessionStorage.removeItem(STORAGE_KEY);
  }
}
