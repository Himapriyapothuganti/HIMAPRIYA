import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-customer-navbar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="h-20 bg-white/80 backdrop-blur-xl border-b border-gray-100 sticky top-0 z-40 px-8 flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-[#111] tracking-tight" style="font-family: 'Poppins', sans-serif;">{{ title }}</h1>
      </div>
      
      <div class="flex items-center gap-6">
        <!-- Notify Icon -->
        <button class="w-10 h-10 rounded-full flex items-center justify-center text-gray-400 hover:text-[#E8584A] hover:bg-[#FDF4F0] transition-colors relative">
          <span class="absolute top-2 right-2 w-2 h-2 bg-red-500 rounded-full border-2 border-white"></span>
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
            <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
          </svg>
        </button>

        <!-- Divider -->
        <div class="h-8 w-px bg-gray-200"></div>

        <!-- Mini Profile -->
        <div class="flex items-center gap-3">
          <div class="text-right">
            <p class="text-sm font-bold text-[#111]">{{ userName }}</p>
            <p class="text-xs text-gray-500">{{ email }}</p>
          </div>
          <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-gray-100 to-gray-50 flex items-center justify-center text-gray-600 font-bold border border-gray-200">
            {{ userNameInitials }}
          </div>
        </div>
      </div>
    </div>
  `
})
export class CustomerNavbar {
  @Input() title: string = 'Welcome';
  userName: string = 'User';
  email: string = 'user@example.com';
  userNameInitials: string = 'U';

  constructor() {
    const fullName = localStorage.getItem('fullName');
    const userEmail = localStorage.getItem('email');
    if (fullName) {
      this.userName = fullName;
      this.userNameInitials = fullName.charAt(0).toUpperCase();
    }
    if (userEmail) {
      this.email = userEmail;
    }
  }
}
