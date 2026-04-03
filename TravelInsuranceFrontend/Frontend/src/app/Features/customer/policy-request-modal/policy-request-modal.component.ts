import { Component, Input, Output, EventEmitter, inject, OnChanges, SimpleChanges, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray } from '@angular/forms';
import { PolicyRequestService } from '../../../Services/policy-request.service';

import { Modal } from '../../admin/components/modal/modal.component';
import { Toast } from '../../admin/components/toast/toast';

declare var google: any;

@Component({
    selector: 'app-policy-request-modal',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, Modal, Toast],
    templateUrl: './policy-request-modal.component.html',
  styles: [`
    .pop-in { animation: pop-in 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275) both; }
    @keyframes pop-in { 0% { opacity: 0; transform: scale(0.5); } 100% { opacity: 1; transform: scale(1); } }
    .success-bg { animation: success-fade 0.5s ease-out both; }
    @keyframes success-fade { 100% { background-color: rgba(34, 197, 94, 0.05); border-color: #22c55e; border-style: solid; } }
  `]
})
export class PolicyRequestModalComponent implements OnChanges {
    @ViewChild('mapContainer') mapElement!: ElementRef;
    @ViewChild('destinationInput') inputElement!: ElementRef;

    private map: any;
    private autocomplete: any;
    @Input() isOpen = false;
    @Input() selectedPlan: any = null;
    @Input() editData: any = null; // New input for edit mode
    @Output() closeModal = new EventEmitter<void>();
    @Output() requestSubmitted = new EventEmitter<any>();

    private fb = inject(FormBuilder);
    private policyRequestService = inject(PolicyRequestService);

    requestForm: FormGroup;
    isSubmitting = false;

    error: string | null = null;
    success: string | null = null;
    showSuccessAnimation = false;

    today = new Date().toISOString().split('T')[0];
    estimatedPremium: number | null = null;

    kycFile: File | null = null;
    passportFile: File | null = null;
    otherFile: File | null = null;
    
    // Risk Details from Backend
    destMultiplier: number | null = null;
    ageLoading: number | null = null;
    riskLevel: string | null = null;
    isDestinationSupported = true;

    // Revision Tracking
    isRevisionMode = false;
    isKycFlagged = false;
    isPassportFlagged = false;
    isOtherFlagged = false;
    revisionFeedback: string | null = null;
    resubmissionCount = 0;
    maxResubmissions = 2;

    constructor() {
        this.requestForm = this.fb.group({
            destination: ['', Validators.required],
            startDate: ['', Validators.required],
            endDate: ['', Validators.required],
            travellerName: ['', Validators.required],
            travellerAge: ['', [Validators.required, Validators.min(18)]],
            passportNumber: ['', [Validators.required, Validators.minLength(5)]],
            kycType: ['Aadhaar', Validators.required],
            kycNumber: ['', Validators.required]
        }, { validators: this.dateRangeValidator });

        // Dynamic KYC validation
        this.requestForm.get('kycType')?.valueChanges.subscribe(type => {
            const kycNumControl = this.requestForm.get('kycNumber');
            if (!kycNumControl) return;

            kycNumControl.clearValidators();
            kycNumControl.addValidators(Validators.required);

            if (type === 'Aadhaar') {
                kycNumControl.addValidators(Validators.pattern(/^\d{12}$/));
            } else if (type === 'PAN') {
                kycNumControl.addValidators(Validators.pattern(/^[A-Z]{5}[0-9]{4}[A-Z]{1}$/));
            }
            kycNumControl.updateValueAndValidity();
        });

        // Trigger premium preview on relevant changes
        this.requestForm.valueChanges.subscribe(() => {
            this.calculatePreviewPremium();
        });
    }

