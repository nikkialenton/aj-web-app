import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { InviteService } from './core/services/invite.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  inviteToken: string | null = null;
  menuOpen = false;

  constructor(private invite: InviteService) {}

  ngOnInit() {
    this.invite.token$.subscribe(token => this.inviteToken = token);
  }
}
