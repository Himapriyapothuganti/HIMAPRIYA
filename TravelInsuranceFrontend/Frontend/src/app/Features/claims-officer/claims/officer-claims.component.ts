import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ClaimsOfficerService } from '../services/claims-officer.service';

@Component({
  selector: 'app-officer-claims',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="space-y-6 animate-fade-in pb-12" style="font-family: 'Poppins', sans-serif;">
      <!-- Header -->
      <div class="flex items-end justify-between">
        <div>
          <h2 class="text-3xl font-bold text-[#111] tracking-tight" style="font-family: 'Poppins', sans-serif;">All Assigned Claims</h2>
          <p class="text-gray-500 mt-1">Manage and review all customer claims assigned to you.</p>
        </div>
        
        <!-- Search -->
        <div class="relative w-72">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg class="h-5 w-5 text-gray-400" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clip-rule="evenodd" />
            </svg>
          </div>
          <input type="text" [ngModel]="searchQuery()" (ngModelChange)="searchQuery.set($event)"
                 class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-xl leading-5 bg-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#E8584A]/20 focus:border-[#E8584A] transition-colors sm:text-sm font-medium"
                 placeholder="Search ID, Name, Policy...">
        </div>
      </div>

      <!-- Filters -->
      <div class="flex items-center gap-2 overflow-x-auto pb-2 scrollbar-none">
        <button *ngFor="let tab of filterTabs"
                (click)="activeTab.set(tab.value)"
                class="px-5 py-2.5 rounded-xl text-sm font-bold transition-all duration-300 whitespace-nowrap"
                [ngClass]="activeTab() === tab.value ? 'bg-[#111] text-white shadow-md' : 'bg-white text-gray-600 hover:bg-gray-50 border border-gray-100/50 hover:border-gray-200'">
          {{ tab.label }}
          <span *ngIf="activeTab() === tab.value" class="ml-1.5 px-2 py-0.5 rounded-full bg-white/20 text-xs">
            {{ filteredClaims().length }}
          </span>
        </button>
      </div>

      <!-- Claims Table Card -->
      <div class="bg-white rounded-2xl shadow-[0_8px_30px_rgba(0,0,0,0.04)] border border-gray-100 overflow-hidden relative min-h-[400px]">
        
        <div *ngIf="isLoading()" class="absolute inset-0 bg-white/80 backdrop-blur-sm z-10 flex flex-col items-center justify-center">
          <div class="w-10 h-10 border-4 border-[#E8584A]/30 border-t-[#E8584A] rounded-full animate-spin"></div>
          <p class="mt-4 text-sm font-bold text-gray-500">Loading claims...</p>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="bg-gray-50/50">
                <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Claim ID</th>
                <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Customer Name</th>
                <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Policy Number</th>
                <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Claim Type</th>
                <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Claimed (₹)</th>
                <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Approved (₹)</th>
                <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Status</th>
                <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Date</th>
                <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100 text-right">Action</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let claim of filteredClaims()" class="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                <td class="py-4 px-6 text-sm font-bold text-[#111]">#{{ claim.id || claim.claimId }}</td>
                <td class="py-4 px-6 text-sm font-medium text-gray-700">{{ claim.customerName }}</td>
                <td class="py-4 px-6 text-sm font-medium text-gray-600">{{ claim.policyNumber }}</td>
                <td class="py-4 px-6">
                  <span class="px-2.5 py-1 rounded-full text-xs font-bold bg-gray-100 text-gray-600">{{ claim.claimType || 'General' }}</span>
                </td>
                <td class="py-4 px-6 text-sm font-bold text-[#111]">₹{{ claim.claimedAmount | number:'1.0-0' }}</td>
                <td class="py-4 px-6 text-sm font-medium text-gray-600">
                  <span *ngIf="claim.approvedAmount > 0" class="text-green-600 font-bold">₹{{ claim.approvedAmount | number:'1.0-0' }}</span>
                  <span *ngIf="!claim.approvedAmount || claim.approvedAmount === 0">—</span>
                </td>
                <td class="py-4 px-6">
                  <span class="px-3 py-1 rounded-full text-[11px] font-black uppercase tracking-wider whitespace-nowrap inline-block" [ngClass]="getStatusBadgeClass(claim.status)">
                    {{ formatStatus(claim.status) }}
                  </span>
                </td>
                <td class="py-4 px-6 text-sm text-gray-500">{{ claim.submittedAt | date:'dd MMM yyyy' }}</td>
                <td class="py-4 px-6 text-right">
                  <button [routerLink]="['/officer/claims', claim.id || claim.claimId]" 
                          class="px-4 py-1.5 rounded-lg text-sm font-bold text-white bg-[#E8584A] hover:bg-[#C94035] transition-colors shadow-sm">
                    Review
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          
          <!-- Empty State -->
          <div *ngIf="!isLoading() && filteredClaims().length === 0" class="py-20 flex flex-col items-center justify-center text-center px-4">
            <div class="w-20 h-20 bg-gray-50 rounded-full flex items-center justify-center text-gray-300 mb-4">
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
                <line x1="9" y1="3" x2="9" y2="21"></line>
              </svg>
            </div>
            <h3 class="text-lg font-bold text-[#111] mb-1 font-poppins">No claims found.</h3>
            <p class="text-gray-500 font-medium max-w-sm">
              Claims submitted by customers matching your filters will appear here.
            </p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class OfficerClaimsComponent implements OnInit {
  private claimsService = inject(ClaimsOfficerService);

  // State using signals
  allClaims = signal<any[]>([]);
  isLoading = signal<boolean>(true);
  searchQuery = signal<string>('');
  activeTab = signal<string>('All');

  filterTabs = [
    { label: 'All', value: 'All' },
    { label: 'Under Review', value: 'UnderReview' },
    { label: 'Pending Docs', value: 'PendingDocuments' },
    { label: 'Approved', value: 'Approved' },
    { label: 'Rejected', value: 'Rejected' },
    { label: 'Closed', value: 'Closed' }
  ];

  filteredClaims = computed(() => {
    let result = this.allClaims();

    const tabItem = this.activeTab();
    if (tabItem !== 'All') {
      result = result.filter(c => c.status === tabItem);
    }

    const term = this.searchQuery().toLowerCase().trim();
    if (term) {
      result = result.filter(c =>
        (c.id?.toString().includes(term) || (c.claimId && c.claimId.toString().includes(term))) ||
        (c.customerName && c.customerName.toLowerCase().includes(term)) ||
        (c.policyNumber && c.policyNumber.toLowerCase().includes(term))
      );
    }

    return result;
  });

  ngOnInit() {
    this.fetchClaims();
  }

  fetchClaims() {
    this.isLoading.set(true);
    this.claimsService.getAssignedClaims().subscribe({
      next: (data: any[]) => {
        this.allClaims.set(data);
        this.isLoading.set(false);
      },
      error: (err: any) => {
        console.error('Error fetching claims', err);
        this.isLoading.set(false);
      }
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'UnderReview': return 'bg-blue-50 text-blue-600 border border-blue-100';
      case 'PendingDocuments': return 'bg-yellow-50 text-yellow-600 border border-yellow-100';
      case 'Approved': return 'bg-emerald-50 text-emerald-600 border border-emerald-100';
      case 'PaymentProcessed': return 'bg-purple-50 text-purple-600 border border-purple-100';
      case 'Rejected': return 'bg-rose-50 text-rose-600 border border-rose-100';
      case 'Closed': return 'bg-gray-50 text-gray-500 border border-gray-200';
      default: return 'bg-gray-50 text-gray-500 border border-gray-200';
    }
  }

  formatStatus(status: string): string {
    if (!status) return 'Unknown';
    return status.replace(/([A-Z])/g, ' $1').trim();
  }
}
