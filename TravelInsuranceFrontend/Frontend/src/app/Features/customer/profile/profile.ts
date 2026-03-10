import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../Models/auth.service';
import { Spinner } from '../../admin/components/spinner/spinner';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, Spinner],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class ProfileComponent implements OnInit {
  customerName: string = '';
  customerEmail: string = '';
  customerInitials: string = '';
  memberSince: string = ''; 
  customerId: string = '';
  phoneNumber: string = '';
  isLoading: boolean = true;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    // Fallbacks initially
    const fullName = localStorage.getItem('fullName') || 'Unknown User';
    this.customerName = fullName;
    this.customerEmail = localStorage.getItem('email') || '';
    this.customerId = localStorage.getItem('nameid') || 'CUS-PENDING';
    
    // Set initials for the avatar based on localStorage before API completes
    const parts = fullName.split(' ');
    this.customerInitials = parts.length > 1
      ? (parts[0].charAt(0) + parts[1].charAt(0)).toUpperCase()
      : fullName.charAt(0).toUpperCase();

    // Fetch live profile details
    this.authService.getProfile().subscribe({
      next: (data) => {
        this.customerName = data.fullName;
        this.customerEmail = data.email;
        this.phoneNumber = data.phoneNumber || 'Not provided';
        this.customerId = data.id;

        const date = new Date(data.createdAt);
        this.memberSince = date.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });

        // Update initials perfectly
        const liveParts = data.fullName.split(' ');
        this.customerInitials = liveParts.length > 1
          ? (liveParts[0].charAt(0) + liveParts[1].charAt(0)).toUpperCase()
          : data.fullName.charAt(0).toUpperCase();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load full profile data', err);
        this.isLoading = false;
      }
    });
  }
}
