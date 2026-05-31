import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GuestLookup, RsvpCreate, RsvpView } from '../models/models';

@Injectable({ providedIn: 'root' })
export class RsvpService {
  private api = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Called when guest opens their unique link
  lookup(token: string): Observable<GuestLookup> {
    return this.http.get<GuestLookup>(`${this.api}/rsvp/${token}`);
  }

  // Guest submits their RSVP
  submit(token: string, dto: RsvpCreate): Observable<RsvpView> {
    return this.http.post<RsvpView>(`${this.api}/rsvp/${token}`, dto);
  }
}
