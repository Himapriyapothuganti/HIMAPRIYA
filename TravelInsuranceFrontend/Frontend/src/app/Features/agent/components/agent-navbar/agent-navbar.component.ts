import { Component, Input, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, NotificationDTO } from '../../../../Services/notification.service';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-agent-navbar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-white shadow-sm sticky top-0 z-40 py-5 px-8 flex justify-between items-center relative">
      <div>
        <h1 class="text-2xl font-bold tracking-tight text-[#111]" style="font-family: 'Poppins', sans-serif;">{{ title }}</h1>
      </div>
      
      <div class="flex items-center gap-6">
        <!-- Notify Icon -->
        <div class="cursor-pointer" (click)="toggleNotifications()">
          <button class="w-10 h-10 rounded-full flex items-center justify-center text-gray-400 hover:text-[#E8584A] hover:bg-[#FDF4F0] transition-colors relative">
            <ng-container *ngIf="unreadCount() > 0">
              <span class="notification-ring absolute top-1.5 right-1.5 h-3 w-3 rounded-full bg-red-400 opacity-75"></span>
              <span class="notification-dot absolute top-1.5 right-1.5 flex h-3 w-3 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white border-2 border-white shadow-sm"></span>
            </ng-container>
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
              <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
            </svg>
          </button>
        </div>

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
    </div>
  `,
  styles: [`
    .notification-ring { animation: pulse-ring 2s infinite; }
    @keyframes pulse-ring { 0% { transform: scale(0.33); } 80%, 100% { opacity: 0; } }
    .notification-dot { animation: pulse-dot 2s infinite; }
    @keyframes pulse-dot { 0%, 100% { transform: scale(0.8); } 50% { transform: scale(1); } }
  `]
})
export class AgentNavbar implements OnInit, OnDestroy {
  @Input() title: string = 'Welcome';
  userName: string = 'User';
  email: string = 'user@example.com';
  userNameInitials: string = 'U';

  private notificationService = inject(NotificationService);

  showNotifications = signal(false);
  notifications = signal<NotificationDTO[]>([]);
  unreadCount = this.notificationService.unreadCount;
  private pollingSub?: Subscription;

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

  ngOnInit() {
    this.loadNotifications();
    this.pollingSub = interval(15000).subscribe(() => this.loadNotifications());
  }

  ngOnDestroy() {
    this.pollingSub?.unsubscribe();
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
        this.notifications.update(list => [...list]);
      }
    });
  }
}
