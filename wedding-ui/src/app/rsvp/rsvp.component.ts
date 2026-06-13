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

  form: RsvpCreate = {
    isAttending: true,
    plusOneAttending: undefined,
    plusOneName: '',
    message: ''
  };

  constructor(private route: ActivatedRoute, private rsvpService: RsvpService) {}

  ngOnInit() {
    this.token = this.route.snapshot.paramMap.get('token') ?? '';
    if (!this.token) { this.state = 'not-found'; return; }

    this.rsvpService.lookup(this.token).subscribe({
      next: (guest) => {
        this.guest = guest;
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
    if (!val) {
      this.form.plusOneAttending = undefined;
      this.form.plusOneName = '';
    }
  }

  setPlusOne(val: boolean) {
    this.form.plusOneAttending = val;
    if (!val) this.form.plusOneName = '';
  }

  submit() {
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
