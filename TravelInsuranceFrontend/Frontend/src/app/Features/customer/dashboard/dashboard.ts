import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CustomerService } from '../../../Services/customer.service';
import { Spinner } from '../../admin/components/spinner/spinner';
import { Toast } from '../../admin/components/toast/toast';

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, Spinner, Toast],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  policies: any[] = [];
  recentPolicies: any[] = [];
  claims: any[] = [];
  recentClaims: any[] = [];

  isLoading = true;
  error = '';
  userName = 'Traveller';

  // Stats
  totalPolicies = 0;
  activePolicies = 0;
  pendingPaymentPolicies = 0;

  totalClaims = 0;
  underReviewClaims = 0;
  approvedClaims = 0;

  constructor(
    private customerService: CustomerService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {
    const fullName = localStorage.getItem('fullName');
    if (fullName) this.userName = fullName.split(' ')[0];
  }

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading = true;

    this.customerService.getMyPolicies().subscribe({
      next: (policyData) => {
        this.ngZone.run(() => {
          this.policies = policyData;
          this.processPolicyStats();
        });

        this.customerService.getMyClaims().subscribe({
          next: (claimData) => {
            this.ngZone.run(() => {
              this.claims = claimData;
              this.processClaimStats();
              this.isLoading = false;
              this.cdr.detectChanges();
            });
          },
          error: (err) => {
            this.ngZone.run(() => {
              console.error(err);
              this.error = 'Failed to load claims.';
              this.isLoading = false;
              this.cdr.detectChanges();
            });
          }
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          console.error(err);
          this.error = 'Failed to load policies.';
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      }
    });
  }

  processPolicyStats() {
    this.totalPolicies = this.policies.length;
    this.activePolicies = this.policies.filter(p => p.status === 'Active').length;
    this.pendingPaymentPolicies = this.policies.filter(p => p.status === 'PendingPayment').length;

    this.recentPolicies = [...this.policies]
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
      .slice(0, 5);
  }

  processClaimStats() {
    this.totalClaims = this.claims.length;
    this.underReviewClaims = this.claims.filter(c => c.status === 'UnderReview').length;
    this.approvedClaims = this.claims.filter(c => c.status === 'Approved').length;

    this.recentClaims = [...this.claims]
      .sort((a, b) => new Date(b.claimDate).getTime() - new Date(a.claimDate).getTime())
      .slice(0, 5);
  }

  closeToast() {
    this.error = '';
  }
}