    calculatePreviewPremium() {
        if (!this.selectedPlan) return;

        const val = this.requestForm.value;
        const age = val.travellerAge;
        const destination = val.destination;
        const startDate = val.startDate;
        const endDate = val.endDate;

        if (!age || !destination || !startDate || !endDate) {
            this.estimatedPremium = null;
            this.destMultiplier = null;
            this.ageLoading = null;
            this.riskLevel = null;
            return;
        }

        const memberCount = 1 + ((this.requestForm.get('dependents') as FormArray)?.length || 0);

        const dto = {
            policyProductId: this.selectedPlan.policyProductId,
            startDate: startDate,
            endDate: endDate,
            travellerAge: age,
            destination: destination,
            memberCount: memberCount
        };

        this.policyRequestService.calculatePremium(dto).subscribe({
            next: (res) => {
                this.estimatedPremium = res.estimatedPremium;
                this.destMultiplier = res.destinationMultiplier;
                this.ageLoading = res.ageLoading;
                this.riskLevel = res.riskLevel;
                this.isDestinationSupported = true;
                this.error = null;
            },
            error: (err) => {
                this.estimatedPremium = null;
                this.destMultiplier = null;
                this.ageLoading = null;
                this.riskLevel = null;
                this.isDestinationSupported = false;
                this.error = err?.error?.message || 'Selected destination is not supported.';
            }
        });
    }

    dateRangeValidator(group: FormGroup) {
        const start = group.get('startDate')?.value;
        const end = group.get('endDate')?.value;
        if (start && end && new Date(start) > new Date(end)) {
            return { dateRangeInvalid: true };
        }
        return null;
    }

    get f() { return this.requestForm.controls; }

    ngOnChanges(changes: SimpleChanges) {
        if (changes['selectedPlan'] && this.selectedPlan && !this.editData) {
            this.setupDynamicForm();
        }

        if (changes['editData'] && this.editData) {
            this.patchFormForEdit();
        }

        if (changes['isOpen'] && this.isOpen) {
            // Short delay to ensure modal is rendered and animations are ready
            setTimeout(() => this.initGoogleMaps(), 400);
        }
    }

    initGoogleMaps() {
        if (typeof google === 'undefined' || !this.mapElement || !this.inputElement) return;

        // Initialize Map in World View
        const mapOptions = {
            center: { lat: 20, lng: 0 },
            zoom: 2,
            mapTypeId: 'roadmap',
            disableDefaultUI: true,
            gestureHandling: 'cooperative',
            styles: [
                {
                    "featureType": "all",
                    "elementType": "labels.text.fill",
                    "stylers": [{ "color": "#7c93a3" }, { "lightness": "-10" }]
                },
                {
                    "featureType": "administrative.country",
                    "elementType": "geometry.stroke",
                    "stylers": [{ "color": "#E8584A" }, { "weight": "0.5" }, { "opacity": "0.2" }]
                },
                {
                    "featureType": "landscape",
                    "elementType": "geometry.fill",
                    "stylers": [{ "color": "#f5f5f5" }, { "lightness": "0" }]
                },
                {
                    "featureType": "water",
                    "elementType": "geometry",
                    "stylers": [{ "color": "#e9e9e9" }, { "lightness": "17" }]
                }
            ]
        };

        try {
            this.map = new google.maps.Map(this.mapElement.nativeElement, mapOptions);

            // Initialize Autocomplete
            this.autocomplete = new google.maps.places.Autocomplete(this.inputElement.nativeElement, {
                types: ['(regions)'], // Use regions to better Capture country-level searches
                fields: ['address_components', 'geometry', 'formatted_address', 'name']
            });

            // Bind Autocomplete to Map
            this.autocomplete.addListener('place_changed', () => {
                const place = this.autocomplete.getPlace();
                if (!place.geometry || !place.geometry.location) return;

                // Extract country long_name as per user request
                const countryComponent = place.address_components?.find((c: any) => c.types.includes('country'));
                const countryName = countryComponent?.long_name;

                if (countryName) {
                    // Update Form Value with exact Country Name for DB matching
                    this.requestForm.patchValue({ destination: countryName });
                } else {
                    this.requestForm.patchValue({ destination: place.formatted_address || place.name });
                }

                // Trigger cinematic zoom
                this.zoomToLocation(place.geometry.location);
            });
        } catch (e) {
            console.error("Google Maps Initialization Failed:", e);
        }
    }

