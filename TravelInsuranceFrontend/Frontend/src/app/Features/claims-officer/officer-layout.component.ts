import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { OfficerSidebarComponent } from './components/officer-sidebar/officer-sidebar.component';

@Component({
  selector: 'app-officer-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, OfficerSidebarComponent],
  template: `
<div class="flex h-screen bg-[#FDF4F0]">
    <!-- Sidebar Container -->
    <div class="w-64 flex-shrink-0 bg-[#0E0E0E] text-white shadow-xl z-20">
        <app-officer-sidebar></app-officer-sidebar>
    </div>

    <!-- Main Content Area -->
    <div class="flex-1 flex flex-col overflow-hidden">
        <!-- Top Header / Topbar Area -->
        <header class="bg-white shadow-sm z-10 py-5 px-8 flex justify-between items-center">
            <h1 class="text-2xl font-bold tracking-tight text-[#111]" style="font-family: 'Poppins', sans-serif;">
                {{ pageTitle() }}
            </h1>
            <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-full bg-[#E8584A] flex items-center justify-center text-white font-bold">
                    {{ userNameInitials() }}
                </div>
                <div class="text-sm font-semibold text-[#444]">{{ userName() }}</div>
            </div>
        </header>

        <!-- Scrollable Content -->
        <main class="flex-1 overflow-x-hidden overflow-y-auto bg-[#FDF4F0] p-8">
            <router-outlet></router-outlet>
        </main>
    </div>
</div>
  `
})
export class OfficerLayoutComponent {
  // Using signals
  pageTitle = signal<string>('Dashboard');
  userName = signal<string>('Officer');

  userNameInitials = computed(() => {
    const name = this.userName();
    return name ? name.charAt(0).toUpperCase() : 'O';
  });

  constructor(private router: Router) {
    const fullName = localStorage.getItem('fullName');
    if (fullName) {
      this.userName.set(fullName);
    }

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.updateTitleByUrl(event.urlAfterRedirects);
    });
    this.updateTitleByUrl(this.router.url);
  }

  private updateTitleByUrl(url: string) {
    if (url.includes('dashboard')) this.pageTitle.set('Claims Dashboard');
    else if (url.includes('claims/') && url.split('/').length > 3) this.pageTitle.set('Claim Details');
    else if (url.includes('claims')) this.pageTitle.set('All Claims');
    else this.pageTitle.set('Claims Officer Portal');
  }
}
