import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="fixed inset-0 bg-black/40 backdrop-blur-sm" (click)="closeModal.emit()"></div>
      <div [ngClass]="['bg-white rounded-3xl shadow-2xl w-full relative z-10 flex flex-col max-h-[90vh] animate-in fade-in zoom-in duration-200', maxWidthClass]">
        <!-- Header (Optional) -->
        <div *ngIf="title" class="flex items-center justify-between p-6 border-b border-gray-100">
          <h3 class="text-xl font-bold text-[#111]" style="font-family: 'Poppins', sans-serif;">{{ title }}</h3>
          <button (click)="closeModal.emit()" class="text-gray-400 hover:text-[#E8584A] transition-colors p-1">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
          </button>
        </div>
        <!-- Close Button (if no title) -->
        <button *ngIf="!title" (click)="closeModal.emit()" class="absolute top-4 right-4 z-50 text-white/70 hover:text-white transition-colors p-2 bg-black/10 hover:bg-black/20 rounded-full backdrop-blur-md">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
        </button>
        <!-- Body (Scrollable) -->
        <div class="p-6 overflow-y-auto overflow-x-hidden flex-1 relative rounded-3xl">
          <ng-content></ng-content>
        </div>
      </div>
    </div>
  `
})
export class Modal {
  @Input() isOpen = false;
  @Input() title = '';
  @Input() maxWidthClass = 'max-w-lg';
  @Output() closeModal = new EventEmitter<void>();
}
