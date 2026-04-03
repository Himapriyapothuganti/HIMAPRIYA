import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { CustomerSidebar } from '../components/customer-sidebar/customer-sidebar.component';
import { NotificationService, NotificationDTO } from '../../../Services/notification.service';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-customer-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, CustomerSidebar],
  templateUrl: './customer-layout.component.html',
  styles: [`
    .notification-ring {
      animation: pulse-ring 2s cubic-bezier(0.455, 0.03, 0.515, 0.955) infinite;
    }
    @keyframes pulse-ring {
      0% { transform: scale(0.33); }
      80%, 100% { opacity: 0; }
    }
    .notification-dot {
      animation: pulse-dot 2s cubic-bezier(0.455, 0.03, 0.515, 0.955) infinite;
    }
    @keyframes pulse-dot {
      0% { transform: scale(0.8); }
      50% { transform: scale(1); }
      100% { transform: scale(0.8); }
    }
  `]
})
export class CustomerLayoutComponent implements OnInit, OnDestroy {
  pageTitle: string = 'Dashboard';
  userName: string = 'Customer';
  userNameInitials: string = 'C';

  private router = inject(Router);
  private notificationService = inject(NotificationService);

  showNotifications = signal(false);
  notifications = signal<NotificationDTO[]>([]);
  unreadCount = this.notificationService.unreadCount;
  private pollingSub?: Subscription;

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
    // Start polling every 15 seconds
    this.pollingSub = interval(15000).subscribe(() => {
      this.loadNotifications();
    });
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
