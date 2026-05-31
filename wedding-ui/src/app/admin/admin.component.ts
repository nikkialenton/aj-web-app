import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GuestService } from '../core/services/guest.service';
import { GuestAdmin, GuestCreate, AdminStats } from '../core/models/models';

type AdminView = 'guests' | 'rsvps';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit {
  loggedIn = false;
  loading = false;
  view: AdminView = 'guests';
  guests: GuestAdmin[] = [];
  stats: AdminStats = { totalGuests: 0, rsvpedCount: 0, pendingCount: 0, attending: 0, declined: 0, totalAttending: 0 };
  copiedToken: string | null = null;
  importStatus = '';

  newGuest: GuestCreate = { fullName: '', email: '', allowedPlusOne: false, groupName: '' };
  showAddForm = false;

  constructor(private guestService: GuestService) {}

  ngOnInit() {}

  login() {
    this.loading = true;
    this.guestService.getStats().subscribe({
      next: (s) => { this.stats = s; this.loggedIn = true; this.loadGuests(); },
      error: () => { alert('Invalid admin key.'); this.loading = false; }
    });
  }

  loadGuests() {
    this.guestService.getAll().subscribe(g => { this.guests = g; this.loading = false; });
  }

  addGuest() {
    if (!this.newGuest.fullName) return;
    this.guestService.create(this.newGuest).subscribe(g => {
      this.guests.unshift(g);
      this.stats.totalGuests++;
      this.stats.pendingCount++;
      this.newGuest = { fullName: '', email: '', allowedPlusOne: false, groupName: '' };
      this.showAddForm = false;
    });
  }

  deleteGuest(id: number, name: string) {
    if (!confirm(`Remove ${name} from the guest list?`)) return;
    this.guestService.delete(id).subscribe(() => {
      this.guests = this.guests.filter(g => g.id !== id);
      this.stats.totalGuests--;
    });
  }

  copyLink(token: string) {
    const link = this.guestService.getRsvpLink(token);
    navigator.clipboard.writeText(link);
    this.copiedToken = token;
    setTimeout(() => this.copiedToken = null, 2000);
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.importStatus = 'Importing…';
    this.guestService.importCsv(file).subscribe({
      next: (r) => {
        this.importStatus = `${r.imported} guests imported!`;
        this.loadGuests();
        this.guestService.getStats().subscribe(s => this.stats = s);
        setTimeout(() => this.importStatus = '', 4000);
      },
      error: () => { this.importStatus = 'Import failed. Check your CSV format.'; }
    });
  }

  get rsvpedGuests() { return this.guests.filter(g => g.hasRsvped); }
  get pendingGuests() { return this.guests.filter(g => !g.hasRsvped); }

  exportCsv() { this.guestService.exportCsv(); }
  downloadTemplate() { this.guestService.downloadTemplate(); }
}
