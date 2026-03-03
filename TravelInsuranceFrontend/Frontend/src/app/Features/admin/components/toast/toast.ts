import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.html',
  styleUrl: './toast.css',
})
export class Toast implements OnInit {
  @Input() message: string = '';
  @Input() type: 'success' | 'error' | 'info' = 'info';
  @Input() duration: number = 3000;
  @Output() closeToast = new EventEmitter<void>();

  isVisible = false;

  get icon(): string {
    switch (this.type) {
      case 'success': return '✅';
      case 'error': return '❌';
      default: return 'ℹ️';
    }
  }

  get containerClasses(): string {
    const base = 'fixed top-6 right-6 z-50 flex items-center gap-3 px-6 py-4 rounded-xl shadow-2xl transform transition-all duration-300 min-w-[300px] border-l-4';
    const state = this.isVisible ? 'translate-x-0 opacity-100' : 'translate-x-12 opacity-0 pointer-events-none';

    let colors = '';
    if (this.type === 'success') colors = 'bg-white text-[#111] border-[#22C55E]';
    else if (this.type === 'error') colors = 'bg-white text-[#111] border-[#E8584A]';
    else colors = 'bg-white text-[#111] border-[#5B9BD5]';

    return `${base} ${state} ${colors}`;
  }

  ngOnInit() {
    // Small delay to allow element to render before adding visible classes
    setTimeout(() => {
      this.isVisible = true;
    }, 10);

    if (this.duration > 0) {
      setTimeout(() => {
        this.close();
      }, this.duration);
    }
  }

  close() {
    this.isVisible = false;
    setTimeout(() => {
      this.closeToast.emit();
    }, 300); // Wait for transition to finish
  }
}
