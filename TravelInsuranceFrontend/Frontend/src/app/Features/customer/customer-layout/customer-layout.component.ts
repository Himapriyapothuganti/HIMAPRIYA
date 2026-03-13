import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { CustomerSidebar } from '../components/customer-sidebar/customer-sidebar.component';
import { NotificationService, NotificationDTO } from '../../../Services/notification.service';

@Component({
  selector: 'app-customer-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, CustomerSidebar],
  template: `
<div class="flex h-screen bg-[#FDF4F0]">
    <div class="w-64 flex-shrink-0 bg-[#0E0E0E] text-white shadow-xl z-20">
        <app-customer-sidebar></app-customer-sidebar>
    </div>

    <div class="flex-1 flex flex-col overflow-hidden">
        <header class="bg-white shadow-sm z-30 py-5 px-8 flex justify-between items-center relative">
            <h1 class="text-2xl font-bold tracking-tight text-[#111]" style="font-family: 'Poppins', sans-serif;">
                {{ pageTitle }}
            </h1>
            <div class="flex items-center gap-5">
                <!-- Notification Bell -->
                <div class="cursor-pointer" (click)="toggleNotifications()">
                    <div class="relative w-10 h-10 rounded-full flex items-center justify-center text-gray-400 hover:text-[#E8584A] hover:bg-[#FDF4F0] transition-colors">
                      <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#444" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
                          <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
                      </svg>
                      <!-- Badge -->
                      <span *ngIf="unreadCount() > 0" class="absolute top-1 right-2 flex h-3 w-3 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white border-2 border-white">
                      </span>
                    </div>
                </div>

                <!-- User Profile -->
                <div class="flex items-center gap-3 pl-4 border-l border-gray-100">
                    <div class="w-10 h-10 rounded-full bg-[#E8584A] flex items-center justify-center text-white font-bold shadow-md">
                        {{ userNameInitials }}
                    </div>
                    <div class="text-sm font-semibold text-[#444]">{{ userName }}</div>
                </div>
            </div>
            
            <!-- Notification Dropdown -->
            <div *ngIf="showNotifications()" 
                 (mouseleave)="showNotifications.set(false)"
                 class="absolute right-8 top-[85%] mt-2 w-[340px] bg-white rounded-xl shadow-[0_15px_50px_rgba(0,0,0,0.15)] border border-gray-100 overflow-hidden z-50 transform origin-top-right transition-all">
                <div class="px-5 py-4 border-b border-gray-50 flex justify-between items-center bg-gray-50/50">
                    <h3 class="font-bold text-[#111]">Notifications</h3>
                    <span class="text-xs font-bold text-[#E8584A]">{{ unreadCount() }} new</span>
                </div>
                <div class="max-h-[320px] overflow-y-auto">
                    <div *ngIf="notifications().length === 0" class="px-5 py-8 text-center text-gray-400 text-sm font-medium">
                        No notifications yet.
                    </div>
                    <!-- Notification Items -->
                    <div *ngFor="let notif of notifications()" 
                         (click)="markAsRead(notif); $event.stopPropagation()"
                         class="p-4 border-b border-gray-50 hover:bg-gray-50 transition-colors cursor-pointer group flex items-start gap-3"
                         [ngClass]="notif.isRead ? 'opacity-60' : 'bg-blue-50/30'">
                        <div class="w-2 h-2 rounded-full mt-2 flex-shrink-0" [ngClass]="notif.isRead ? 'bg-transparent' : 'bg-blue-500'"></div>
                        <div>
                            <p class="text-sm text-gray-800 leading-snug group-hover:text-black">{{ notif.message }}</p>
                            <p class="text-[10px] font-bold text-gray-400 uppercase tracking-wider mt-2">{{ notif.createdAt | date:'short' }}</p>
                        </div>
                    </div>
                </div>
            </div>
        </header>

        <main class="flex-1 overflow-x-hidden overflow-y-auto bg-[#FDF4F0] p-8">
            <router-outlet></router-outlet>
        </main>
    </div>
</div>
  `
})
export class CustomerLayoutComponent implements OnInit {
  pageTitle: string = 'Dashboard';
  userName: string = 'Customer';
  userNameInitials: string = 'C';

  private router = inject(Router);
  private notificationService = inject(NotificationService);

  showNotifications = signal(false);
  notifications = signal<NotificationDTO[]>([]);
  unreadCount = this.notificationService.unreadCount;

  constructor() {
    const fullName = localStorage.getItem('fullName');
    if (fullName) {
      this.userName = fullName;
      this.userNameInitials = fullName.charAt(0).toUpperCase();
    }

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.updateTitleByUrl(event.urlAfterRedirects);
    });
    this.updateTitleByUrl(this.router.url);
  }

  ngOnInit() {
    this.loadNotifications();
  }

  loadNotifications() {
    this.notificationService.getNotifications().subscribe({
      next: (data) => this.notifications.set(data),
      error: (err) => console.error('Failed to load notifications', err)
    });
  }

  toggleNotifications() {
    this.showNotifications.set(!this.showNotifications());
    if (this.showNotifications()) {
      this.loadNotifications(); // Refresh when opened
    }
  }

  markAsRead(notif: NotificationDTO) {
    if (notif.isRead) return;
    this.notificationService.markAsRead(notif.id).subscribe({
      next: () => {
        notif.isRead = true;
        // Optionally update the local list array
        this.notifications.update(list => [...list]);
        // Router navigation could be triggered based on notif.type if wanted
      }
    });
  }

  private updateTitleByUrl(url: string) {
    if (url.includes('dashboard')) this.pageTitle = 'Dashboard';
    else if (url.includes('browse-plans')) this.pageTitle = 'Browse Plans';
    else if (url.includes('purchase')) this.pageTitle = 'Checkout';
    else if (url.includes('payment')) this.pageTitle = 'Payment';
    else if (url.includes('my-policies')) this.pageTitle = 'My Policies';
    else if (url.includes('my-claims')) this.pageTitle = 'My Claims';
    else this.pageTitle = 'Customer Portal';
  }
}