    zoomToLocation(location: any) {
        if (!this.map) return;

        // Start from a neutral zoom to create the "pan" effect
        this.map.panTo(location);

        // Custom Step-wise zoom animation for "Cinematic" feel
        let currentZoom = 2;
        const targetZoom = 12;

        const zoomInterval = setInterval(() => {
            if (currentZoom >= targetZoom) {
                clearInterval(zoomInterval);
            } else {
                currentZoom++;
                this.map.setZoom(currentZoom);
            }
        }, 120);

        // Add a premium marker
        new google.maps.Marker({
            position: location,
            map: this.map,
            animation: google.maps.Animation.DROP,
            icon: {
                path: google.maps.SymbolPath.CIRCLE,
                scale: 10,
                fillColor: "#E8584A",
                fillOpacity: 1,
                strokeWeight: 2,
                strokeColor: "#FFFFFF",
            }
        });
    }

    patchFormForEdit() {
        // Ensure dynamic controls exist first
        this.setupDynamicForm();

        // Patch main fields
        this.requestForm.patchValue({
            destination: this.editData.destination,
            startDate: this.editData.startDate.split('T')[0],
            endDate: this.editData.endDate.split('T')[0],
            travellerName: this.editData.travellerName,
            travellerAge: this.editData.travellerAge,
            passportNumber: this.editData.passportNumber,
            kycType: this.editData.kycType,
            kycNumber: this.editData.kycNumber,
            universityName: this.editData.universityName,
            studentId: this.editData.studentId,
            tripFrequency: this.editData.tripFrequency
        });

        // Handle Revision Mode
        if (this.editData.status === 'Needs Revision') {
            this.isRevisionMode = true;
            this.revisionFeedback = this.editData.rejectionReason;
            this.resubmissionCount = this.editData.resubmissionCount || 0;
            this.maxResubmissions = this.editData.maxResubmissions || 2;
            
            const flagged = this.editData.requestedDocTypes || '';
            this.isKycFlagged = flagged.includes('KYC');
            this.isPassportFlagged = flagged.includes('Passport');
            this.isOtherFlagged = flagged.includes('Other');
        } else {
            this.isRevisionMode = false;
            this.isKycFlagged = false;
            this.isPassportFlagged = false;
            this.isOtherFlagged = false;
            this.revisionFeedback = null;
        }

        // Handle Dependents
        if (this.editData.dependents) {
            try {
                const dependents = JSON.parse(this.editData.dependents);
                const depArray = this.dependents;
                depArray.clear();
                dependents.forEach((d: any) => {
                    depArray.push(this.fb.group({
                        Name: [d.Name, Validators.required],
                        Age: [d.Age, [Validators.required, Validators.min(1)]],
                        Relationship: [d.Relationship, Validators.required],
                        OtherRelationship: [d.OtherRelationship || '']
                    }));
                });
            } catch (e) {
                console.error('Error parsing dependents', e);
            }
        }

        this.calculatePreviewPremium();
    }

    setupDynamicForm() {
        const type = this.selectedPlan?.policyType || this.editData?.policyType;

        this.requestForm.removeControl('universityName');
        this.requestForm.removeControl('studentId');
        this.requestForm.removeControl('tripFrequency');
        this.requestForm.removeControl('dependents');

        if (type === 'Student') {
            this.requestForm.addControl('universityName', this.fb.control('', Validators.required));
            this.requestForm.addControl('studentId', this.fb.control('', Validators.required));
        } else if (type === 'Multi-Trip') {
            this.requestForm.addControl('tripFrequency', this.fb.control('Multiple trips (max 30 days per trip)', Validators.required));
        } else if (type === 'Family') {
            const dependentsArray = this.fb.array([]);
            this.requestForm.addControl('dependents', dependentsArray);
        }

        this.calculatePreviewPremium();
    }

