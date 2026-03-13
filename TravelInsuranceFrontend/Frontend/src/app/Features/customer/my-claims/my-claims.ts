import { Component, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CustomerService } from '../../../Services/customer.service';
import { Spinner } from '../../admin/components/spinner/spinner';
import { Toast } from '../../admin/components/toast/toast';
import { Modal } from '../../admin/components/modal/modal.component';

@Component({
  selector: 'app-my-claims',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, Spinner, Toast, Modal],
  providers: [DatePipe],
  templateUrl: './my-claims.html'
})
export class MyClaims implements OnInit {
  claims: any[] = [];
  activePolicies: any[] = [];

  isLoading = true;
  isSubmitting = false;
  error = '';
  success = '';

  isModalOpen = false;
  claimForm!: FormGroup;
  selectedFiles: File[] = [];

  availableClaimTypes: any[] = [];
  selectedPolicy: any = null;
  selectedClaimConfig: any = null;

  CLAIM_TYPES: any = {
    Silver: [
      { type: 'Emergency Medical', maxINR: null, deductibleINR: 8300 },
      { type: 'Dental', maxINR: 24900, deductibleINR: 12450 },
      { type: 'Hospital Cash', maxINR: 6225, deductibleINR: 0 },
      { type: 'Personal Accident', maxINR: 415000, deductibleINR: 0 },
    ],
    Gold: [
      { type: 'Emergency Medical', maxINR: null, deductibleINR: 8300 },
      { type: 'Dental', maxINR: 24900, deductibleINR: 12450 },
      { type: 'Hospital Cash', maxINR: 6225, deductibleINR: 0 },
      { type: 'Personal Accident', maxINR: 415000, deductibleINR: 0 },
      { type: 'Baggage Loss', maxINR: 16600, deductibleINR: 0 },
      { type: 'Baggage Delay', maxINR: 20750, deductibleINR: 0 },
      { type: 'Flight Cancellation', maxINR: 8300, deductibleINR: 0 },
      { type: 'Trip Cancellation', maxINR: 8300, deductibleINR: 4150 },
      { type: 'Loss of Passport', maxINR: 16600, deductibleINR: 0 },
      { type: 'Flight Delay', maxINR: 8300, deductibleINR: 0 },
      { type: 'Emergency Hotel', maxINR: 83000, deductibleINR: 8300 },
    ],
    Platinum: [
      { type: 'Emergency Medical', maxINR: null, deductibleINR: 8300 },
      { type: 'Dental', maxINR: 24900, deductibleINR: 12450 },
      { type: 'Hospital Cash', maxINR: 6225, deductibleINR: 0 },
      { type: 'Personal Accident', maxINR: 415000, deductibleINR: 0 },
      { type: 'Baggage Loss', maxINR: 16600, deductibleINR: 0 },
      { type: 'Baggage Delay', maxINR: 20750, deductibleINR: 0 },
      { type: 'Flight Cancellation', maxINR: 8300, deductibleINR: 0 },
      { type: 'Trip Cancellation', maxINR: 8300, deductibleINR: 4150 },
      { type: 'Loss of Passport', maxINR: 16600, deductibleINR: 0 },
      { type: 'Flight Delay', maxINR: 8300, deductibleINR: 0 },
      { type: 'Emergency Hotel', maxINR: 83000, deductibleINR: 8300 },
      { type: 'Pre-existing Disease', maxINR: null, deductibleINR: 8300 },
      { type: 'Personal Liability', maxINR: 830000, deductibleINR: 0 },
      { type: 'Missed Connection', maxINR: 41500, deductibleINR: 0 },
      { type: 'Hijack Distress', maxINR: 8300, deductibleINR: 0 },
      { type: 'Emergency Cash', maxINR: 41500, deductibleINR: 0 },
    ]
  };

  constructor(
    private customerService: CustomerService,
    private fb: FormBuilder,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.initForm();
    this.loadData();
    this.setupListeners();
  }

  setupListeners() {
    // When Policy changes, load the correct Claim Types based on Tier
    this.claimForm.get('policyId')?.valueChanges.subscribe(val => {
      this.selectedPolicy = this.activePolicies.find(p => p.policyId == val);
      if (this.selectedPolicy) {
        const tier = this.selectedPolicy.planTier || 'Silver'; // Fallback
        this.availableClaimTypes = this.CLAIM_TYPES[tier] || this.CLAIM_TYPES['Silver'];
        this.claimForm.get('claimType')?.setValue(''); // Reset selected type
        this.selectedClaimConfig = null;
      }
    });

    // When Claim Type changes, update the selected config to show max limiting text
    this.claimForm.get('claimType')?.valueChanges.subscribe(val => {
      if (this.availableClaimTypes.length > 0 && val) {
        this.selectedClaimConfig = this.availableClaimTypes.find(t => t.type === val);
      }
    });
  }


