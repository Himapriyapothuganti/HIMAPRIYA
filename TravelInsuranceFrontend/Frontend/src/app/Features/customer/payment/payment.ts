import { Component, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CustomerService } from '../../../Services/customer.service';
import { Spinner } from '../../admin/components/spinner/spinner';
import { Toast } from '../../admin/components/toast/toast';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, FormsModule, Spinner, Toast],
  templateUrl: './payment.html'
})
export class Payment implements OnInit {
  policyId: string = '';
  policy: any = null;

  isLoading = true;
  isProcessing = false;
  error = '';
  success = '';

  paymentMethod: string = 'Credit Card';
  paymentOptions = ['Credit Card', 'Debit Card', 'Net Banking', 'UPI', 'Wallet'];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private customerService: CustomerService,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.policyId = this.route.snapshot.paramMap.get('policyId') || '';
    this.loadPolicy();
  }

  loadPolicy() {
    this.isLoading = true;
    this.customerService.getPolicyDetails(this.policyId).subscribe({
      next: (data) => {
        this.ngZone.run(() => {
          this.policy = data;

          // If already active, no need to pay
          if (this.policy.status === 'Active') {
            this.router.navigate(['/customer/my-policies']);
          }

          this.isLoading = false;
          this.cdr.detectChanges(); // Manually trigger change detection
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          console.error(err);
          this.error = "Failed to load policy details.";
          this.isLoading = false;
          this.cdr.detectChanges(); // Manually trigger change detection on error too
        });
      }
    });
  }

  processPayment() {
    this.isProcessing = true;
    const payload = {
      policyId: parseInt(this.policyId, 10),
      paymentMethod: this.paymentMethod
    };

    this.customerService.payPremium(payload).subscribe({
      next: () => {
        this.ngZone.run(() => {
          this.isProcessing = false;
          this.success = `🎉 Policy Activated Successfully!\nPolicy Number: ${this.policy.policyNumber}\nYou are now covered!`;
          this.cdr.detectChanges(); // Update UI to show success message

          setTimeout(() => {
            this.router.navigate(['/customer/my-policies']);
          }, 3000);
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          console.error(err);
          this.error = err.error?.message || "Payment processing failed.";
          this.isProcessing = false;
          this.cdr.detectChanges(); // Update UI to show error message
        });
      }
    });
  }

  closeToast() {
    this.error = '';
    this.success = '';
  }
}
