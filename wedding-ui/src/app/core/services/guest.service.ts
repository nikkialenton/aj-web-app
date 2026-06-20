import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GuestAdmin, GuestCreate, GuestUpdate, AdminStats } from '../models/models';

@Injectable({ providedIn: 'root' })
export class GuestService {
  private api = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAll(): Observable<GuestAdmin[]> {
    return this.http.get<GuestAdmin[]>(`${this.api}/guests`, { headers: this.h() });
  }

  getStats(): Observable<AdminStats> {
    return this.http.get<AdminStats>(`${this.api}/guests/stats`, { headers: this.h() });
  }

  create(dto: GuestCreate): Observable<GuestAdmin> {
    return this.http.post<GuestAdmin>(`${this.api}/guests`, dto, { headers: this.h() });
  }

  update(id: number, dto: GuestUpdate): Observable<GuestAdmin> {
    return this.http.put<GuestAdmin>(`${this.api}/guests/${id}`, dto, { headers: this.h() });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/guests/${id}`, { headers: this.h() });
  }

  importCsv(file: File): Observable<{ imported: number }> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ imported: number }>(`${this.api}/guests/import`, form, { headers: this.h() });
  }

  exportCsv(): void {
    this.http.get(`${this.api}/guests/export`, { headers: this.h(), responseType: 'blob' })
      .subscribe(blob => {
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = `guests-${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
      });
  }

  downloadTemplate(): void {
    this.http.get(`${this.api}/guests/template`, { headers: this.h(), responseType: 'blob' })
      .subscribe(blob => {
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = 'guest-import-template.csv';
        a.click();
      });
  }

  getRsvpLink(token: string): string {
    return `${environment.siteUrl}/invite/${token}`;
  }

  private h(): HttpHeaders {
    return new HttpHeaders({ 'X-Admin-Key': environment.adminKey });
  }
}