    get dependents(): FormArray {
        return this.requestForm.get('dependents') as FormArray;
    }

    createDependent(): FormGroup {
        return this.fb.group({
            Name: ['', Validators.required],
            Age: ['', [Validators.required, Validators.min(1)]],
            Relationship: ['Spouse', Validators.required],
            OtherRelationship: ['']
        });
    }

    addDependent() {
        if (this.dependents.length < 5) {
            this.dependents.push(this.createDependent());
        } else {
            this.error = 'Family Plan allows a maximum of 6 members.';
        }
    }

    removeDependent(index: number) {
        this.dependents.removeAt(index);
    }

    onFileSelected(event: any, type: 'kyc' | 'passport' | 'other' | 'universityLetter') {
        const file = event.target.files[0];
        if (file) {
            if (type === 'kyc') this.kycFile = file;
            if (type === 'passport') this.passportFile = file;
            if (type === 'other' || type === 'universityLetter') this.otherFile = file;
        }
    }


    clearFile(type: string) {
    if (type === 'kyc') this.kycFile = null;
    else if (type === 'passport') this.passportFile = null;
    else if (type === 'other') this.otherFile = null;
    else if (type === 'universityLetter') this.otherFile = null;
  }

  submitRequest() {
        if (this.requestForm.invalid || !this.isDestinationSupported) {
            this.requestForm.markAllAsTouched();
            this.error = !this.isDestinationSupported 
                ? 'The selected destination is not supported by Talk & Travel.' 
                : 'Please fill in all required fields correctly.';
            return;
        }

        // Age Eligibility Check for Student Plan
        if (this.selectedPlan?.policyType === 'Student' && this.requestForm.value.travellerAge > 35) {
            this.error = 'Student plans are only available for travellers aged 35 and below.';
            return;
        }

        // In Edit Mode, files are optional if not changed
        if (!this.editData) {
            if (!this.kycFile || !this.passportFile) {
                this.error = 'KYC Document and Passport Document are required.';
                return;
            }
        }

        this.isSubmitting = true;
        this.error = null;

        const formData = new FormData();
        formData.append('PolicyProductId', (this.selectedPlan?.policyProductId || this.editData?.policyProductId).toString());

        Object.keys(this.requestForm.controls).forEach(key => {
            if (key === 'dependents') {
                formData.append('Dependents', JSON.stringify(this.requestForm.get('dependents')?.value));
            } else {
                formData.append(key, this.requestForm.value[key] || '');
            }
        });

        if (this.kycFile) formData.append('kycFile', this.kycFile);
        if (this.passportFile) formData.append('passportFile', this.passportFile);
        if (this.otherFile) formData.append('otherFile', this.otherFile);

        const request$ = this.editData
            ? this.policyRequestService.updateRequest(this.editData.policyRequestId, formData)
            : this.policyRequestService.submitRequest(formData);

        request$.subscribe({
            next: (res: any) => {
                this.isSubmitting = false;
                this.success = this.editData ? 'Request updated successfully!' : 'Request submitted successfully!';
                this.showSuccessAnimation = true;
                setTimeout(() => {
                    this.requestSubmitted.emit(res);
                    this.close();
                }, 3000); // Wait for animation
            },
            error: (err: any) => {
                this.error = err?.error?.message || err?.error || 'Failed to submit request.';
                this.isSubmitting = false;
                
                // If limit reached, close modal and let parent handle the status change
                if (err?.error?.includes('Maximum resubmission attempts')) {
                    setTimeout(() => {
                        this.requestSubmitted.emit(null); 
                        this.close();
                    }, 2000);
                }
            }
        });
    }

    closeToast() {
        this.error = null;
        this.success = null;
    }

    close() {
        this.requestForm.reset({ kycType: 'Aadhaar' });
        this.kycFile = null;
        this.passportFile = null;
        this.otherFile = null;
        this.error = null;
        this.success = null;
        this.showSuccessAnimation = false;
        this.closeModal.emit();
    }
}
