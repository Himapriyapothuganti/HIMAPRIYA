import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../../Models/auth.service';

@Component({
    selector: 'app-officer-sidebar',
    standalone: true,
    imports: [CommonModule, RouterModule],
    template: `
<div class="h-full flex flex-col bg-[#0E0E0E] text-[#888]">
    <!-- Logo Area -->
    <div class="p-6 border-b border-[#1E1E1E] flex items-center justify-center">
        <div class="flex items-center gap-3">
            <div class="w-10 h-10 bg-[#E8584A] rounded-full flex items-center justify-center">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2.5">
                    <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
                    <circle cx="12" cy="10" r="3" />
                </svg>
            </div>
            <span class="font-['Poppins'] text-xl font-extrabold text-white tracking-tight">
                Talk<span class="text-[#E8584A]">&amp;</span>Travel
            </span>
        </div>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 overflow-y-auto py-6 px-4 space-y-2">
        <a routerLink="/officer/dashboard" routerLinkActive="bg-[#1E1E1E] text-white font-semibold shadow-inner"
            class="flex items-center gap-3 px-4 py-3 rounded-xl transition-all hover:bg-[#1E1E1E] hover:text-white group text-[15px]">
            <span class="text-xl group-hover:drop-shadow-md">📊</span>
            Dashboard
        </a>

        <a routerLink="/officer/claims" routerLinkActive="bg-[#1E1E1E] text-white font-semibold shadow-inner"
            class="flex items-center gap-3 px-4 py-3 rounded-xl transition-all hover:bg-[#1E1E1E] hover:text-white group text-[15px]">
            <span class="text-xl group-hover:drop-shadow-md">🔍</span>
            All Claims
        </a>
    </nav>

    <!-- Logout Area -->
    <div class="p-4 border-t border-[#1E1E1E]">
        <button (click)="logout()"
            class="w-full flex justify-center items-center gap-2 px-4 py-3 rounded-xl text-white bg-gradient-to-r from-red-500 to-red-600 hover:from-red-600 hover:to-red-700 shadow-md transform transition-transform hover:-translate-y-0.5 active:translate-y-0 active:shadow-sm font-semibold text-[15px]">
            <span class="text-lg">🚪</span> Logout
        </button>
    </div>
</div>
  `
})
export class OfficerSidebarComponent {
    userName = signal<string>('Officer');

    userNameInitials = computed(() => {
        const name = this.userName();
        return name ? name.charAt(0).toUpperCase() : 'O';
    });

    constructor(
        private authService: AuthService,
        private router: Router
    ) {
        const fullName = localStorage.getItem('fullName');
        if (fullName) {
            this.userName.set(fullName);
        }
    }

    logout() {
        this.authService.logout();
        this.router.navigate(['/login']);
    }
}
