import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { InviteService } from '../core/services/invite.service';
import { RsvpService } from '../core/services/rsvp.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  guestName: string | null = null;
  inviteToken: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private invite: InviteService,
    private rsvpService: RsvpService
  ) {}

  ngOnInit() {
    document.addEventListener('contextmenu', (e) => {
      if ((e.target as HTMLElement).tagName === 'IMG') {
        e.preventDefault();
      }
    });
    
    const token = this.route.snapshot.paramMap.get('token') ?? this.invite.getToken();
    if (!token) return;

    this.invite.setToken(token);
    this.inviteToken = token;

    this.rsvpService.lookup(token).subscribe({
      next: (guest) => this.guestName = guest.fullName,
      error: () => {}
    });
  }
}
