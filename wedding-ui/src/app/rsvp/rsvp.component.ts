import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
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

    this.rsvpService.lookup(this.token).subscribe({
      next: (guest) => {
        this.guest = guest;
        this.form.additionalGuests = Array(guest.allowedGuests).fill('');
        if (guest.hasRsvped) {
          this.submittedRsvp = guest.existingRsvp!;
          this.state = 'already-submitted';
        } else {
          this.state = 'form';
        }
      },
      error: () => this.state = 'not-found'
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
