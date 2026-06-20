import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GuestService } from '../core/services/guest.service';
import { GuestAdmin, GuestCreate, GuestUpdate, AdminStats } from '../core/models/models';
import { environment } from '../../environments/environment';

type AdminView = 'guests' | 'rsvps' | 'pending';

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
  loginError = '';
  username = '';
  password = '';
  showPassword = false;
  view: AdminView = 'guests';
  guests: GuestAdmin[] = [];
  stats: AdminStats = { totalGuests: 0, rsvpedCount: 0, pendingCount: 0, attending: 0, declined: 0, totalAttending: 0 };
  copiedToken: string | null = null;
  importStatus = '';

  newGuest: GuestCreate = { firstName: '', lastName: '', email: '', allowedGuests: 0, groupName: '' };
  addGuestError = '';
  showAddForm = false;

  selectedMessage: string | null = null;
  editingId: number | null = null;
  editDraft: GuestUpdate = { firstName: '', lastName: '', groupName: '', allowedGuests: 0 };
  editGuestError = '';
  saving = false;

  guestSortCol = 'name';
  guestSortDir: 1 | -1 = 1;
  rsvpSortCol = 'name';
  rsvpSortDir: 1 | -1 = 1;
  rsvpFilter: 'all' | 'attending' | 'declined' = 'all';

  constructor(private guestService: GuestService) {}

  ngOnInit() {}

  login() {
    const match = environment.adminUsers.find(
      u => u.username === this.username && u.password === this.password
    );
    if (!match) {
      this.loginError = 'Incorrect username or password.';
      return;
    }
    this.loginError = '';
    this.loading = true;
    this.guestService.getStats().subscribe({
      next: (s) => { this.stats = s; this.loggedIn = true; this.loadGuests(); },
      error: () => { this.loginError = 'Could not connect to server.'; this.loading = false; }
    });
  }

  loadGuests() {
    this.guestService.getAll().subscribe(g => { this.guests = g; this.loading = false; });
  }

  private requiredFieldsError(firstName: string, lastName: string, groupName: string): string {
    if (!firstName?.trim() || !lastName?.trim() || !groupName?.trim())
      return 'First name, Last name, and Group are required.';
    return '';
  }

  addGuest() {
    this.addGuestError = this.requiredFieldsError(this.newGuest.firstName, this.newGuest.lastName, this.newGuest.groupName);
    if (this.addGuestError) return;
    this.guestService.create(this.newGuest).subscribe({
      next: g => {
        this.guests.unshift(g);
        this.stats.totalGuests++;
        this.stats.pendingCount++;
        this.newGuest = { firstName: '', lastName: '', email: '', allowedGuests: 0, groupName: '' };
        this.showAddForm = false;
      },
      error: () => { this.addGuestError = 'Failed to add guest. Please try again.'; }
    });
  }

  deleteGuest(id: number, name: string, hasRsvped: boolean) {
    const msg = hasRsvped
      ? `${name} has already submitted an RSVP. Removing them will also delete their RSVP response. Are you sure?`
      : `Remove ${name} from the guest list?`;
    if (!confirm(msg)) return;
    const snapshot = this.guests;
    this.guests = this.guests.filter(g => g.id !== id);
    this.guestService.delete(id).subscribe({
      next: () => { this.guestService.getStats().subscribe(s => this.stats = s); },
      error: () => { this.guests = snapshot; alert(`Failed to remove ${name}. Please try again.`); }
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
    input.value = '';
    this.importStatus = 'Importing…';
    this.guestService.importCsv(file).subscribe({
      next: (r) => {
        this.importStatus = r.skipped
          ? `${r.imported} guests imported, ${r.skipped} skipped (already exist).`
          : `${r.imported} guests imported!`;
        this.loadGuests();
        this.guestService.getStats().subscribe(s => this.stats = s);
        setTimeout(() => this.importStatus = '', 4000);
      },
      error: (err) => { this.importStatus = err?.error?.error ?? 'Import failed. Check your CSV format.'; }
    });
  }

  get rsvpedGuests() { return this.guests.filter(g => g.hasRsvped); }
  get pendingGuests() { return this.guests.filter(g => !g.hasRsvped); }

  setGuestSort(col: string) {
    this.guestSortDir = this.guestSortCol === col ? (-this.guestSortDir as 1 | -1) : 1;
    this.guestSortCol = col;
  }
  setRsvpSort(col: string) {
    this.rsvpSortDir = this.rsvpSortCol === col ? (-this.rsvpSortDir as 1 | -1) : 1;
    this.rsvpSortCol = col;
  }
  guestIcon(col: string) { return this.guestSortCol === col ? (this.guestSortDir === 1 ? ' ↑' : ' ↓') : ''; }
  rsvpIcon(col: string)  { return this.rsvpSortCol  === col ? (this.rsvpSortDir  === 1 ? ' ↑' : ' ↓') : ''; }

  private sortedBy<T>(arr: T[], _col: string, dir: 1 | -1, key: (x: T) => string | number): T[] {
    return [...arr].sort((a, b) => {
      const av = key(a), bv = key(b);
      return (av < bv ? -1 : av > bv ? 1 : 0) * dir;
    });
  }

  get sortedGuests() {
    return this.sortedBy(this.guests, this.guestSortCol, this.guestSortDir, g => ({
      name: `${g.firstName} ${g.lastName}`.toLowerCase(),
      group: (g.groupName || '').toLowerCase(),
      guests: g.allowedGuests,
      status: g.hasRsvped ? (g.rsvp?.isAttending ? 0 : 1) : 2,
    }[this.guestSortCol] ?? ''));
  }
  get filteredRsvpedGuests() {
    if (this.rsvpFilter === 'attending') return this.rsvpedGuests.filter(g => g.rsvp?.isAttending);
    if (this.rsvpFilter === 'declined')  return this.rsvpedGuests.filter(g => !g.rsvp?.isAttending);
    return this.rsvpedGuests;
  }
  get sortedRsvpedGuests() {
    return this.sortedBy(this.filteredRsvpedGuests, this.rsvpSortCol, this.rsvpSortDir, g => ({
      name: `${g.firstName} ${g.lastName}`.toLowerCase(),
      attending: g.rsvp?.isAttending ? 0 : 1,
      submitted: new Date(g.rsvp!.submittedAt).getTime(),
    }[this.rsvpSortCol] ?? ''));
  }
  get sortedPendingGuests() {
    return this.sortedBy(this.pendingGuests, this.guestSortCol, this.guestSortDir, g => ({
      name: `${g.firstName} ${g.lastName}`.toLowerCase(),
      group: (g.groupName || '').toLowerCase(),
      guests: g.allowedGuests,
    }[this.guestSortCol] ?? ''));
  }

  get groupOptions() {
    return [...new Set(this.guests.map(g => g.groupName).filter(Boolean))].sort();
  }

  startEdit(g: GuestAdmin) {
    this.editingId = g.id;
    this.editGuestError = '';
    this.editDraft = { firstName: g.firstName, lastName: g.lastName, groupName: g.groupName, allowedGuests: g.allowedGuests };
  }

  cancelEdit() {
    this.editingId = null;
    this.editGuestError = '';
  }

  saveEdit(id: number) {
    this.editGuestError = this.requiredFieldsError(this.editDraft.firstName!, this.editDraft.lastName!, this.editDraft.groupName!);
    if (this.editGuestError) return;
    this.saving = true;
    const minDelay = new Promise(res => setTimeout(res, 300));
    const save$ = new Promise<void>((res, rej) =>
      this.guestService.update(id, this.editDraft).subscribe({ next: updated => {
        const i = this.guests.findIndex(g => g.id === id);
        if (i !== -1) this.guests[i] = { ...this.guests[i], ...updated };
        res();
      }, error: rej })
    );
    Promise.all([minDelay, save$]).then(() => {
      this.saving = false;
      this.editingId = null;
    }).catch(() => {
      this.saving = false;
      this.editGuestError = 'Failed to save changes. Please try again.';
    });
  }

  refresh() {
    this.loading = true;
    const minDelay = new Promise(res => setTimeout(res, 600));
    const stats$ = new Promise<void>(res =>
      this.guestService.getStats().subscribe(s => { this.stats = s; res(); })
    );
    const guests$ = new Promise<void>(res =>
      this.guestService.getAll().subscribe(g => { this.guests = g; res(); })
    );
    Promise.all([minDelay, stats$, guests$]).then(() => this.loading = false);
  }

  logout() {
    this.loggedIn = false;
    this.username = '';
    this.password = '';
    this.showPassword = false;
    this.guests = [];
    this.view = 'guests';
    this.showAddForm = false;
    this.editingId = null;
    this.editGuestError = '';
    this.addGuestError = '';
  }

  exportCsv() { this.guestService.exportCsv(); }
  downloadTemplate() { this.guestService.downloadTemplate(); }
}
