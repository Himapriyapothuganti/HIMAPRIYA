import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ClaimsOfficerService } from '../services/claims-officer.service';

@Component({
  selector: 'app-officer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="space-y-6 animate-fade-in pb-12">
      <!-- Welcome Message -->
      <div>
        <h2 class="text-3xl font-bold text-[#111] tracking-tight" style="font-family: 'Poppins', sans-serif;">Dashboard Overview</h2>
        <p class="text-gray-500 mt-1">Here is a summary of all claims in the system.</p>
      </div>

      <div *ngIf="isLoading()" class="flex justify-center py-12">
        <div class="w-10 h-10 border-4 border-[#E8584A]/30 border-t-[#E8584A] rounded-full animate-spin"></div>
      </div>

      <ng-container *ngIf="!isLoading()">
        <!-- STATS CARDS ROW 1 -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          
          <!-- Total Claims -->
          <div class="bg-white rounded-2xl p-6 shadow-[0_8px_30px_rgba(0,0,0,0.04)] border border-gray-100 flex flex-col transition-all duration-300">
            <div class="flex items-center justify-between mb-4">
              <div class="w-12 h-12 rounded-2xl bg-blue-50 flex items-center justify-center text-blue-500">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                  <polyline points="14 2 14 8 20 8"></polyline>
                </svg>
              </div>
            </div>
            <div>
              <p class="text-[13px] font-bold text-gray-400 uppercase tracking-wider mb-1">Total Claims</p>
              <h3 class="text-3xl font-extrabold text-[#111] font-poppins">{{ totalClaims() }}</h3>
            </div>
          </div>

          <!-- Under Review -->
          <div class="bg-white rounded-2xl p-6 shadow-[0_8px_30px_rgba(0,0,0,0.04)] border border-gray-100 flex flex-col transition-all duration-300">
            <div class="flex items-center justify-between mb-4">
              <div class="w-12 h-12 rounded-2xl bg-orange-50 flex items-center justify-center text-orange-500">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <circle cx="12" cy="12" r="10"></circle>
                  <polyline points="12 6 12 12 16 14"></polyline>
                </svg>
              </div>
            </div>
            <div>
              <p class="text-[13px] font-bold text-gray-400 uppercase tracking-wider mb-1">Under Review</p>
              <h3 class="text-3xl font-extrabold text-[#111] font-poppins">{{ underReviewClaims() }}</h3>
            </div>
          </div>

          <!-- Pending Documents -->
          <div class="bg-white rounded-2xl p-6 shadow-[0_8px_30px_rgba(0,0,0,0.04)] border border-gray-100 flex flex-col transition-all duration-300">
            <div class="flex items-center justify-between mb-4">
              <div class="w-12 h-12 rounded-2xl bg-yellow-50 flex items-center justify-center text-yellow-500">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"></path>
                  <polyline points="14 2 14 8 20 8"></polyline>
                </svg>
              </div>
            </div>
            <div>
              <p class="text-[13px] font-bold text-gray-400 uppercase tracking-wider mb-1">Pending Documents</p>
              <h3 class="text-3xl font-extrabold text-[#111] font-poppins">{{ pendingDocsClaims() }}</h3>
            </div>
          </div>

          <!-- Approved -->
          <div class="bg-white rounded-2xl p-6 shadow-[0_8px_30px_rgba(0,0,0,0.04)] border border-gray-100 flex flex-col transition-all duration-300">
            <div class="flex items-center justify-between mb-4">
              <div class="w-12 h-12 rounded-2xl bg-green-50 flex items-center justify-center text-green-500">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
                  <polyline points="22 4 12 14.01 9 11.01"></polyline>
                </svg>
              </div>
            </div>
            <div>
              <p class="text-[13px] font-bold text-gray-400 uppercase tracking-wider mb-1">Approved</p>
              <h3 class="text-3xl font-extrabold text-[#111] font-poppins">{{ approvedClaims() }}</h3>
            </div>
          </div>

        </div>

        <!-- STATS CARDS ROW 2 -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6">
          
          <!-- Total Rejected -->
          <div class="bg-white rounded-2xl p-6 shadow-[0_8px_30px_rgba(0,0,0,0.04)] border border-gray-100 flex flex-col transition-all duration-300">
            <div class="flex items-center gap-4">
              <div class="w-12 h-12 rounded-2xl bg-red-50 flex items-center justify-center text-[#E8584A]">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <circle cx="12" cy="12" r="10"></circle>
                  <line x1="15" y1="9" x2="9" y2="15"></line>
                  <line x1="9" y1="9" x2="15" y2="15"></line>
                </svg>
              </div>
              <div>
                <p class="text-[13px] font-bold text-gray-400 uppercase tracking-wider mb-1">Total Rejected</p>
                <div class="flex items-baseline gap-2">
                  <h3 class="text-3xl font-extrabold text-[#111] font-poppins">{{ rejectedClaims() }}</h3>
                  <span class="text-sm text-gray-500 font-medium">claims</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Total Closed -->
          <div class="bg-white rounded-2xl p-6 shadow-[0_8px_30px_rgba(0,0,0,0.04)] border border-gray-100 flex flex-col transition-all duration-300">
            <div class="flex items-center gap-4">
              <div class="w-12 h-12 rounded-2xl bg-gray-50 flex items-center justify-center text-gray-500">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
                  <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
                </svg>
              </div>
              <div>
                <p class="text-[13px] font-bold text-gray-400 uppercase tracking-wider mb-1">Total Closed</p>
                <div class="flex items-baseline gap-2">
                  <h3 class="text-3xl font-extrabold text-[#111] font-poppins">{{ closedClaims() }}</h3>
                  <span class="text-sm text-gray-500 font-medium">claims</span>
                </div>
              </div>
            </div>
          </div>

        </div>

        <!-- RECENT CLAIMS TABLE -->
        <div class="mt-8 bg-white rounded-2xl p-6 shadow-[0_8px_30px_rgba(0,0,0,0.04)] border border-gray-100 overflow-hidden">
          <div class="pb-5 mb-5 border-b border-gray-100 flex items-center justify-between">
            <h3 class="text-lg font-bold text-[#111] font-poppins">Recent Claims</h3>
            <a routerLink="/officer/claims" class="text-sm font-bold text-[#E8584A] hover:text-[#C94035] transition-colors">View All</a>
          </div>
          <div class="overflow-x-auto">
            <table class="w-full text-left border-collapse">
              <thead>
                <tr class="bg-gray-50/50">
                  <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Claim ID</th>
                  <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Customer Name</th>
                  <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Policy Number</th>
                  <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Claim Type</th>
                  <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Amount (₹)</th>
                  <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Status</th>
                  <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100">Date</th>
                  <th class="py-4 px-6 text-xs font-bold text-gray-400 uppercase tracking-wider border-b border-gray-100 text-right">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let claim of recentClaims()" class="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                  <td class="py-4 px-6 text-sm font-bold text-[#111]">#{{ claim.id || claim.claimId }}</td>
                  <td class="py-4 px-6 text-sm font-medium text-gray-700">{{ claim.customerName }}</td>
                  <td class="py-4 px-6 text-sm font-medium text-gray-600">{{ claim.policyNumber }}</td>
                  <td class="py-4 px-6">
                    <span class="px-2.5 py-1 rounded-full text-xs font-bold bg-gray-100 text-gray-600">{{ claim.claimType || 'General' }}</span>
                  </td>
                  <td class="py-4 px-6 text-sm font-bold text-[#111]">₹{{ claim.claimedAmount | number:'1.0-0' }}</td>
                  <td class="py-4 px-6">
                    <span class="px-2.5 py-1 rounded-full text-xs font-bold" [ngClass]="getStatusBadgeClass(claim.status)">
                      {{ claim.status }}
                    </span>
                  </td>
                  <td class="py-4 px-6 text-sm text-gray-500">{{ claim.submittedDate | date:'dd MMM yyyy' }}</td>
                  <td class="py-4 px-6 text-right">
                    <button [routerLink]="['/officer/claims', claim.id || claim.claimId]" 
                            class="px-4 py-1.5 rounded-lg text-sm font-bold text-white bg-[#E8584A] hover:bg-[#C94035] transition-colors">
                      Review
                    </button>
                  </td>
                </tr>
                <tr *ngIf="recentClaims().length === 0">
                  <td colspan="8" class="py-12 text-center text-gray-500 font-medium">
                    No claims assigned to you yet.
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </ng-container>
    </div>
  `
})
export class OfficerDashboardComponent implements OnInit {
  private claimsService = inject(ClaimsOfficerService);

  // Using signals
  allClaims = signal<any[]>([]);
  isLoading = signal<boolean>(true);

  // Computed signals for stats
  totalClaims = computed(() => this.allClaims().length);
  underReviewClaims = computed(() => this.allClaims().filter(c => c.status === 'UnderReview').length);
  pendingDocsClaims = computed(() => this.allClaims().filter(c => c.status === 'PendingDocuments').length);
  approvedClaims = computed(() => this.allClaims().filter(c => c.status === 'Approved').length);
  rejectedClaims = computed(() => this.allClaims().filter(c => c.status === 'Rejected').length);
  closedClaims = computed(() => this.allClaims().filter(c => c.status === 'Closed').length);

  recentClaims = computed(() => {
    // Sort by date desc, then take top 10
    const sorted = [...this.allClaims()].sort((a, b) =>
      new Date(b.submittedDate).getTime() - new Date(a.submittedDate).getTime()
    );
    return sorted.slice(0, 10);
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
        // Error toast here if needed
        this.isLoading.set(false);
      }
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'UnderReview': return 'bg-blue-50 text-blue-600';
      case 'PendingDocuments': return 'bg-yellow-50 text-yellow-600';
      case 'Approved': return 'bg-green-50 text-green-600';
      case 'PaymentProcessed': return 'bg-purple-50 text-purple-600';
      case 'Rejected': return 'bg-red-50 text-red-600';
      case 'Closed': return 'bg-gray-100 text-gray-600';
      default: return 'bg-gray-100 text-gray-600';
    }
  }
}
