import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PolicyRequestService } from '../../../Services/policy-request.service';
import { Modal } from '../../admin/components/modal/modal.component';

@Component({
    selector: 'app-policy-request-modal',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, Modal],
    templateUrl: './policy-request-modal.component.html'
})
export class PolicyRequestModalComponent {
    @Input() isOpen = false;
    @Input() selectedPlan: any = null;
    @Output() closeModal = new EventEmitter<void>();
    @Output() requestSubmitted = new EventEmitter<any>();

    private fb = inject(FormBuilder);
    private policyRequestService = inject(PolicyRequestService);

    requestForm: FormGroup;
    isSubmitting = false;

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
            passportNumber: ['', Validators.required],
            kycType: ['Aadhaar', Validators.required],
            kycNumber: ['', Validators.required]
        });
    }

    onFileSelected(event: any, type: 'kyc' | 'passport' | 'other') {
        const file = event.target.files[0];
        if (file) {
            if (type === 'kyc') this.kycFile = file;
            if (type === 'passport') this.passportFile = file;
            if (type === 'other') this.otherFile = file;
        }
    }

    submitRequest() {
        if (this.requestForm.invalid) {
            this.requestForm.markAllAsTouched();
            return;
        }
        if (!this.kycFile || !this.passportFile) {
            alert('KYC Document and Passport Document are required.');
            return;
        }

        this.isSubmitting = true;

        const formData = new FormData();
        formData.append('PolicyProductId', this.selectedPlan.policyProductId.toString());

        // Append form data
        Object.keys(this.requestForm.controls).forEach(key => {
            formData.append(key, this.requestForm.value[key] || '');
        });

        // Append files
        formData.append('kycFile', this.kycFile);
        formData.append('passportFile', this.passportFile);
        if (this.otherFile) {
            formData.append('otherFile', this.otherFile);
        }

        this.policyRequestService.submitRequest(formData).subscribe({
            next: (res: any) => {
                this.isSubmitting = false;
                this.requestSubmitted.emit(res);
                this.close();
            },
            error: (err: any) => {
                console.error(err);
                alert(err?.error?.message || 'Failed to submit request.');
                this.isSubmitting = false;
            }
        });
    }

    close() {
        this.requestForm.reset({ kycType: 'Aadhaar' });
        this.kycFile = null;
        this.passportFile = null;
        this.otherFile = null;
        this.closeModal.emit();
    }
}
