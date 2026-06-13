import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScrollRevealDirective } from '../shared/scroll-reveal.directive';

@Component({
  selector: 'app-attire',
  standalone: true,
  imports: [CommonModule, ScrollRevealDirective],
  templateUrl: './attire.component.html',
  styleUrls: ['./attire.component.scss']
})
export class AttireComponent {}
