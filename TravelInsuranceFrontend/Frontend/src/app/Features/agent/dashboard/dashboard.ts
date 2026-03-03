import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AgentService } from '../../../Services/agent.service';
import { Spinner } from '../../admin/components/spinner/spinner';
import { Toast } from '../../admin/components/toast/toast';

@Component({
    selector: 'app-agent-dashboard',
    standalone: true,
    imports: [CommonModule, RouterModule, Spinner, Toast],
    templateUrl: './dashboard.html'
})
export class Dashboard implements OnInit {
    isLoading = signal<boolean>(true);
    error = signal<string>('');

    // Dashboard state mapped from API
    dashboardData = signal<any>({
        totalPoliciesAssigned: 0,
        activePolicies: 0,
        pendingPaymentPolicies: 0,
        expiredPolicies: 0,
        totalPremiumCollected: 0,
        totalCommissionEarned: 0,
        assignedPolicies: []
    });

    constructor(private agentService: AgentService) { }

    ngOnInit() {
        this.loadDashboard();
    }

    loadDashboard() {
        this.isLoading.set(true);
        this.error.set('');

        this.agentService.getDashboard().subscribe({
            next: (res) => {
                this.dashboardData.set(res);
                this.isLoading.set(false);
            },
            error: (err) => {
                console.error('Error fetching dashboard data', err);
                // Because the API might not exist yet, we can mock it here for visual testing if needed,
                // but for now, we just show the error.
                this.error.set('Failed to load dashboard data. Please make sure the backend endpoint exists.');
                this.isLoading.set(false);
            }
        });

    }

    closeToast() {
        this.error.set('');
    }

    getStatusBadgeClass(status: string): string {
        switch (status) {
            case 'Active':
                return 'bg-green-100 text-green-700 border-green-200';
            case 'Pending Payment':
            case 'PendingPayment':
                return 'bg-yellow-100 text-yellow-700 border-yellow-200';
            case 'Expired':
                return 'bg-gray-100 text-gray-700 border-gray-200';
            case 'Cancelled':
            case 'Canceled':
                return 'bg-red-100 text-red-700 border-red-200';
            default:
                return 'bg-gray-100 text-gray-700 border-gray-200';
        }
    }
}
