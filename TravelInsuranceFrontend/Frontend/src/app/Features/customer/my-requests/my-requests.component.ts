import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PolicyRequestService, PolicyRequestResponse } from '../../../Services/policy-request.service';
import { Spinner } from '../../admin/components/spinner/spinner';
import { PolicyRequestModalComponent } from '../policy-request-modal/policy-request-modal.component';

@Component({
    selector: 'app-my-requests',
    standalone: true,
    imports: [CommonModule, FormsModule, Spinner, PolicyRequestModalComponent],
    templateUrl: './my-requests.component.html'
})
export class MyRequestsComponent implements OnInit {
    private policyRequestService = inject(PolicyRequestService);
    private router = inject(Router);

    requests = signal<PolicyRequestResponse[]>([]);
    isLoading = signal<boolean>(true);
    isPaying = signal<boolean>(false);

    // Edit Modal State
    isEditModalOpen = signal<boolean>(false);
    editData = signal<PolicyRequestResponse | null>(null);

    // Payment confirmation state
    showPaymentModal = signal<boolean>(false);
    selectedRequest = signal<PolicyRequestResponse | null>(null);
    paymentMethod = 'Credit Card';
    paymentOptions = ['Credit Card', 'Debit Card', 'Net Banking', 'UPI', 'Wallet'];
    paymentSuccess = signal<string>('');

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

    ngOnInit() {
        this.loadRequests();
    }

    loadRequests() {
        this.isLoading.set(true);
        this.policyRequestService.getMyRequests().subscribe({
            next: (res) => {
                this.requests.set(res);
                this.isLoading.set(false);
            },
            error: (err) => {
                console.error(err);
                this.isLoading.set(false);
            }
        });
    }

    openPayment(request: PolicyRequestResponse) {
        if (request.status !== 'Approved') return;
        this.selectedRequest.set(request);
        this.showPaymentModal.set(true);
    }

    closePayment() {
        this.showPaymentModal.set(false);
        this.selectedRequest.set(null);
    }

    openEdit(request: PolicyRequestResponse) {
        if (request.status !== 'Rejected') return;
        this.editData.set(request);
        this.isEditModalOpen.set(true);
    }

    closeEdit() {
        this.isEditModalOpen.set(false);
        this.editData.set(null);
    }

    isFormValid(): boolean {
        if (this.paymentMethod === 'Credit Card' || this.paymentMethod === 'Debit Card') {
            return this.cardNumber.length >= 16 && this.cardName.trim().length > 0 && this.cardExpiry.length >= 5 && this.cardCvv.length >= 3;
        }
        if (this.paymentMethod === 'UPI') {
            return this.upiId.includes('@');
        }
        if (this.paymentMethod === 'Net Banking') {
            return this.selectedBank !== '';
        }
        if (this.paymentMethod === 'Wallet') {
            return this.selectedWallet !== '';
        }
        return false;
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

    confirmPayment() {
        const req = this.selectedRequest();
        if (!req) return;

        this.isPaying.set(true);
        this.policyRequestService.payRequest({ policyRequestId: req.policyRequestId, paymentMethod: this.paymentMethod }).subscribe({
            next: (res) => {
                this.isPaying.set(false);
                this.showPaymentModal.set(false);
                this.paymentSuccess.set(`🎉 Policy Activated Successfully!\nYou are now covered. Redirecting...`);

                setTimeout(() => {
                    this.paymentSuccess.set('');
                    this.router.navigate(['/customer/my-policies']);
                }, 3000);
            },
            error: (err) => {
                this.isPaying.set(false);
                alert(err.error?.message || 'Payment failed. Please try again.');
            }
        });
    }

    getStatusBadgeClass(status: string): string {
        switch (status) {
            case 'Pending': return 'bg-yellow-100 text-yellow-800';
            case 'Approved': return 'bg-green-100 text-green-800';
            case 'Rejected': return 'bg-red-100 text-red-800';
            case 'Purchased': return 'bg-blue-100 text-blue-800';
            default: return 'bg-gray-100 text-gray-800';
        }
    }
}