  // Creates a reactive form with validation rules for each field
  initForm() {
    this.claimForm = this.fb.group({
      policyId: ['', Validators.required],                              // Must select a policy
      claimType: ['', Validators.required],                             // Must choose claim type
      incidentDate: ['', Validators.required],                          // Date of the incident
      description: ['', [Validators.required, Validators.minLength(20)]], // Min 20 chars
      claimedAmount: ['', [Validators.required, Validators.min(1)]]       // Must be > 0
    });
  }

  loadData() {
    this.isLoading = true;

    // Load user claims
    this.customerService.getMyClaims().subscribe({
      next: (data) => {
        this.ngZone.run(() => {
          // Sort newest first
          this.claims = data.sort((a, b) => new Date(b.claimDate).getTime() - new Date(a.claimDate).getTime());
        });

        // Also load policies to populate the "Policy to claim against" dropdown
        this.customerService.getMyPolicies().subscribe({
          next: (policies) => {
            this.ngZone.run(() => {
              // Only allow claiming against Active or Expired policies (not pending payment or cancelled)
              this.activePolicies = policies.filter(p => p.status === 'Active' || p.status === 'Expired');
              this.isLoading = false;
              this.cdr.detectChanges();
            });
          },
          error: (err) => {
            this.ngZone.run(() => {
              console.error(err);
              this.error = "Could not load policies for claim dropdown.";
              this.isLoading = false;
              this.cdr.detectChanges();
            });
          }
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          console.error(err);
          this.error = "Failed to load your claims.";
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      }
    });
  }

  openModal() {
    this.isModalOpen = true;
    this.claimForm.reset();
    this.selectedFiles = [];
    this.error = '';
    this.success = '';
    this.selectedPolicy = null;
    this.selectedClaimConfig = null;
    this.availableClaimTypes = [];
  }

  closeModal() {
    this.isModalOpen = false;
    this.error = '';
  }

  onFileChange(event: any) {
    if (event.target.files.length > 0) {
      const files: FileList = event.target.files;
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        // Check size < 5MB and allowed types (mock check, but good practice)
        if (file.size > 5 * 1024 * 1024) {
          this.error = `${file.name} is too large. Max 5MB allowed.`;
        } else {
          this.selectedFiles.push(file);
        }
      }
      event.target.value = ''; // Reset input to allow adding same file again if removed
    }
  }

  removeFile(index: number) {
    this.selectedFiles.splice(index, 1);
  }
  // SUBMIT THE CLAIM
  onSubmit() {
    // Stop if form has any invalid fields
    if (this.claimForm.invalid) {
      this.claimForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    // Pack form values into FormData (needed for file uploads)
    const formData = new FormData();
    formData.append('policyId', this.claimForm.get('policyId')?.value);
    formData.append('claimType', this.claimForm.get('claimType')?.value);
    formData.append('incidentDate', this.claimForm.get('incidentDate')?.value);
    formData.append('description', this.claimForm.get('description')?.value);
    formData.append('claimedAmount', this.claimForm.get('claimedAmount')?.value);

    // Attach each uploaded document
    for (let i = 0; i < this.selectedFiles.length; i++) {
      formData.append('documents', this.selectedFiles[i]);
    }

    // → Hand off to customer.service.ts which fires the POST request
    this.customerService.submitClaim(formData).subscribe({
      next: (res) => {
        this.ngZone.run(() => {
          setTimeout(() => {
            this.success = "Claim submitted successfully!";
            this.isSubmitting = false;
            this.closeModal();
            this.loadData();
            this.cdr.detectChanges();
          }, 0);
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          console.error('Submit Claim HTTP Error:', err);
          
          let errorMsg = "Failed to submit claim. Please try again.";
          if (err.error) {
            if (typeof err.error === 'string') errorMsg = err.error;
            else if (err.error.message) errorMsg = err.error.message;
            else if (err.error.errors) errorMsg = Object.values(err.error.errors).flat().join(' ');
          }
          
          // Use setTimeout to ensure the UI updates in the next tick
          setTimeout(() => {
            this.error = errorMsg;
            this.isSubmitting = false;
            this.cdr.detectChanges();
            this.cdr.markForCheck();
          }, 0);
        });
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'UnderReview': return 'bg-blue-100 text-blue-700';
      case 'PendingDocuments': return 'bg-yellow-100 text-yellow-700';
      case 'Approved': return 'bg-green-100 text-green-700';
      case 'Rejected': return 'bg-red-100 text-red-700';
      case 'PaymentProcessed': return 'bg-purple-100 text-purple-700';
      case 'Closed': return 'bg-gray-100 text-gray-700';
      default: return 'bg-gray-100 text-gray-700';
    }
  }

  get f() { return this.claimForm.controls; }

  closeToast() {
    this.error = '';
    this.success = '';
  }
}
