import { Component } from '@angular/core';
import { ScrollRevealDirective } from '../shared/scroll-reveal.directive';

@Component({
  selector: 'app-details',
  standalone: true,
  imports: [ScrollRevealDirective],
  templateUrl: './details.component.html',
  styleUrls: ['./details.component.scss']
})
export class DetailsComponent {}
