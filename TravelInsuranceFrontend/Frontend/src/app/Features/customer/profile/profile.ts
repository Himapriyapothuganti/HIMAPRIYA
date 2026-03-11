import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
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

  constructor(
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

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
        try {
          // If response is somehow empty but not an error
          if (!data) return;
          
          this.customerName = data?.fullName || 'Unknown User';
          this.customerEmail = data?.email || '';
          this.phoneNumber = data?.phoneNumber || 'Not provided';
          this.customerId = data?.id || 'CUS-PENDING';

          const date = new Date(data?.createdAt || Date.now());
          this.memberSince = date.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });

          // Update initials perfectly
          const fullNameStr = data?.fullName || 'U';
          const liveParts = fullNameStr.split(' ');
          this.customerInitials = liveParts.length > 1 && liveParts[1].length > 0
            ? (liveParts[0].charAt(0) + liveParts[1].charAt(0)).toUpperCase()
            : fullNameStr.charAt(0).toUpperCase();
        } catch (error) {
          console.error('Error processing profile data', error);
        } finally {
          this.isLoading = false;
          this.cdr.detectChanges(); // Ensure UI un-spins immediately
        }
      },
      error: (err) => {
        console.error('Failed to load full profile data', err);
        this.isLoading = false;
        this.cdr.detectChanges(); // Ensure UI un-spins on error too
      }
    });
  }
}
