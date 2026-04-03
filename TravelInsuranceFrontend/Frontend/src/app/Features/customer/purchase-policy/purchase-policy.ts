import { Component, OnInit, NgZone, ChangeDetectorRef, ViewChild, ElementRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomerService } from '../../../Services/customer.service';
import { Spinner } from '../../admin/components/spinner/spinner';
import { Toast } from '../../admin/components/toast/toast';
import { PolicyRequestService } from '../../../Services/policy-request.service';

declare const google: any;

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
  destMultiplier = 1.0;
  ageLoading = 1.0;
  isDestinationSupported = true;

  today: string;

  @ViewChild('mapContainer') mapElement!: ElementRef;
  @ViewChild('destinationInput') inputElement!: ElementRef;

  map: any;
  autocomplete: any;
  marker: any;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private customerService: CustomerService,
    private datePipe: DatePipe,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef,
    private policyRequestService: PolicyRequestService
  ) {
    this.today = this.datePipe.transform(new Date(), 'yyyy-MM-dd') || '';
  }

  ngOnInit() {
    this.productId = this.route.snapshot.paramMap.get('productId') || '';
    this.initForm();
    this.loadProductDetails();
    // Give DOM time to render before initializing map
    setTimeout(() => this.initGoogleMaps(), 500);
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

  initGoogleMaps() {
    if (!this.mapElement || !this.inputElement) return;

    this.ngZone.runOutsideAngular(() => {
      // Default to slightly wider view
      const defaultPos = { lat: 20, lng: 0 };
      this.map = new google.maps.Map(this.mapElement.nativeElement, {
        center: defaultPos,
        zoom: 2,
        styles: this.mapStyles,
        disableDefaultUI: true,
        zoomControl: true
      });

      // Initialize Autocomplete
      this.autocomplete = new google.maps.places.Autocomplete(this.inputElement.nativeElement, {
        types: ['(regions)'],
        fields: ['address_components', 'geometry', 'formatted_address', 'name']
      });

      this.autocomplete.addListener('place_changed', () => {
        this.ngZone.run(() => {
          const place = this.autocomplete.getPlace();
          if (!place.geometry || !place.geometry.location) return;

          // Extract country long_name
          const countryComponent = place.address_components?.find((c: any) => c.types.includes('country'));
          const countryName = countryComponent?.long_name;

          if (countryName) {
            this.purchaseForm.patchValue({ destination: countryName });
          } else {
            this.purchaseForm.patchValue({ destination: place.formatted_address || place.name });
          }

          // Update Map
          this.map.setCenter(place.geometry.location);
          this.map.setZoom(5);

          if (this.marker) this.marker.setMap(null);
          this.marker = new google.maps.Marker({
            position: place.geometry.location,
            map: this.map,
            animation: google.maps.Animation.DROP
          });

          this.calculateEstimatedPremium();
        });
      });
    });
  }

  // Dark Mode Cinematic Map Styles
  mapStyles = [
    { "elementType": "geometry", "stylers": [{ "color": "#212121" }] },
    { "elementType": "labels.icon", "stylers": [{ "visibility": "off" }] },
    { "elementType": "labels.text.fill", "stylers": [{ "color": "#757575" }] },
    { "elementType": "labels.text.stroke", "stylers": [{ "color": "#212121" }] },
    { "featureType": "administrative", "elementType": "geometry", "stylers": [{ "color": "#757575" }] },
    { "featureType": "administrative.country", "elementType": "labels.text.fill", "stylers": [{ "color": "#E8584A" }] },
    { "featureType": "water", "elementType": "geometry", "stylers": [{ "color": "#000000" }] }
  ];

  calculateEstimatedPremium() {
    if (!this.product || this.purchaseForm.invalid) return;

    const vals = this.purchaseForm.value;
    const dto = {
      policyProductId: this.product.policyProductId,
      startDate: vals.startDate,
      endDate: vals.endDate,
      travellerAge: parseInt(vals.travellerAge, 10),
      destination: vals.destination,
      memberCount: 1
    };

    this.policyRequestService.calculatePremium(dto).subscribe({
      next: (res) => {
        this.estimatedPremium = res.estimatedPremium;
        this.destMultiplier = res.destinationMultiplier;
        this.ageLoading = res.ageLoading;
        this.isDestinationSupported = true;
        this.error = '';
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isDestinationSupported = false;
        this.estimatedPremium = this.product.basePremium;
        if (err.status === 400 || err.status === 404) {
          this.error = "This destination is not supported by Talk & Travel.";
        }
        this.cdr.detectChanges();
      }
    });
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
