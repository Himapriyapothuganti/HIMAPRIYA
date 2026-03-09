import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { PolicyRequestService, AgentPolicyRequestResponse } from '../../../Services/policy-request.service';
import { Spinner } from '../../admin/components/spinner/spinner';

@Component({
  selector: 'app-policy-requests',
  standalone: true,
  imports: [CommonModule, RouterModule, Spinner],
  templateUrl: './policy-requests.html'
})
export class PolicyRequests implements OnInit {
  private policyRequestService = inject(PolicyRequestService);
  private router = inject(Router);

  requests = signal<AgentPolicyRequestResponse[]>([]);
  isLoading = signal<boolean>(true);

  activeTab = signal<'All' | 'Pending' | 'Approved' | 'Rejected' | 'Purchased'>('Pending');
  tabs: Array<'All' | 'Pending' | 'Approved' | 'Rejected' | 'Purchased'> = ['Pending', 'Approved', 'Purchased', 'Rejected', 'All'];

  filteredRequests = computed(() => {
    const currentTab = this.activeTab();
    const allReqs = this.requests();
    if (currentTab === 'All') return allReqs;
    return allReqs.filter(r => r.status === currentTab);
  });

  ngOnInit() {
    this.loadRequests();
  }

  loadRequests() {
    this.isLoading.set(true);
    this.policyRequestService.getAgentRequests().subscribe({
      next: (res) => {
        this.requests.set(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching agent requests:', err);
        // Make sure loading state is dismissed even on error to prevent infinite spin
        this.isLoading.set(false);
      }
    });
  }

  applyFilter(tab: 'All' | 'Pending' | 'Approved' | 'Rejected' | 'Purchased') {
    this.activeTab.set(tab);
  }

  viewDetail(id: number) {
    this.router.navigate(['/agent/policy-requests', id]);
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Pending': return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'Approved': return 'bg-green-100 text-green-800 border-green-200';
      case 'Rejected': return 'bg-red-100 text-red-800 border-red-200';
      case 'Purchased': return 'bg-[#FDE8E0] text-[#E8584A] border-[#E8584A]/20';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  getRiskBadgeClass(level: string): string {
    switch (level) {
      case 'Low': return 'bg-green-50 text-green-700';
      case 'Medium': return 'bg-yellow-50 text-yellow-700';
      case 'High': return 'bg-red-50 text-red-700';
      default: return 'bg-gray-50 text-gray-700';
    }
  }
}
