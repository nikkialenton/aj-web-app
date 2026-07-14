import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, OnDestroy } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { InviteService } from '../core/services/invite.service';
import { RsvpService } from '../core/services/rsvp.service';
import { ScrollRevealDirective } from '../shared/scroll-reveal.directive';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, ScrollRevealDirective],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, AfterViewInit, OnDestroy {
  guestName: string | null = null;
  inviteToken: string | null = null;

  @ViewChild('decadeVideo') decadeVideoRef!: ElementRef<HTMLVideoElement>;
  private videoObserver?: IntersectionObserver;

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
      next: (guest) => this.guestName = `${guest.firstName}`.trim(),
      error: () => {}
    });
  }

  ngAfterViewInit() {
    if (document.readyState === 'complete') {
      this.initVideoObserver();
    } else {
      window.addEventListener('load', () => this.initVideoObserver(), { once: true });
    }
  }

  private initVideoObserver() {
    const video = this.decadeVideoRef?.nativeElement;
    if (!video) return;

    this.videoObserver = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (!entry.isIntersecting) return;
        if (!video.getAttribute('src')) {
          video.muted = true;
          video.src = video.getAttribute('data-src') ?? '';
          video.load();
          video.addEventListener('canplay', () => video.play().catch(() => {}), { once: true });
        }
        this.videoObserver?.unobserve(video);
      });
    }, { rootMargin: '300px' });

    this.videoObserver.observe(video);
  }

  ngOnDestroy() {
    this.videoObserver?.disconnect();
  }
}
