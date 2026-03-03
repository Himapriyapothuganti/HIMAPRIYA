import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ClaimsOfficerService } from '../services/claims-officer.service';

@Component({
  selector: 'app-officer-claim-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="space-y-6 animate-fade-in pb-12 overflow-x-hidden font-sans">
      
      <!-- Header Area -->
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-4">
          <a routerLink="/officer/claims" class="w-10 h-10 rounded-xl bg-white border border-gray-200 flex items-center justify-center text-gray-500 hover:text-[#E8584A] hover:border-[#E8584A]/30 hover:bg-[#FDF4F0] transition-all duration-300">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="19" y1="12" x2="5" y2="12"></line>
              <polyline points="12 19 5 12 12 5"></polyline>
            </svg>
          </a>
          <div>
            <h2 class="text-2xl font-bold text-[#111] tracking-tight font-poppins flex items-center gap-3">
              Claim #{{ claim()?.id || claim()?.claimId || '...' }}
              <span *ngIf="claim()" class="px-3 py-1 rounded-full text-sm font-bold tracking-normal" [ngClass]="getStatusBadgeClass(claim().status)">
                {{ claim().status }}
              </span>
            </h2>
            <p class="text-gray-500 mt-1 text-sm font-medium">Review claim details and process accordingly.</p>
          </div>
        </div>
      </div>

      <div *ngIf="isLoading()" class="flex justify-center py-20">
        <div class="w-10 h-10 border-4 border-[#E8584A]/30 border-t-[#E8584A] rounded-full animate-spin"></div>
      </div>

      <ng-container *ngIf="!isLoading() && claim()">
        
        <!-- TWO COLUMN LAYOUT -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
          
          <!-- LEFT COLUMN - Claim Info Card -->
          <div class="lg:col-span-2 space-y-6">
            <div class="bg-white rounded-2xl shadow-[0_8px_30px_rgba(0,0,0,0.04)] border border-gray-100 p-6 md:p-8">
              <div class="flex items-start justify-between mb-8">
                <div>
                  <h3 class="text-xl font-bold text-[#111] font-poppins mb-1">Claim Information</h3>
                  <p class="text-sm text-gray-500">Submitted on {{ claim().submittedDate | date:'dd MMM yyyy' }}</p>
                </div>
                <!-- Claim Type Badge -->
                <span class="px-4 py-1.5 rounded-full text-xs font-bold bg-gray-100 text-gray-600 uppercase tracking-wider">
                  {{ claim().claimType || 'General' }}
                </span>
              </div>

              <!-- Amounts Grid -->
              <div class="grid grid-cols-2 gap-4 mb-8">
                <div class="p-5 rounded-2xl bg-gray-50/50 border border-gray-100">
                  <p class="text-xs font-bold text-gray-400 uppercase tracking-widest mb-1.5">Claimed Amount</p>
                  <p class="text-3xl font-extrabold text-[#111] font-poppins">₹{{ claim().claimedAmount | number:'1.0-0' }}</p>
                </div>
                <div *ngIf="claim().approvedAmount > 0" class="p-5 rounded-2xl bg-green-50/30 border border-green-100/50">
                  <p class="text-xs font-bold text-green-500/70 uppercase tracking-widest mb-1.5">Approved Amount</p>
                  <p class="text-3xl font-extrabold text-green-600 font-poppins">₹{{ claim().approvedAmount | number:'1.0-0' }}</p>
                </div>
              </div>

              <!-- Description Box -->
              <div class="mb-6">
                <h4 class="text-sm font-bold text-[#111] mb-3">Claim Description</h4>
                <div class="bg-gray-50 p-5 rounded-2xl border border-gray-100/80">
                  <p class="text-gray-700 leading-relaxed whitespace-pre-wrap text-sm" [class.italic]="!claim().description">
                    {{ claim().description || 'No description provided.' }}
                  </p>
                </div>
              </div>

              <!-- Rejection Reason (If Rejected) -->
              <div *ngIf="claim().status === 'Rejected' && claim().rejectionReason" class="mt-6">
                <h4 class="text-sm font-bold text-red-600 mb-3 flex items-center gap-2">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                    <circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line>
                  </svg>
                  Rejection Reason
                </h4>
                <div class="bg-red-50 p-5 rounded-2xl border border-red-100">
                  <p class="text-red-700 leading-relaxed whitespace-pre-wrap text-sm">
                    {{ claim().rejectionReason }}
                  </p>
                </div>
              </div>
            </div>
            
            <!-- BOTTOM - ACTIONS PANEL -->
            <div class="bg-white rounded-2xl shadow-[0_8px_30px_rgba(0,0,0,0.04)] border border-gray-100 p-6 md:p-8">
              <h3 class="text-xl font-bold text-[#111] font-poppins mb-6">Officer Actions</h3>
              
              <!-- Alerts/Banners -->
              <div *ngIf="claim().status === 'PendingDocuments'" class="mb-6 p-4 rounded-xl bg-yellow-50 border border-yellow-200 flex items-start gap-3">
                <div class="text-yellow-600 mt-0.5">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline>
                  </svg>
                </div>
                <div>
                  <h4 class="font-bold text-yellow-800">Pending Documents</h4>
                  <p class="text-sm text-yellow-700 mt-1">⏳ Waiting for customer to upload documents.</p>
                </div>
              </div>

              <div *ngIf="claim().status === 'Approved'" class="mb-6 p-4 rounded-xl bg-green-50 border border-green-200 flex items-start gap-3">
                <div class="text-green-600 mt-0.5">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline>
                  </svg>
                </div>
                <div>
                  <h4 class="font-bold text-green-800">Claim Approved</h4>
                  <p class="text-sm text-green-700 mt-1">✅ Claim approved for ₹{{ claim().approvedAmount | number:'1.0-0' }}</p>
                </div>
              </div>

              <div *ngIf="claim().status === 'PaymentProcessed'" class="mb-6 p-4 rounded-xl bg-purple-50 border border-purple-200 flex items-start gap-3">
                <div class="text-purple-600 mt-0.5">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <rect x="1" y="4" width="22" height="16" rx="2" ry="2"></rect><line x1="1" y1="10" x2="23" y2="10"></line>
                  </svg>
                </div>
                <div>
                  <h4 class="font-bold text-purple-800">Payment Processed</h4>
                  <p class="text-sm text-purple-700 mt-1">💳 Payment has been processed successfully.</p>
                </div>
              </div>

              <div *ngIf="claim().status === 'Rejected' || claim().status === 'Closed'" class="p-4 rounded-xl border flex items-start gap-3"
                   [ngClass]="{'bg-red-50 border-red-200': claim().status === 'Rejected', 'bg-gray-50 border-gray-200': claim().status === 'Closed'}">
                <div class="mt-0.5" [ngClass]="{'text-red-600': claim().status === 'Rejected', 'text-gray-600': claim().status === 'Closed'}">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
                  </svg>
                </div>
                <div>
                  <h4 class="font-bold" [ngClass]="{'text-red-800': claim().status === 'Rejected', 'text-gray-800': claim().status === 'Closed'}">Claim is {{ claim().status }}</h4>
                  <p class="text-sm mt-1" [ngClass]="{'text-red-700': claim().status === 'Rejected', 'text-gray-600': claim().status === 'Closed'}">🔒 This claim is {{ claim().status }}. No further actions available.</p>
                </div>
              </div>

              <!-- Button Groups -->
              <div class="flex flex-wrap gap-4" *ngIf="['UnderReview', 'PendingDocuments'].includes(claim().status)">
                <button (click)="openAction('approve')" 
                        class="px-5 py-2.5 rounded-xl font-bold text-white bg-[#E8584A] hover:bg-[#C94035] transition-all shadow-sm flex items-center gap-2">
                  <span>✅</span> Approve
                </button>
                <button (click)="openAction('reject')" 
                        class="px-5 py-2.5 rounded-xl font-bold text-red-600 bg-white border-2 border-red-100 hover:border-red-600 hover:bg-red-50 transition-all flex items-center gap-2">
                  <span>❌</span> Reject
                </button>
                <button *ngIf="claim().status === 'UnderReview'" (click)="openAction('requestDocs')" 
                        class="px-5 py-2.5 rounded-xl font-bold text-yellow-600 bg-white border-2 border-yellow-100 hover:border-yellow-500 hover:bg-yellow-50 transition-all flex items-center gap-2">
                  <span>📄</span> Request Docs
                </button>
              </div>

              <div *ngIf="claim().status === 'Approved'">
                <button (click)="submitAction('processPayment')" 
                        class="px-6 py-3 rounded-xl font-bold text-white bg-purple-600 hover:bg-purple-700 transition-all shadow-sm flex items-center gap-2">
                  💳 Process Payment
                </button>
              </div>

              <div *ngIf="claim().status === 'PaymentProcessed'">
                <button (click)="submitAction('close')" 
                        class="px-6 py-3 rounded-xl font-bold text-white bg-gray-800 hover:bg-gray-900 transition-all shadow-sm flex items-center gap-2">
                  🔒 Close Claim
                </button>
              </div>
              
              <div *ngIf="['Rejected', 'Closed'].includes(claim().status)" class="mt-4">
                 <button routerLink="/officer/claims" 
                        class="px-5 py-2.5 rounded-xl font-bold text-gray-700 bg-gray-100 hover:bg-gray-200 transition-all">
                  ← Back to All Claims
                </button>
              </div>

              <!-- INLINE FORMS (Only one visible at a time) -->
              
              <!-- Approve Form -->
              <div *ngIf="activeAction() === 'approve'" class="mt-6 p-6 rounded-2xl bg-gray-50 border border-[#E8584A]/20 animate-fade-in relative">
                <button (click)="openAction('')" class="absolute top-4 right-4 text-gray-400 hover:text-gray-600">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line>
                  </svg>
                </button>
                <h4 class="text-sm font-bold text-[#111] mb-4">Confirm Approval Amount</h4>
                <div class="space-y-4">
                  <div>
                    <label class="block text-xs font-bold text-gray-500 uppercase tracking-wider mb-2">Approved Amount (₹)</label>
                    <div class="relative">
                      <span class="absolute left-4 top-1/2 -translate-y-1/2 font-bold text-gray-500">₹</span>
                      <input type="number" [(ngModel)]="approvedAmountForm" placeholder="0"
                             class="w-full pl-8 pr-4 py-3 bg-white border border-gray-200 rounded-xl font-bold text-[#111] focus:ring-2 focus:ring-[#E8584A]/20 focus:border-[#E8584A] outline-none transition-all">
                    </div>
                  </div>
                  <button (click)="submitAction('approve')" [disabled]="!approvedAmountForm || approvedAmountForm <= 0 || isProcessing()"
                          class="w-full py-3 rounded-xl font-bold text-white bg-[#E8584A] hover:bg-[#C94035] transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex justify-center items-center gap-2">
                    <div *ngIf="isProcessing()" class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                    Confirm Approval
                  </button>
                </div>
              </div>

              <!-- Reject Form -->
              <div *ngIf="activeAction() === 'reject'" class="mt-6 p-6 rounded-2xl bg-red-50/50 border border-red-100 animate-fade-in relative">
                 <button (click)="openAction('')" class="absolute top-4 right-4 text-gray-400 hover:text-gray-600">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line>
                  </svg>
                </button>
                <h4 class="text-sm font-bold text-red-700 mb-4">Provide Rejection Reason</h4>
                <div class="space-y-4">
                  <div>
                    <label class="block text-xs font-bold text-red-600 uppercase tracking-wider mb-2">Reason</label>
                    <textarea [(ngModel)]="rejectionReasonForm" rows="3" placeholder="Explain why this claim is being rejected..."
                              class="w-full p-4 bg-white border border-red-200 rounded-xl font-medium text-[#111] focus:ring-2 focus:ring-red-500/20 focus:border-red-500 outline-none transition-all resize-none"></textarea>
                  </div>
                  <button (click)="submitAction('reject')" [disabled]="rejectionReasonForm.length < 10 || isProcessing()"
                          class="w-full py-3 rounded-xl font-bold text-white bg-red-600 hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex justify-center items-center gap-2">
                    <div *ngIf="isProcessing()" class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                    Confirm Rejection
                  </button>
                </div>
              </div>

              <!-- Request Docs Form -->
              <div *ngIf="activeAction() === 'requestDocs'" class="mt-6 p-6 rounded-2xl bg-yellow-50/50 border border-yellow-100 animate-fade-in relative">
                 <button (click)="openAction('')" class="absolute top-4 right-4 text-gray-400 hover:text-gray-600">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line>
                  </svg>
                </button>
                <h4 class="text-sm font-bold text-yellow-800 mb-2">Request Documents</h4>
                <p class="text-sm text-yellow-700 mb-6">The customer will be notified to upload supporting documents for this claim.</p>
                <button (click)="submitAction('requestDocs')" [disabled]="isProcessing()"
                        class="w-full py-3 rounded-xl font-bold text-yellow-700 bg-yellow-400 hover:bg-yellow-500 transition-colors disabled:opacity-50 disabled:cursor-not-allowed shadow-sm flex justify-center items-center gap-2">
                  <div *ngIf="isProcessing()" class="w-4 h-4 border-2 border-yellow-700/30 border-t-yellow-700 rounded-full animate-spin"></div>
                  Send Request
                </button>
              </div>

            </div>
          </div>

          <!-- RIGHT COLUMN - Customer & Policy -->
          <div class="space-y-6">
            
            <!-- Customer Info -->
            <div class="bg-white rounded-2xl shadow-[0_8px_30px_rgba(0,0,0,0.04)] border border-gray-100 p-6">
              <h3 class="flex items-center gap-2 text-sm text-gray-400 font-bold uppercase tracking-wider mb-6">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle>
                </svg>
                Customer Information
              </h3>
              
              <div class="space-y-5">
                <div>
                  <p class="text-xs text-gray-400 font-bold mb-1">Customer Name</p>
                  <p class="text-base font-bold text-[#111]">{{ claim().customerName }}</p>
                </div>
                <div>
                  <p class="text-xs text-gray-400 font-bold mb-1">Policy Number</p>
                  <p class="text-base font-bold text-gray-600 font-mono bg-gray-50 px-3 py-1.5 rounded-lg inline-block border border-gray-100">{{ claim().policyNumber }}</p>
                </div>
              </div>
            </div>

            <!-- Documents -->
            <div class="bg-white rounded-2xl shadow-[0_8px_30px_rgba(0,0,0,0.04)] border border-gray-100 p-6">
              <h3 class="flex items-center gap-2 text-sm text-gray-400 font-bold uppercase tracking-wider mb-6">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M21.44 11.05l-9.19 9.19a6 6 0 0 1-8.49-8.49l9.19-9.19a4 4 0 0 1 5.66 5.66l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48"></path>
                </svg>
                Submitted Documents
              </h3>
              
              <div *ngIf="claim().documents && claim().documents.length > 0; else noDocs" class="space-y-3">
                <div *ngFor="let doc of claim().documents" 
                     class="flex items-center justify-between p-3 rounded-xl border border-gray-100 hover:border-blue-100 hover:bg-blue-50/30 transition-colors group">
                  <div class="flex items-center gap-3 overflow-hidden">
                    <div class="w-10 h-10 rounded-lg bg-gray-50 text-gray-400 flex flex-shrink-0 items-center justify-center">
                      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                        <polyline points="14 2 14 8 20 8"></polyline>
                        <line x1="16" y1="13" x2="8" y2="13"></line><line x1="16" y1="17" x2="8" y2="17"></line><polyline points="10 9 9 9 8 9"></polyline>
                      </svg>
                    </div>
                    <div class="min-w-0">
                      <p class="text-sm font-bold text-[#111] truncate">{{ doc.fileName }}</p>
                      <p class="text-xs text-blue-500 font-bold uppercase tracking-wider mt-0.5">{{ doc.fileType || 'Document' }}</p>
                    </div>
                  </div>
                  <button (click)="download(doc.fileUrl, doc.fileName)"
                     class="w-8 h-8 rounded-lg bg-white border border-gray-200 flex items-center justify-center text-gray-400 hover:text-blue-500 hover:border-blue-200 transition-colors flex-shrink-0">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                      <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line>
                    </svg>
                  </button>
                </div>
              </div>

              <ng-template #noDocs>
                <div class="py-8 flex flex-col items-center justify-center text-center bg-gray-50/50 rounded-xl border border-dashed border-gray-200">
                  <div class="text-gray-300 mb-2">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                      <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                      <polyline points="14 2 14 8 20 8"></polyline>
                      <line x1="9" y1="15" x2="15" y2="15"></line>
                    </svg>
                  </div>
                  <p class="text-sm font-medium text-gray-500">No documents submitted<br>by customer yet.</p>
                </div>
              </ng-template>

            </div>

          </div>
          
        </div>

      </ng-container>
      
      <!-- Fallback when claim not found -->
      <div *ngIf="!isLoading() && !claim()" class="py-20 flex flex-col items-center justify-center text-center bg-white rounded-3xl border border-gray-100 shadow-sm">
        <h3 class="text-2xl font-bold text-[#111] mb-2 font-poppins">Claim Not Found</h3>
        <p class="text-gray-500 mb-6">The claim you're looking for doesn't exist or you don't have permission to view it.</p>
        <button routerLink="/officer/claims" class="px-6 py-3 rounded-xl font-bold text-white bg-[#E8584A] hover:bg-[#C94035] transition-colors">
          Return to Claims
        </button>
      </div>

    </div>
  `
})
export class OfficerClaimDetailComponent implements OnInit {
  private claimsService = inject(ClaimsOfficerService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // State
  claim = signal<any>(null);
  isLoading = signal<boolean>(true);
  isProcessing = signal<boolean>(false);
  activeAction = signal<string>(''); // 'approve' | 'reject' | 'requestDocs' | ''

  // Form states
  approvedAmountForm: number = 0;
  rejectionReasonForm: string = '';

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.fetchClaim(parseInt(id, 10));
      }
    });
  }

  fetchClaim(id: number) {
    this.isLoading.set(true);
    this.claimsService.getAssignedClaims().subscribe({
      next: (claims: any[]) => {
        const found = claims.find((c: any) => (c.id === id || c.claimId === id));
        if (found) {
          this.claim.set(found);
          this.approvedAmountForm = found.claimedAmount || 0;
        } else {
          this.claim.set(null);
        }
        this.isLoading.set(false);
      },
      error: (err: any) => {
        console.error('Error fetching claim', err);
        this.isLoading.set(false);
      }
    });
  }

  openAction(actionName: string) {
    this.activeAction.set(actionName);
  }

  submitAction(actionType: string) {
    if (!this.claim()) return;

    const id = this.claim().id || this.claim().claimId;
    this.isProcessing.set(true);

    let submitObs;

    switch (actionType) {
      case 'approve':
        submitObs = this.claimsService.reviewClaim(id, {
          isApproved: true,
          approvedAmount: this.approvedAmountForm
        });
        break;
      case 'reject':
        submitObs = this.claimsService.reviewClaim(id, {
          isApproved: false,
          rejectionReason: this.rejectionReasonForm
        });
        break;
      case 'requestDocs':
        submitObs = this.claimsService.requestDocuments(id);
        break;
      case 'processPayment':
        submitObs = this.claimsService.processPayment(id);
        break;
      case 'close':
        submitObs = this.claimsService.closeClaim(id);
        break;
    }

    if (submitObs) {
      submitObs.subscribe({
        next: () => {
          // Success! Reset form and reload
          alert(`Success! Claim status updated.`); // Simulated toast
          this.activeAction.set('');
          this.isProcessing.set(false);
          this.fetchClaim(id); // Reload
        },
        error: (err: any) => {
          console.error(err);
          alert('Failed to process claim action. Check console for details.');
          this.isProcessing.set(false);
        }
      });
    } else {
      this.isProcessing.set(false);
    }
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

  download(url: string, fileName: string) {
    if (!url) return;
    this.claimsService.downloadDocument(url).subscribe({
      next: (blob: Blob) => {
        const downloadUrl = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = downloadUrl;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(downloadUrl);
        document.body.removeChild(a);
      },
      error: (err) => {
        console.error('Download failed', err);
        alert('Failed to download document.');
      }
    });
  }
}
