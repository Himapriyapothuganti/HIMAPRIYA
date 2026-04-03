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
    <div class="space-y-6 animate-fade-in pb-12 overflow-x-hidden" style="font-family: 'Poppins', sans-serif;">
      
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
              <span *ngIf="claim()" class="px-3 py-1 rounded-full text-[11px] font-black uppercase tracking-wider whitespace-nowrap inline-block" [ngClass]="getStatusBadgeClass(claim().status)">
                {{ formatStatus(claim().status) }}
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
        
        <!-- SINGLE COLUMN CONTEXTUAL LAYOUT (4XL) -->
        <div class="max-w-4xl mx-auto space-y-8 pb-20">
          
          <!-- 1. CLAIM & AMOUNTS (CORE INFO) -->
          <div class="bg-white rounded-[2.5rem] shadow-[0_20px_50px_rgba(0,0,0,0.04)] border border-gray-100 p-8 md:p-10 relative overflow-hidden">
            <div class="absolute top-0 right-0 w-64 h-64 bg-gray-50 rounded-full -mr-32 -mt-32 opacity-40"></div>
            
            <div class="flex flex-wrap items-center justify-between gap-6 mb-10 relative z-10">
              <div>
                <p class="text-[10px] font-black text-gray-400 uppercase tracking-[4px] mb-2">Primary Claim Context</p>
                <h3 class="text-2xl font-black text-[#111] font-poppins">{{ claim().claimType || 'General Insurance' }} Claim</h3>
                <p class="text-gray-400 text-xs font-bold mt-1">Submitted on {{ claim().submittedAt | date:'dd MMM yyyy' }}</p>
              </div>
              <div class="px-6 py-4 bg-[#FDF4F0] border border-[#E8584A]/10 rounded-3xl">
                <p class="text-[10px] font-black text-[#E8584A] uppercase tracking-[3px] mb-1">Total Claimed</p>
                <p class="text-3xl font-black text-[#111]">₹{{ claim().claimedAmount | number:'1.0-0' }}</p>
              </div>
            </div>

            <!-- Description -->
            <div class="relative z-10">
              <h4 class="text-xs font-black text-gray-500 uppercase tracking-widest mb-4 flex items-center gap-2">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path></svg>
                Incident Statement
              </h4>
              <div class="bg-gray-50/80 p-6 rounded-3xl border border-gray-100 text-gray-700 leading-relaxed font-medium italic">
                "{{ claim().description || 'No detailed statement provided by the claimant.' }}"
              </div>
            </div>
          </div>

          <!-- 2. PARTICIPANT DATA (Moved from Sidebar) -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="bg-white rounded-[2rem] border border-gray-100 p-8 shadow-sm group hover:border-[#E8584A]/20 transition-all">
              <div class="flex items-center gap-4 mb-6">
                <div class="w-12 h-12 rounded-2xl bg-gray-50 flex items-center justify-center text-gray-400 group-hover:bg-[#FDF4F0] group-hover:text-[#E8584A] transition-colors">
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle>
                  </svg>
                </div>
                <div>
                  <p class="text-[10px] font-black text-gray-400 uppercase tracking-widest">Claimant</p>
                  <p class="text-lg font-black text-[#111]">{{ claim().customerName }}</p>
                </div>
              </div>
              <div class="space-y-4">
                <div class="flex justify-between items-center py-3 border-b border-gray-50">
                  <span class="text-xs font-bold text-gray-400 uppercase">Policy No.</span>
                  <span class="text-sm font-black text-gray-600 font-mono tracking-tighter bg-gray-50 px-2 py-1 rounded-md">{{ claim().policyNumber }}</span>
                </div>
              </div>
            </div>

            <!-- Documents (Moved from Sidebar) -->
            <div class="bg-white rounded-[2rem] border border-gray-100 p-8 shadow-sm flex flex-col justify-between">
              <div>
                <h4 class="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-6 flex items-center gap-2">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M21.44 11.05l-9.19 9.19a6 6 0 0 1-8.49-8.49l9.19-9.19a4 4 0 0 1 5.66 5.66l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48"></path></svg>
                  Evidence Vault ({{ claim().documents?.length || 0 }})
                </h4>
                <div *ngIf="claim().documents && claim().documents.length > 0; else noDocs" class="space-y-3">
                  <div *ngFor="let doc of claim().documents" 
                       class="flex items-center justify-between p-3 rounded-2xl bg-gray-50/50 border border-gray-100">
                    <span class="text-xs font-black text-gray-700 truncate max-w-[150px]">{{ doc.fileName }}</span>
                    <button (click)="viewDocument(doc.fileUrl, doc.fileName)" class="p-2 rounded-xl bg-white border border-gray-200 text-gray-400 hover:text-[#E8584A] hover:border-[#E8584A]/30 transition-all">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                    </button>
                  </div>
                </div>
                <ng-template #noDocs>
                  <p class="text-xs font-bold text-gray-400 italic">No files attached.</p>
                </ng-template>
              </div>
            </div>
          </div>

              <!-- ✨ ENHANCED AI ANALYST REPORT CARD (10X VERSION) -->
              <div class="mb-10 animate-fade-in">
                
                <!-- TOP HEADER: TITLE & RISK & BUTTON -->
                <div class="flex flex-col md:flex-row md:items-center justify-between gap-6 mb-8 px-2">
                  <div class="flex-1">
                    <h4 class="text-[11px] font-black text-indigo-500 uppercase tracking-[4px] mb-2 flex items-center gap-2">
                       <span class="p-1.5 bg-indigo-50 rounded-lg"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#4F46E5" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"></path></svg></span>
                       Expert AI Review
                    </h4>
                    <h2 class="text-2xl font-extrabold text-[#111] font-poppins tracking-tight">Forensic Audit Intelligence</h2>
                  </div>
                  
                  <div class="flex items-center gap-4">
                    <!-- Analyze Button Integrated Here -->
                    <button (click)="runAnalysis()" [disabled]="isProcessing()"
                            class="px-8 py-3.5 rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-xl flex items-center gap-3"
                            [ngClass]="isProcessing() ? 'bg-indigo-50 text-indigo-400 cursor-not-allowed border-2 border-indigo-100' : 'bg-slate-900 text-white hover:bg-[#111] hover:-translate-y-1 active:translate-y-0 border-2 border-slate-900'">
                      <div *ngIf="isProcessing()" class="w-4 h-4 border-2 border-indigo-400/30 border-t-indigo-500 rounded-full animate-spin"></div>
                      <span *ngIf="!isProcessing()"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M12 2L2 7l10 5 10-5-10-5z"></path></svg></span>
                      {{ isProcessing() ? 'Analysing Claims...' : 'Start Audit Analysis' }}
                    </button>

                    <!-- DYNAMIC RISK BADGE -->
                    <div *ngIf="aiReport()" 
                         class="px-5 py-3 rounded-2xl border-2 transition-all shadow-lg flex items-center gap-3"
                         [ngClass]="{
                           'bg-red-50 border-red-100 text-red-700': aiReport().riskScore === 'High',
                           'bg-amber-50 border-amber-100 text-amber-700': aiReport().riskScore === 'Medium',
                           'bg-emerald-50 border-emerald-100 text-emerald-700': aiReport().riskScore === 'Low'
                         }">
                      <span class="w-2.5 h-2.5 rounded-full" [ngClass]="{'bg-red-500 animate-pulse': aiReport().riskScore === 'High', 'bg-amber-500 animate-pulse': aiReport().riskScore === 'Medium', 'bg-emerald-500 animate-pulse': aiReport().riskScore === 'Low'}"></span>
                      <span class="text-xs font-black uppercase tracking-widest">{{ aiReport().riskScore }} RISK</span>
                    </div>
                  </div>
                </div>

                <!-- MAIN REPORT CONTAINER (Already moved or remaining here) -->
                <!-- Note: I will only replace the top header and bottom cleanup in this chunk to avoid over-complicating -->
                <div *ngIf="claim().aiSummary">
                   <div *ngIf="aiReport(); else legacySummary" class="space-y-6">
                      <!-- AI OPINION BOX -->
                      <div class="relative overflow-hidden rounded-[2.5rem] bg-slate-950 p-10 text-white shadow-2xl border-4 border-indigo-500/10">
                        <div class="relative z-10">
                          <span class="text-[10px] font-black uppercase tracking-[4px] text-indigo-400 mb-4 block">Senior AI Specialist Findings</span>
                          <h3 class="text-2xl font-bold mb-4 leading-snug tracking-tight">{{ aiReport().aiOpinion }}</h3>
                          <p class="text-slate-400 text-[15px] font-medium leading-relaxed">{{ aiReport().riskReasoning }}</p>
                        </div>
                      </div>

                      <!-- RED FLAGS -->
                      <div *ngIf="!aiReport().documentComparison?.isConsistent" class="bg-red-50 border-2 border-red-100 p-8 rounded-[2rem] flex items-start gap-6 shadow-sm">
                        <div class="w-12 h-12 rounded-2xl bg-red-100 text-red-600 flex items-center justify-center shrink-0">
                          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path></svg>
                        </div>
                        <div>
                          <h4 class="text-red-900 font-extrabold text-[11px] uppercase tracking-widest mb-1">Audit Discrepancy Found</h4>
                          <p class="text-red-800 text-sm font-bold leading-relaxed">{{ aiReport().documentComparison.mismatchDetails }}</p>
                        </div>
                      </div>

                      <!-- ANALYTICS GRID -->
                      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div *ngFor="let cat of aiReport().categories" class="p-8 bg-white border border-gray-100 rounded-[2rem] shadow-sm hover:border-indigo-100 transition-all group">
                          <p class="text-[10px] font-black text-indigo-400 uppercase tracking-widest mb-3 pr-4 border-b border-indigo-50 pb-2 inline-block">{{ cat.title }}</p>
                          <p class="text-[14px] text-gray-700 font-bold leading-relaxed">{{ cat.summary }}</p>
                        </div>
                      </div>

                      <!-- FINANCIAL CHART -->
                      <div class="bg-indigo-50/30 border-2 border-indigo-100/50 p-10 rounded-[3rem]">
                        <h5 class="text-[11px] font-black text-indigo-900 uppercase tracking-[4px] mb-10 text-center">Document vs Data Reconciliation</h5>
                        <div class="space-y-12 max-w-2xl mx-auto">
                          <div *ngFor="let metric of aiReport().chartData; let i = index">
                            <div class="flex items-center justify-between mb-4 px-2">
                              <span class="text-[14px] font-black text-slate-700 uppercase tracking-wide">{{ metric.label }}</span>
                              <span class="text-lg font-black text-slate-950">₹{{ metric.value | number }}</span>
                            </div>
                            <div class="h-5 bg-white rounded-full border border-indigo-100 shadow-inner p-1">
                              <div class="h-full rounded-full transition-all duration-1000 bg-gradient-to-r shadow-md"
                                   [ngStyle]="{'width': (metric.value / (aiReport().chartData[0].value > aiReport().chartData[1].value ? aiReport().chartData[0].value : aiReport().chartData[1].value) * 100) + '%'}"
                                   [ngClass]="i === 0 ? 'from-blue-500 to-indigo-600' : 'from-emerald-400 to-teal-500'">
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>

                      <!-- CHECKLISTS -->
                      <div class="grid grid-cols-1 md:grid-cols-2 gap-8 pt-4">
                        <div class="bg-white p-8 rounded-[2rem] border-2 border-rose-50 shadow-sm">
                          <h5 class="text-[11px] font-black text-rose-500 uppercase tracking-[3px] mb-6 flex items-center gap-2">⚠️ Audit Observations</h5>
                          <div class="space-y-4">
                            <div *ngFor="let issue of aiReport()?.keyIssues" class="text-[13px] font-bold text-gray-700 bg-rose-50/30 p-4 rounded-2xl flex items-center gap-3">
                               <span class="w-2 h-2 rounded-full bg-rose-400 shrink-0"></span> {{ issue }}
                            </div>
                          </div>
                        </div>
                        <div class="bg-white p-8 rounded-[2rem] border-2 border-emerald-50 shadow-sm">
                          <h5 class="text-[11px] font-black text-emerald-500 uppercase tracking-[3px] mb-6 flex items-center gap-2">🚀 Next Step Protocol</h5>
                          <div class="space-y-4">
                            <div *ngFor="let step of aiReport()?.checkList" class="text-[13px] font-bold text-gray-700 bg-emerald-50/30 p-4 rounded-2xl flex items-center gap-3">
                               <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="4" stroke-linecap="round" stroke-linejoin="round" class="text-emerald-500 shrink-0"><polyline points="20 6 9 17 4 12"></polyline></svg> {{ step }}
                            </div>
                          </div>
                        </div>
                      </div>
                   </div>

                   <ng-template #legacySummary>
                     <div class="bg-white p-10 rounded-[2.5rem] border-4 border-indigo-50 shadow-xl italic text-slate-700 leading-relaxed text-[15px] font-medium" [innerHTML]="formatMarkdown(claim().aiSummary)"></div>
                   </ng-template>
                </div>
              </div>

              <!-- 4. FINAL ACTION LAYER (Replacing old sidebar/actions logic) -->
              <div class="bg-white rounded-[3rem] shadow-2xl border-8 border-gray-50 p-10 md:p-14 mt-12">
                <div class="flex items-center gap-6 mb-12">
                  <div class="w-16 h-16 rounded-[2rem] bg-[#FDF4F0] text-[#E8584A] flex items-center justify-center shadow-inner">
                    <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>
                  </div>
                  <div>
                    <h3 class="text-2xl font-black text-[#111] font-poppins">Official Determination</h3>
                    <p class="text-gray-400 text-[10px] font-black uppercase tracking-[4px] mt-2 flex items-center gap-2">
                       Current Status: <span class="text-slate-900 border-b-2 border-slate-200 pb-0.5">{{ formatStatus(claim().status) }}</span>
                    </p>
                  </div>
                </div>

                <!-- Action Buttons flow better here -->
                <div class="flex flex-wrap gap-6" *ngIf="['UnderReview', 'PendingDocuments'].includes(claim().status)">
                  <button (click)="openAction('approve')" 
                          class="px-12 py-5 rounded-2xl font-black text-sm text-white bg-[#E8584A] hover:bg-[#C94035] transition-all shadow-2xl shadow-[#E8584A]/30 flex items-center gap-4 hover:-translate-y-1">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="4" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>
                    Authorize Payout
                  </button>
                  <button (click)="openAction('reject')" 
                          class="px-10 py-5 rounded-2xl font-black text-sm text-rose-600 bg-white border-2 border-rose-100 hover:border-rose-600 transition-all flex items-center gap-4">
                    Decline Claim
                  </button>
                </div>

                <!-- Fallback Actions -->
                <div *ngIf="!['UnderReview', 'PendingDocuments'].includes(claim().status)" class="space-y-6">
                   <div class="p-8 rounded-[2rem] bg-gray-50 border-2 border-gray-100 flex items-center gap-6">
                      <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" class="text-gray-400"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
                      <p class="font-black text-gray-500 uppercase text-xs tracking-widest leading-relaxed">System Lock: This claim has been finalized as <span class="text-slate-900">{{ claim().status }}</span>. No further audit modifications allowed.</p>
                   </div>
                   <button routerLink="/officer/claims" class="text-xs font-black uppercase tracking-widest text-gray-400 hover:text-[#E8584A] transition-colors flex items-center gap-2">← Return to Claim Registry</button>
                </div>

                <!-- Forms remain integrated below -->
                 <div *ngIf="activeAction() === 'approve'" class="mt-12 space-y-8 p-10 bg-[#fdfaf8] rounded-[3rem] border-2 border-[#E8584A]/10 animate-fade-in relative">
                    <button (click)="openAction('')" class="absolute top-6 right-8 text-gray-300 hover:text-gray-600"><svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="4" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg></button>
                    <div>
                      <p class="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 text-center">Calculated Settlement Limit</p>
                      <p class="text-4xl font-black text-slate-950 text-center">₹{{ claim().suggestedPayout | number }}</p>
                    </div>
                    <div class="h-px bg-[#E8584A]/10"></div>
                    <div>
                      <label class="block text-xs font-black text-slate-500 uppercase tracking-widest mb-4">Payout Amount Authorization</label>
                      <input type="number" [(ngModel)]="approvedAmountForm" class="w-full p-6 bg-white border-2 border-slate-100 rounded-[2rem] font-black text-2xl focus:border-[#E8584A] outline-none transition-all shadow-sm">
                    </div>
                    <button (click)="submitAction('approve')" class="w-full py-6 rounded-[2rem] bg-slate-950 text-white font-black uppercase tracking-widest shadow-2xl hover:bg-black transition-all">Submit Final Approval</button>
                 </div>

                 <div *ngIf="activeAction() === 'reject'" class="mt-12 space-y-8 p-10 bg-rose-50/50 rounded-[3rem] border-2 border-rose-100 animate-fade-in relative">
                    <button (click)="openAction('')" class="absolute top-6 right-8 text-gray-300 hover:text-rose-600"><svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="4" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg></button>
                    <div>
                      <label class="block text-xs font-black text-rose-600 uppercase tracking-widest mb-4">Official Reason for Denial</label>
                      <textarea [(ngModel)]="rejectionReasonForm" rows="5" class="w-full p-8 bg-white border-2 border-transparent focus:border-rose-200 rounded-[2.5rem] font-bold text-gray-700 outline-none transition-all resize-none shadow-sm"></textarea>
                    </div>
                    <button (click)="submitAction('reject')" class="w-full py-6 rounded-[2rem] bg-rose-600 text-white font-black uppercase tracking-widest shadow-2xl hover:bg-rose-700 transition-all">Formally Decline Claim</button>
                 </div>
              </div>
            </div> <!-- Close max-w-4xl -->
          
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

  // Computed AI Report
  aiReport = computed(() => {
    let summary = this.claim()?.aiSummary;
    if (!summary) return null;

    // Resilient Parsing: Strip markdown code blocks if the AI includes them accidentally
    if (summary.includes('```json')) {
      summary = summary.replace(/```json/g, '').replace(/```/g, '');
    } else if (summary.includes('```')) {
      summary = summary.replace(/```/g, '');
    }

    try {
      summary = summary.trim();
      // Ensure it starts with { and ends with }
      const start = summary.indexOf('{');
      const end = summary.lastIndexOf('}');
      if (start !== -1 && end !== -1) {
        return JSON.parse(summary.substring(start, end + 1));
      }
    } catch (e) {
      console.warn('AI Summary is not valid JSON, falling back to text.');
    }
    return null;
  });

  // Form states
  approvedAmountForm: number = 0;
  rejectionReasonForm: string = '';

  formatMarkdown(text: string): string {
    if (!text) return '';
    
    // Escape raw HTML brackets so AI-generated placeholders like <CustomerName> don't get swallowed by the DOM
    let safeText = text.replace(/</g, '&lt;').replace(/>/g, '&gt;');

    return safeText
      .replace(/\*\*(.*?)\*\*/g, '<strong class="text-indigo-950 font-extrabold bg-indigo-50/50 px-1 rounded-md">$1</strong>')
      .replace(/^### (.*$)/gim, '<h3 class="text-[15px] font-bold text-indigo-900 mt-5 mb-2 border-b border-indigo-100/60 pb-1 flex items-center gap-2"><span class="w-1.5 h-1.5 rounded-full bg-indigo-400"></span>$1</h3>')
      .replace(/^## (.*$)/gim, '<h2 class="text-[17px] font-black text-indigo-950 mt-6 mb-3 tracking-tight">$1</h2>')
      .replace(/^\* (.*$)/gim, '<li class="ml-5 list-disc pl-1 mb-1.5 text-blue-800 marker:text-indigo-400">$1</li>')
      .replace(/^(\d+)\. (.*$)/gim, '<li class="ml-5 list-decimal pl-1 mb-1.5 font-bold text-indigo-900 marker:text-indigo-500" value="$1"><span class="font-medium text-blue-800">$2</span></li>')
      .replace(/\n\n/g, '<div class="h-4"></div>')
      .replace(/\n/g, '<br>');
  }

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
    if (actionName === 'approve' && this.claim()) {
      this.approvedAmountForm = this.claim().suggestedPayout > 0 ? this.claim().suggestedPayout : this.claim().claimedAmount;
    } else {
      this.approvedAmountForm = 0;
      this.rejectionReasonForm = '';
    }
  }

  submitAction(actionType: string) {
    if (!this.claim()) return;

    const id = this.claim().id || this.claim().claimId;
    this.isProcessing.set(true);

    let submitObs;

    switch (actionType) {
      case 'approve':
        if (this.approvedAmountForm > this.claim().suggestedPayout) {
          alert(`Cannot approve more than the system-calculated limit of ₹${this.claim().suggestedPayout}`);
          this.isProcessing.set(false);
          return;
        }
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

  runAnalysis() {
    if (!this.claim()) return;
    const id = this.claim().id || this.claim().claimId;
    this.isProcessing.set(true);
    this.claimsService.analyzeClaim(id).subscribe({
      next: (updatedClaim: any) => {
        this.claim.set(updatedClaim);
        this.isProcessing.set(false);
      },
      error: (err: any) => {
        console.error(err);
        alert('AI Analysis failed. See console for details.');
        this.isProcessing.set(false);
      }
    });
  }


  viewDocument(url: string, fileName: string) {
    if (!url) return;
    this.claimsService.downloadDocument(url).subscribe({
      next: (blob: Blob) => {
        const fileUrl = window.URL.createObjectURL(blob);
        window.open(fileUrl, '_blank');
        
        // Clean up memory after a short delay since we opened it in a new tab
        setTimeout(() => window.URL.revokeObjectURL(fileUrl), 60000);
      },
      error: (err) => {
        console.error('Failed to load document', err);
        alert('Failed to load document for viewing.');
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
    // Convert CamelCase to Space Case (e.g., UnderReview -> Under Review)
    return status.replace(/([A-Z])/g, ' $1').trim();
  }
}
