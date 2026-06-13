import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScrollRevealDirective } from '../shared/scroll-reveal.directive';

@Component({
  selector: 'app-gifts',
  standalone: true,
  imports: [CommonModule, ScrollRevealDirective],
  templateUrl: './gifts.component.html',
  styleUrls: ['./gifts.component.scss']
})
export class GiftsComponent {}
