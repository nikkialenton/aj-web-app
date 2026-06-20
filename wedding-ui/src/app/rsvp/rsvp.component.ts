import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { RsvpService } from '../core/services/rsvp.service';
import { GuestLookup, RsvpCreate, RsvpView } from '../core/models/models';

type PageState = 'loading' | 'not-found' | 'form' | 'submitted' | 'already-submitted';

@Component({
  selector: 'app-rsvp',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './rsvp.component.html',
  styleUrls: ['./rsvp.component.scss']
})
export class RsvpComponent implements OnInit {
  state: PageState = 'loading';
  showLoading = false;
  token = '';
  guest: GuestLookup | null = null;
  submittedRsvp: RsvpView | null = null;
  submitError = '';
  submitting = false;
  showConfirm = false;

  form: RsvpCreate = {
    isAttending: true,
    additionalGuests: [],
    message: ''
  };

  constructor(private route: ActivatedRoute, private rsvpService: RsvpService) {}

  ngOnInit() {
    this.token = this.route.snapshot.paramMap.get('token') ?? '';
    if (!this.token) { this.state = 'not-found'; return; }

    this.showLoading = true;

    const minDelay = new Promise<void>(res => setTimeout(res, 2000));
    const lookup = firstValueFrom(this.rsvpService.lookup(this.token));

    Promise.all([lookup, minDelay]).then(([guest]) => {
      this.showLoading = false;
      if (!guest) { this.state = 'not-found'; return; }
      this.guest = guest;
      this.form.additionalGuests = Array(guest.allowedGuests).fill('');
      if (guest.hasRsvped) {
        this.submittedRsvp = guest.existingRsvp!;
        this.state = 'already-submitted';
      } else {
        this.state = 'form';
      }
    }).catch(() => {
      this.showLoading = false;
      this.state = 'not-found';
    });
  }

  setAttending(val: boolean) {
    this.form.isAttending = val;
    if (!val) this.form.additionalGuests = this.form.additionalGuests.map(() => '');
  }

  get filledGuests(): string[] {
    return this.form.additionalGuests.filter(n => n.trim().length > 0);
  }

  get guestSlots(): number[] {
    return Array.from({ length: this.guest?.allowedGuests ?? 0 }, (_, i) => i);
  }

  trackByIndex(index: number): number { return index; }

  openConfirm() {
    this.showConfirm = true;
  }

  closeConfirm() {
    this.showConfirm = false;
  }

  submit() {
    this.showConfirm = false;
    this.submitting = true;
    this.submitError = '';
    this.rsvpService.submit(this.token, this.form).subscribe({
      next: (rsvp) => {
        this.submittedRsvp = rsvp;
        this.state = 'submitted';
        this.submitting = false;
      },
      error: () => {
        this.submitError = 'Something went wrong. Please try again.';
        this.submitting = false;
      }
    });
  }
}
