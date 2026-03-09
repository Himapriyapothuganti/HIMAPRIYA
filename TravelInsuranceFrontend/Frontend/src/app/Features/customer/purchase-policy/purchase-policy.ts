import { Component, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomerService } from '../../../Services/customer.service';
import { Spinner } from '../../admin/components/spinner/spinner';
import { Toast } from '../../admin/components/toast/toast';

@Component({
  selector: 'app-purchase-policy',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, Spinner, Toast],
  providers: [DatePipe],
  templateUrl: './purchase-policy.html'
})
export class PurchasePolicy implements OnInit {
  purchaseForm!: FormGroup;
  product: any = null;
  productId: string = '';

  isLoading = true;
  isSubmitting = false;
  error = '';
  estimatedPremium = 0;

  today: string;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private customerService: CustomerService,
    private datePipe: DatePipe,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) {
    this.today = this.datePipe.transform(new Date(), 'yyyy-MM-dd') || '';
  }

  ngOnInit() {
    this.productId = this.route.snapshot.paramMap.get('productId') || '';
    this.initForm();
    this.loadProductDetails();

  }

  initForm() {
    this.purchaseForm = this.fb.group({
      destination: ['', [Validators.required]],
      startDate: ['', [Validators.required, this.pastDateValidator]],
      endDate: ['', [Validators.required]],
      travellerName: ['', [Validators.required]],
      travellerAge: ['', [Validators.required, Validators.min(1), Validators.max(99)]],
      passportNumber: ['', [Validators.required, Validators.pattern(/^[A-Z][0-9]{7}$/)]],
      kycType: ['PAN', [Validators.required]],
      kycNumber: ['', [Validators.required]]
    }, { validators: this.dateRangeValidator });
  }

  // Rejects dates that are before today
  pastDateValidator(control: any): { [key: string]: boolean } | null {
    if (!control.value) return null;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const selected = new Date(control.value);
    if (selected < today) {
      return { 'pastDate': true };
    }
    return null;
  }

  // Cross-field validation to ensure end date is after start date
  dateRangeValidator(group: FormGroup): { [key: string]: boolean } | null {
    const start = group.get('startDate')?.value;
    const end = group.get('endDate')?.value;
    if (start && end && new Date(start) >= new Date(end)) {
      return { 'dateRangeInvalid': true };
    }
    return null;
  }

  // Dynamic Validation for KYC
  updateKycValidators() {
    const kycType = this.purchaseForm.get('kycType')?.value;
    const kycNumberCtrl = this.purchaseForm.get('kycNumber');

    if (!kycNumberCtrl) return;

    // Swap regex validation patterns based on the selected Indian KYC document type
    if (kycType === 'PAN') {
      kycNumberCtrl.setValidators([Validators.required, Validators.pattern(/^[A-Z]{5}[0-9]{4}[A-Z]{1}$/)]);
    } else if (kycType === 'Aadhaar') {
      kycNumberCtrl.setValidators([Validators.required, Validators.pattern(/^[0-9]{12}$/)]);
    } else if (kycType === 'CKYC') {
      kycNumberCtrl.setValidators([Validators.required, Validators.pattern(/^[0-9]{14}$/)]);
    }

    // Only update value and validity if the field has been touched or dirty to avoid throwing
    // premature validation errors in the UI before the user starts typing.
    if (kycNumberCtrl.dirty || kycNumberCtrl.touched) {
      kycNumberCtrl.updateValueAndValidity({ emitEvent: false });
    }
  }

  loadProductDetails() {
    this.isLoading = true;
    this.customerService.getPolicyProducts().subscribe({
      next: (products) => {
        this.ngZone.run(() => {
          console.log('Available products:', products);
          console.log('Looking for productId:', this.productId);
          const found = products.find(p => p.policyProductId.toString() === this.productId);
          console.log('Found product:', found);
          if (found) {
            this.product = found;
            this.estimatedPremium = this.product.basePremium;

            // Listen to form changes to update the premium live, now that we have a product
            this.purchaseForm.valueChanges.subscribe(() => {
              this.calculateEstimatedPremium();
              this.updateKycValidators();
            });
          } else {
            this.error = "Product not found.";
          }
          this.isLoading = false;
          this.cdr.detectChanges(); // Manually trigger change detection
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          console.error(err);
          this.error = "Failed to load product details.";
          this.isLoading = false;
        });
      }
    });
  }

  calculateEstimatedPremium() {
    if (!this.product) return;

    let premium = this.product.basePremium;
    const vals = this.purchaseForm.value;

    // 1. Calculate Duration modifier
    if (vals.startDate && vals.endDate) {
      const start = new Date(vals.startDate);
      const end = new Date(vals.endDate);
      const diffTime = Math.abs(end.getTime() - start.getTime());
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

      if (diffDays > 0) {
        // Assume base premium covers an initial 30-day block. Pro-rate for extended travel.
        premium = premium * (diffDays / 30);
      }
    }

    // 2. Age Loading
    if (vals.travellerAge) {
      const age = parseInt(vals.travellerAge, 10);
      // Senior citizens carry higher risk, apply 30% loading factor
      if (age > 60) {
        premium = premium * 1.3;
      } else if (age > 40) {
        // Middle-aged bracket, apply 10% loading factor
        premium = premium * 1.1;
      }
    }

    // Never drop the total price below the core base premium, regardless of prorating math
    this.estimatedPremium = Math.max(premium, this.product.basePremium);
  }

  onSubmit() {
    if (this.purchaseForm.invalid) {
      this.purchaseForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const payload = {
      policyProductId: parseInt(this.productId, 10),
      ...this.purchaseForm.value
    };

    this.customerService.purchasePolicy(payload).subscribe({
      next: (res) => {
        this.ngZone.run(() => {
          this.isSubmitting = false;
          // On success, backend returns the newly created policy (which has a policyId)
          // Navigate to payment with that Policy ID.
          this.router.navigate(['/customer/payment', res.policyId]);
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          console.error(err);
          this.error = err.error?.message || "Failed to process purchase request.";
          this.isSubmitting = false;
        });
      }
    });
  }

  get f() { return this.purchaseForm.controls; }

  closeToast() {
    this.error = '';
  }
}
