import { Component, Input, Output, EventEmitter, inject, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray } from '@angular/forms';
import { PolicyRequestService } from '../../../Services/policy-request.service';
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
    @Output() closeModal = new EventEmitter<void>();
    @Output() requestSubmitted = new EventEmitter<any>();

    private fb = inject(FormBuilder);
    private policyRequestService = inject(PolicyRequestService);

    requestForm: FormGroup;
    isSubmitting = false;

    error: string | null = null;
    success: string | null = null;

    today = new Date().toISOString().split('T')[0];

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
        if (changes['selectedPlan'] && this.selectedPlan) {
            this.setupDynamicForm();
        }
    }

    setupDynamicForm() {
        const type = this.selectedPlan?.policyType;

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
            if (type === 'passport') this.passportFile = file;
            if (type === 'other' || type === 'universityLetter') this.otherFile = file;
        }
    }

    submitRequest() {
        if (this.requestForm.invalid) {
            this.requestForm.markAllAsTouched();
            this.error = 'Please fill in all required fields correctly.';
            return;
        }
        if (!this.kycFile || !this.passportFile) {
            this.error = 'KYC Document and Passport Document are required.';
            return;
        }

        this.isSubmitting = true;
        this.error = null;

        const formData = new FormData();
        formData.append('PolicyProductId', this.selectedPlan.policyProductId.toString());

        Object.keys(this.requestForm.controls).forEach(key => {
            if (key === 'dependents') {
                formData.append('Dependents', JSON.stringify(this.requestForm.get('dependents')?.value));
            } else {
                formData.append(key, this.requestForm.value[key] || '');
            }
        });

        formData.append('kycFile', this.kycFile);
        formData.append('passportFile', this.passportFile);
        
        if (this.otherFile) {
            formData.append('otherFile', this.otherFile);
        } else if (this.selectedPlan?.policyType === 'Student') {
            this.error = 'University Letter is required for Student plans.';
            this.isSubmitting = false;
            return;
        }

        this.policyRequestService.submitRequest(formData).subscribe({
            next: (res: any) => {
                this.isSubmitting = false;
                this.success = 'Request submitted successfully!';
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
