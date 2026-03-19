import { Component, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CustomerService } from '../../../Services/customer.service';
import { Spinner } from '../../admin/components/spinner/spinner';
import { Toast } from '../../admin/components/toast/toast';
import { InvoiceComponent } from '../invoice/invoice.component';

@Component({
  selector: 'app-my-policies',
  standalone: true,
  imports: [CommonModule, RouterModule, Spinner, Toast, InvoiceComponent],
  providers: [DatePipe],
  templateUrl: './my-policies.html'
})
export class MyPolicies implements OnInit {
  policies: any[] = [];
  filteredPolicies: any[] = [];
  isLoading = true;
  error = '';

  activeTab: string = 'All';
  tabs = ['All', 'Active', 'PendingPayment', 'Expired'];

  selectedInvoice: any = null;
  isInvoiceVisible = false;

  constructor(
    private customerService: CustomerService,
    private router: Router,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.loadPolicies();
  }

  loadPolicies() {
    this.isLoading = true;
    this.customerService.getMyPolicies().subscribe({
      next: (data) => {
        this.ngZone.run(() => {
          this.policies = data.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
          this.applyFilter(this.activeTab);
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          console.error(err);
          this.error = "Failed to load your policies.";
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      }
    });
  }

  applyFilter(tabName: string) {
    this.activeTab = tabName;
    if (tabName === 'All') {
      this.filteredPolicies = [...this.policies];
    } else {
      this.filteredPolicies = this.policies.filter(p => p.status === tabName);
    }
  }

  getTierBadgeClass(tier: string): string {
    switch (tier) {
      case 'Silver': return 'bg-gray-100 text-gray-700 border-gray-200';
      case 'Gold': return 'bg-yellow-100 text-yellow-700 border-yellow-200';
      case 'Platinum': return 'bg-purple-100 text-purple-700 border-purple-200';
      default: return 'bg-blue-50 text-blue-600 border-blue-100';
    }
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Active': return 'bg-green-100 text-green-700';
      case 'PendingPayment': return 'bg-yellow-100 text-yellow-700';
      case 'Expired': return 'bg-gray-100 text-gray-700';
      case 'Cancelled': return 'bg-red-100 text-red-700';
      default: return 'bg-gray-100 text-gray-700';
    }
  }

  copyToClipboard(text: string) {
    navigator.clipboard.writeText(text).then(() => {
      // Optional: show a mini toast
    });
  }

  closeToast() {
    this.error = '';
  }

  viewInvoice(policyId: number) {
    this.isLoading = true;
    this.customerService.getInvoice(policyId).subscribe({
      next: (invoice) => {
        this.selectedInvoice = invoice;
        this.isInvoiceVisible = true;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = "Could not load invoice data.";
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
