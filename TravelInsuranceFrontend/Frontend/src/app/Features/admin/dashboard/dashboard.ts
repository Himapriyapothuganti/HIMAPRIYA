import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../../Services/admin.service';
import { Spinner } from '../components/spinner/spinner';
import { Toast } from '../components/toast/toast';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, Spinner, Toast],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  stats: any = null;
  isLoading = true;
  error = '';

  constructor(
    private adminService: AdminService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard() {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.adminService.getDashboardStats().subscribe({
      next: (data) => {
        console.log('Dashboard Data Received:', data);
        this.stats = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error fetching dashboard stats', err);
        this.error = 'Failed to load dashboard data.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  closeToast() {
    this.error = '';
    this.cdr.detectChanges();
  }
}
