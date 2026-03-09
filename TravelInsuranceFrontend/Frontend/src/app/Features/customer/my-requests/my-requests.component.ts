import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PolicyRequestService, PolicyRequestResponse } from '../../../Services/policy-request.service';
import { Spinner } from '../../admin/components/spinner/spinner';

@Component({
    selector: 'app-my-requests',
    standalone: true,
    imports: [CommonModule, FormsModule, Spinner],
    templateUrl: './my-requests.component.html'
})
export class MyRequestsComponent implements OnInit {
    private policyRequestService = inject(PolicyRequestService);
    private router = inject(Router);

    requests = signal<PolicyRequestResponse[]>([]);
    isLoading = signal<boolean>(true);
    isPaying = signal<boolean>(false);

    // Payment confirmation state
    showPaymentModal = signal<boolean>(false);
    selectedRequest = signal<PolicyRequestResponse | null>(null);
    paymentMethod = 'Credit Card';
    paymentOptions = ['Credit Card', 'Debit Card', 'Net Banking', 'UPI', 'Wallet'];
    paymentSuccess = signal<string>('');

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
