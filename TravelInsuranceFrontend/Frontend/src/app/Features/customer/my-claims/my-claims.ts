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

  // Master list of main claim types
  MASTER_CLAIM_TYPES: any[] = [
    { type: 'Medical Claim', keywords: ['Medical Claim'], deductibleINR: 0 },
    { type: 'Personal Accident Claim', keywords: ['Personal Accident'], maxINR: 415000, deductibleINR: 0 },
    { type: 'Travel Claim', keywords: ['Travel Claim'], deductibleINR: 0 },
    { type: 'Study Related Claim', keywords: ['Study Related Claim'], deductibleINR: 0 }
  ];

  // Auto-detection rules for Travel Claim sub-types
  TRAVEL_SUBTYPES = [
    { name: 'Baggage Loss', keywords: ['baggage', 'lost', 'missing'], maxINR: 16600 },
    { name: 'Baggage Delay', keywords: ['baggage', 'delay', 'late'], maxINR: 20750 },
    { name: 'Baggage Theft', keywords: ['baggage', 'stolen', 'theft'], maxINR: 8300 },
    { name: 'Passport Loss', keywords: ['passport', 'document', 'lost'], maxINR: 16600 },
    { name: 'Flight Cancellation', keywords: ['flight', 'cancel'], maxINR: 8300 },
    { name: 'Flight Delay', keywords: ['flight', 'delay', 'late'], maxINR: 8300 },
    { name: 'Missed Connection', keywords: ['missed', 'connection', 'flight'], maxINR: 41500 },
    { name: 'Trip Cancellation', keywords: ['trip', 'cancel'], maxINR: 8300 },
    { name: 'Emergency Hotel', keywords: ['hotel', 'accommodation'], maxINR: 83000 }
  ];

  detectedSubtype: any = null;

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
    // When Policy changes, dynamically filter Claim Types based on its CoverageDetails
    this.claimForm.get('policyId')?.valueChanges.subscribe(val => {
      this.selectedPolicy = this.activePolicies.find(p => p.policyId == val);
      if (this.selectedPolicy) {
        const coverageStr = (this.selectedPolicy.coverageDetails || '').toLowerCase();
        
        // Filter: Medical, Personal Accident, and Travel are for almost all
        // Study Related is only if CoverageDetails includes it
        this.availableClaimTypes = this.MASTER_CLAIM_TYPES.filter(config => 
            config.keywords.some((kw: string) => coverageStr.includes(kw.toLowerCase()))
        );

        this.claimForm.get('claimType')?.setValue(''); 
        this.selectedClaimConfig = null;
        this.detectedSubtype = null;
      }
    });

    // When Claim Type changes, update the selected config to show max limiting text
    this.claimForm.get('claimType')?.valueChanges.subscribe(val => {
      if (this.availableClaimTypes.length > 0 && val) {
        this.selectedClaimConfig = this.availableClaimTypes.find(t => t.type === val);
        this.detectSubtype();
      }
    });

    // Auto-detect Travel Subtype while typing description
    this.claimForm.get('description')?.valueChanges.subscribe(() => {
        this.detectSubtype();
    });
  }

  detectSubtype() {
    const claimType = this.claimForm.get('claimType')?.value;
    const description = (this.claimForm.get('description')?.value || '').toLowerCase();

    if (claimType !== 'Travel Claim' || !description) {
        this.detectedSubtype = null;
        return;
    }

    let foundSubtype = null;
    for (const sub of this.TRAVEL_SUBTYPES) {
        // Multi-keyword check (e.g., baggage AND lost)
        if (sub.keywords.every(kw => description.includes(kw))) {
            foundSubtype = sub;
            break;
        }
    }

    if (!foundSubtype) {
        // Fallback for no matches
        this.detectedSubtype = { name: 'Travel General', maxINR: 83000 };
    } else {
        this.detectedSubtype = foundSubtype;
    }
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

    if (this.detectedSubtype) {
        formData.append('travelSubtype', this.detectedSubtype.name);
    }

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
