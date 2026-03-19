import { Component, Input, Output, EventEmitter, inject, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray } from '@angular/forms';
import { PolicyRequestService } from '../../../Services/policy-request.service';
import * as pdfjsLib from 'pdfjs-dist';

// Configure PDF.js worker
(pdfjsLib as any).GlobalWorkerOptions.workerSrc = `https://cdnjs.cloudflare.com/ajax/libs/pdf.js/5.5.207/pdf.worker.min.js`;

import { Modal } from '../../admin/components/modal/modal.component';
import { Toast } from '../../admin/components/toast/toast';

@Component({
    selector: 'app-policy-request-modal',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, Modal, Toast],
    templateUrl: './policy-request-modal.component.html'
})
export class PolicyRequestModalComponent implements OnChanges {
    @Input() isOpen = false;
    @Input() selectedPlan: any = null;
    @Input() editData: any = null; // New input for edit mode
    @Output() closeModal = new EventEmitter<void>();
    @Output() requestSubmitted = new EventEmitter<any>();

    private fb = inject(FormBuilder);
    private policyRequestService = inject(PolicyRequestService);

    requestForm: FormGroup;
    isSubmitting = false;
    loading = false;

    error: string | null = null;
    success: string | null = null;

    today = new Date().toISOString().split('T')[0];
    estimatedPremium: number | null = null;

