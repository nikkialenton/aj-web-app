import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-message-letter',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './message-letter.component.html',
  styleUrls: ['./message-letter.component.scss']
})
export class MessageLetterComponent {
  @Input() message = '';
  @Input() guestName = '';
  @Output() close = new EventEmitter<void>();

  isClosing = false;

  dismiss() {
    this.isClosing = true;
    setTimeout(() => this.close.emit(), 250);
  }
}
