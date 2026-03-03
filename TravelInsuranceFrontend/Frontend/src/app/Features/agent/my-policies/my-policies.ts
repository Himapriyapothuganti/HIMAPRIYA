import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AgentService } from '../../../Services/agent.service';
import { Spinner } from '../../admin/components/spinner/spinner';
import { Toast } from '../../admin/components/toast/toast';

@Component({
    selector: 'app-agent-policies',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule, Spinner, Toast],
    templateUrl: './my-policies.html'
})
export class MyPolicies implements OnInit {
    isLoading = signal<boolean>(true);
    error = signal<string>('');

    policies = signal<any[]>([]);
    searchQuery = signal<string>('');
    activeTab = signal<string>('All');

    // Compute filtered policies based on tab and search
    filteredPolicies = computed(() => {
        let filtered = this.policies();

        // Tab filter
        if (this.activeTab() !== 'All') {
            filtered = filtered.filter(p => p.status === this.activeTab());
        }

        // Search filter
        const query = this.searchQuery().toLowerCase();
        if (query) {
            filtered = filtered.filter(p =>
                p.policyNumber.toLowerCase().includes(query) ||
                p.customerName.toLowerCase().includes(query)
            );
        }

        return filtered;
    });

    constructor(private agentService: AgentService) { }

    ngOnInit() {
        this.loadPolicies();
    }

    loadPolicies() {
        this.isLoading.set(true);
        this.agentService.getMyPolicies().subscribe({
            next: (data) => {
                this.policies.set(data);
                this.isLoading.set(false);
            },
            error: (err) => {
                console.error('Error fetching agent policies', err);
                // Fallback fake data if API isn't fully ready yet for testing UI 
                this.error.set('Failed to load assigned policies. Ensure the endpoint exists.');
                this.isLoading.set(false);
            }
        });
    }

    copyToClipboard(text: string) {
        navigator.clipboard.writeText(text);
        // Could integrate a tiny toast here specifically for "Copied!" if needed
    }

    setTab(tab: string) {
        this.activeTab.set(tab);
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