    kycFile: File | null = null;
    passportFile: File | null = null;
    otherFile: File | null = null;

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
            return;
        }

        const start = new Date(startDate);
        const end = new Date(endDate);
        const days = Math.max(1, Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24)));

        // Base Premium
        let basePremium = this.selectedPlan.basePremium;

        // Age Loading
        let ageLoading = 1.0;
        let effectiveAge = age;

        // For family, use oldest member
        if (this.selectedPlan.policyType === 'Family') {
            const deps = (this.requestForm.get('dependents') as FormArray)?.value || [];
            if (deps.length > 0) {
                const maxDepAge = Math.max(...deps.map((d: any) => d.Age || 0));
                if (maxDepAge > effectiveAge) effectiveAge = maxDepAge;
            }
        }

        if (effectiveAge > 60) ageLoading = 1.3;
        else if (effectiveAge > 40) ageLoading = 1.1;

        // Multi-Trip and Student Plans are FIXED ANNUAL PREM
        if (this.selectedPlan.policyType === 'Multi-Trip' || this.selectedPlan.policyType === 'Student') {
            this.estimatedPremium = Math.round(basePremium * ageLoading);
            return;
        }

        // Single Trip and Family Plans are PER-TRIP (calculated on 30-day base)
        const daysRatio = days / 30;

        if (this.selectedPlan.policyType === 'Family') {
            const memberCount = 1 + ((this.requestForm.get('dependents') as FormArray)?.length || 0);
            let multiplier = 1.0;
            if (memberCount === 2) multiplier = 1.5;
            else if (memberCount === 3) multiplier = 1.8;
            else if (memberCount === 4) multiplier = 2.0;
            else if (memberCount === 5) multiplier = 2.2;
            else if (memberCount >= 6) multiplier = 2.5;

            this.estimatedPremium = Math.round(basePremium * daysRatio * ageLoading * multiplier);
        } else {
            // Single Trip
            this.estimatedPremium = Math.round(basePremium * daysRatio * ageLoading);
        }
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
                        Relationship: [d.Relationship, Validators.required]
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
            Relationship: ['Spouse', Validators.required]
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
            if (type === 'passport') {
                this.passportFile = file;
                this.processPassportDocument(file);
            }
            if (type === 'other' || type === 'universityLetter') this.otherFile = file;
        }
    }

    async processPassportDocument(file: File) {
        this.loading = true;
        this.error = null;

        if (file.type === 'application/pdf') {
            await this.handlePdfProcessing(file);
        } else if (file.type.startsWith('image/')) {
            this.handleBackendOcr(file);
        }
    }

    async handlePdfProcessing(file: File) {
        try {
            const arrayBuffer = await file.arrayBuffer();
            const pdf = await pdfjsLib.getDocument({ data: arrayBuffer }).promise;
            let fullText = '';
            let hasSelectableText = false;

            for (let i = 1; i <= pdf.numPages; i++) {
                const page = await pdf.getPage(i);
                const textContent = await page.getTextContent();
                const pageText = textContent.items.map((item: any) => item.str).join(' ');
                if (pageText.trim().length > 0) {
                    hasSelectableText = true;
                    fullText += pageText + ' ';
                }
            }

            if (hasSelectableText) {
                console.log('PDF contains selectable text. Extracting...');
                this.parseAndAutofill(fullText);
                this.loading = false;
            } else {
                console.log('PDF appears to be a scan. Rendering first page to image for OCR...');
                await this.renderPdfPageToImage(pdf, file.name);
            }
        } catch (err) {
            console.error('Error parsing PDF:', err);
            this.handleBackendOcr(file);
        }
    }

    async renderPdfPageToImage(pdf: any, originalFileName: string) {
        try {
            const page = await pdf.getPage(1);
            const viewport = page.getViewport({ scale: 2.0 }); // Higher scale for better OCR
            const canvas = document.createElement('canvas');
            const context = canvas.getContext('2d');
            canvas.height = viewport.height;
            canvas.width = viewport.width;

            if (!context) throw new Error('Could not get canvas context');

            await page.render({ canvasContext: context, viewport }).promise;

            canvas.toBlob(async (blob) => {
                if (blob) {
                    const renderedFile = new File([blob], originalFileName.replace('.pdf', '.png'), { type: 'image/png' });
                    this.handleBackendOcr(renderedFile);
                } else {
                    this.error = 'Failed to process scanned PDF.';
                    this.loading = false;
                }
            }, 'image/png');
        } catch (err) {
            console.error('Error rendering PDF page:', err);
            this.error = 'Failed to render PDF for OCR.';
            this.loading = false;
        }
    }

    handleBackendOcr(file: File) {
        const formData = new FormData();
        formData.append('file', file);

        this.policyRequestService.processDocument(formData).subscribe({
            next: (res: any) => {
                if (res.success) {
                    this.autofillFromData(res);
                }
                this.loading = false;
            },
            error: (err: any) => {
                console.error('OCR Error:', err);
                this.loading = false;
            }
        });
    }

    parseAndAutofill(text: string) {
        const data: any = { success: true };

        // 1. Passport Number: Starts with a letter followed by 7-8 digits
        const passportRegex = /([A-PR-WYZ][1-9][0-9]{7})|([A-Z][0-9]{7,8})/i;
        const passportMatch = text.match(passportRegex);
        if (passportMatch) data.documentNumber = passportMatch[0].toUpperCase();

        // 2. Date of Birth: Common formats like 15/02/1990, 15 FEB 1990
        const dobRegex = /(\d{1,2}[-/\s](?:[A-Z]{3}|\d{1,2})[-/\s]\d{4})/i;
        const dobMatch = text.match(dobRegex);
        if (dobMatch) data.dateOfBirth = dobMatch[0];

        // 3. Name Extraction (Digital PDFs)
        // Look for Surname and Given Names specifically
        const surnameRegex = /(?:SURNAME|NAME)[\s:]+([A-Z\s]+)/i;
        const givenNamesRegex = /(?:GIVEN NAMES)[\s:]+([A-Z\s]+)/i;
        
        const surnameMatch = text.match(surnameRegex);
        const givenNamesMatch = text.match(givenNamesRegex);

        if (surnameMatch || givenNamesMatch) {
            let full = "";
            if (givenNamesMatch) full += givenNamesMatch[1].trim();
            if (surnameMatch) full += " " + surnameMatch[1].trim();
            data.fullName = full.trim();
        } else {
            // Fallback: Just look for a large uppercase string
            const fallbackNameRegex = /([A-Z]{3,}\s[A-Z]{3,}(?:\s[A-Z]{3,})?)/;
            const fallbackMatch = text.match(fallbackNameRegex);
            if (fallbackMatch) data.fullName = fallbackMatch[0].trim();
        }

        this.autofillFromData(data);
    }

    autofillFromData(data: any) {
        if (data.fullName) this.requestForm.patchValue({ travellerName: data.fullName });
        if (data.documentNumber) this.requestForm.patchValue({ passportNumber: data.documentNumber });
        
        if (data.dateOfBirth) {
            const date = new Date(data.dateOfBirth);
            if (!isNaN(date.getTime())) {
                // Calculate age
                const today = new Date();
                let age = today.getFullYear() - date.getFullYear();
                const m = today.getMonth() - date.getMonth();
                if (m < 0 || (m === 0 && today.getDate() < date.getDate())) {
                    age--;
                }
                this.requestForm.patchValue({ travellerAge: age });
            }
        }

        if (data.fullName || data.documentNumber) {
            this.success = 'Extracted details from passport!';
            setTimeout(() => this.success = null, 3000);
        }
    }

    submitRequest() {
        if (this.requestForm.invalid) {
            this.requestForm.markAllAsTouched();
            this.error = 'Please fill in all required fields correctly.';
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
                setTimeout(() => {
                    this.requestSubmitted.emit(res);
                    this.close();
                }, 1500);
            },
            error: (err: any) => {
                this.error = err?.error?.message || 'Failed to submit request.';
                this.isSubmitting = false;
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
        this.closeModal.emit();
    }
}
