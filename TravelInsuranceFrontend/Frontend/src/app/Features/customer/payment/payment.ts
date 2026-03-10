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

  // Payment Details
  cardNumber: string = '';
  cardName: string = '';
  cardExpiry: string = '';
  cardCvv: string = '';
  upiId: string = '';
  selectedBank: string = '';
  selectedWallet: string = '';

  banks = ['HDFC Bank', 'ICICI Bank', 'State Bank of India', 'Axis Bank', 'Kotak Mahindra Bank'];
  wallets = ['Amazon Pay', 'Paytm', 'PhonePe', 'MobiKwik'];

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
          this.policy = data;

          // If already active, no need to pay
          if (this.policy.status === 'Active') {
            this.router.navigate(['/customer/my-policies']);
          }

          this.isLoading = false;
      },
      error: (err) => {
          console.error(err);
          this.error = "Failed to load policy details.";
          this.isLoading = false;
      }
    });
  }

  processPayment() {
    this.isProcessing = true;
    const payload = {
      policyId: Number(this.policyId),
      paymentMethod: this.paymentMethod
    };

    this.customerService.payPremium(payload).subscribe({
      next: () => {
          this.isProcessing = false;
          this.success = `🎉 Policy Activated Successfully!\nPolicy Number: ${this.policy.policyNumber}\nYou are now covered!`;

          setTimeout(() => {
            this.router.navigate(['/customer/my-policies']);
          }, 3000);
      },
      error: (err) => {
          console.error(err);
          this.error = err.error?.message || "Payment processing failed.";
          this.isProcessing = false;
      }
    });
  }

  formatCardNumber(event: any) {
    let input = event.target.value.replace(/\D/g, '').substring(0, 16);
    input = input.replace(/(\d{4})/g, '$1 ').trim();
    this.cardNumber = input;
  }

  formatExpiry(event: any) {
    let input = event.target.value.replace(/\D/g, '').substring(0, 4);
    if (input.length > 2) {
      input = input.substring(0, 2) + '/' + input.substring(2, 4);
    }
    this.cardExpiry = input;
  }

  formatCvv(event: any) {
    this.cardCvv = event.target.value.replace(/\D/g, '').substring(0, 4);
  }

  isFormValid(): boolean {
    if (this.paymentMethod === 'Credit Card' || this.paymentMethod === 'Debit Card') {
      return this.cardNumber.length >= 15 && this.cardName.length > 0 && this.cardExpiry.length === 5 && this.cardCvv.length >= 3;
    } else if (this.paymentMethod === 'Net Banking') {
      return this.selectedBank.length > 0;
    } else if (this.paymentMethod === 'UPI') {
      return this.upiId.includes('@');
    } else if (this.paymentMethod === 'Wallet') {
      return this.selectedWallet.length > 0;
    }
    return false;
  }

  closeToast() {
    this.error = '';
    this.success = '';
  }
}
