import { Component, EventEmitter, Output, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomerService } from '../../../Services/customer.service';

@Component({
  selector: 'app-recommendation-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="fixed inset-0 z-[60] flex items-center justify-center p-4 bg-black/70 backdrop-blur-md">
      <div class="bg-white rounded-[32px] w-full max-w-lg shadow-2xl overflow-hidden animate-in fade-in zoom-in duration-300 border border-white/20">
        
        <!-- Header -->
        <div class="px-8 py-7 bg-gradient-to-br from-[#E8584A] to-[#D4483A] text-white flex justify-between items-center relative overflow-hidden">
          <div class="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -translate-y-1/2 translate-x-1/2 blur-2xl"></div>
          <div class="relative z-10">
            <h2 class="text-2xl font-extrabold font-poppins tracking-tight">AI Plan Finder ⭐</h2>
            <p class="text-white/90 text-[11px] font-medium tracking-wide uppercase">Real-time intelligence by Groq</p>
          </div>
          <button (click)="close()" class="relative z-10 p-2 hover:bg-white/20 rounded-full transition-all active:scale-90">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2.5"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" /></svg>
          </button>
        </div>

        <div class="p-8">
          <ng-container *ngIf="!recommendation && !loading; else processingOrResult">
            <!-- Step 1: Trip Details -->
            <div class="space-y-6">
              <div>
                <label class="block text-[12px] font-black text-slate-400 uppercase tracking-widest mb-2 px-1">Where are you heading?</label>
                <div class="relative">
                  <input [(ngModel)]="request.Destination" placeholder="e.g. USA, Thailand, France" 
                    class="w-full px-5 py-4 bg-slate-50 border border-slate-200 rounded-2xl focus:ring-4 focus:ring-[#E8584A]/10 focus:border-[#E8584A] outline-none transition-all text-[15px] font-semibold text-slate-800 placeholder:text-slate-300">
                  <span class="absolute right-5 top-1/2 -translate-y-1/2 text-xl">📍</span>
                </div>
              </div>

              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-[12px] font-black text-slate-400 uppercase tracking-widest mb-2 px-1">Duration (Days)</label>
                  <input type="number" [(ngModel)]="request.DurationDays" 
                    class="w-full px-5 py-4 bg-slate-50 border border-slate-200 rounded-2xl focus:ring-4 focus:ring-[#E8584A]/10 focus:border-[#E8584A] outline-none transition-all text-[15px] font-semibold text-slate-800">
                </div>
                <div>
                  <label class="block text-[12px] font-black text-slate-400 uppercase tracking-widest mb-2 px-1">Traveller Age</label>
                  <input type="number" [(ngModel)]="request.Age" 
                    class="w-full px-5 py-4 bg-slate-50 border border-slate-200 rounded-2xl focus:ring-4 focus:ring-[#E8584A]/10 focus:border-[#E8584A] outline-none transition-all text-[15px] font-semibold text-slate-800">
                </div>
              </div>

              <div>
                <label class="block text-[12px] font-black text-slate-400 uppercase tracking-widest mb-2 px-1">Travel Purpose</label>
                <div class="grid grid-cols-3 gap-2">
                  <button *ngFor="let p of purposes" (click)="request.Purpose = p"
                    [class]="request.Purpose === p ? 'bg-[#E8584A] text-white shadow-lg shadow-red-500/20' : 'bg-slate-50 text-slate-500 hover:bg-slate-100 border-slate-200 border'"
                    class="py-3 rounded-2xl text-[13px] font-bold transition-all active:scale-95">
                    {{p}}
                  </button>
                </div>
              </div>

              <div>
                <button (click)="getRecommendation()" [disabled]="!request.Destination"
                  class="w-full py-5 bg-black text-white rounded-[24px] font-black text-[16px] shadow-xl hover:bg-slate-800 active:scale-[0.98] transition-all flex justify-center items-center gap-3 mt-4 disabled:opacity-30 disabled:grayscale">
                  Ask AI Consultant
                  <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10.293 3.293a1 1 0 011.414 0l6 6a1 1 0 010 1.414l-6 6a1 1 0 01-1.414-1.414L14.586 11H3a1 1 0 110-2h11.586l-4.293-4.293a1 1 0 010-1.414z" clip-rule="evenodd" /></svg>
                </button>
              </div>
            </div>
          </ng-container>

          <ng-template #processingOrResult>
            <div *ngIf="loading" class="text-center py-10 animate-pulse">
              <div class="relative w-24 h-24 mx-auto mb-6">
                <div class="absolute inset-0 border-4 border-[#E8584A]/20 rounded-full"></div>
                <div class="absolute inset-0 border-4 border-[#E8584A] rounded-full border-t-transparent animate-spin"></div>
                <div class="absolute inset-0 flex items-center justify-center text-3xl">🤖</div>
              </div>
              <h3 class="text-xl font-black text-slate-800 mb-2">Analyzing your trip...</h3>
              <p class="text-slate-400 text-sm font-medium tracking-wide italic">"Consulting Groq Llama-3 for best coverage"</p>
            </div>

            <div *ngIf="!loading && recommendation" class="text-center space-y-6 animate-in slide-in-from-bottom-8 duration-700">
              <div class="inline-flex items-center justify-center w-20 h-20 bg-emerald-50 rounded-3xl mb-2 rotate-3 shadow-sm border border-emerald-100">
                <span class="text-4xl">💎</span>
              </div>
              
              <div>
                <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-1">Recommended for you</h3>
                <div class="p-6 bg-slate-900 rounded-[32px] relative shadow-2xl group">
                  <div class="absolute -top-3 left-1/2 -translate-x-1/2 px-5 py-1.5 bg-[#E8584A] text-white text-[11px] font-black rounded-full uppercase tracking-widest shadow-lg">
                    Premium Choice
                  </div>
                  <h4 class="text-2xl font-black text-white mb-3 tracking-tight">{{recommendation.policyProductName}}</h4>
                  <div class="h-px bg-white/10 w-1/2 mx-auto mb-4"></div>
                  <p class="text-sm text-slate-300 leading-relaxed italic px-2">"{{displayedReason}}<span class="animate-pulse inline-block w-1.5 h-4 bg-[#E8584A] ml-0.5" *ngIf="displayedReason !== recommendation.reason"></span>"</p>
                </div>
              </div>

              <div class="text-left space-y-3">
                <p class="text-[11px] font-black text-slate-400 uppercase tracking-widest px-2">Key AI Insights</p>
                <div class="grid grid-cols-1 gap-2">
                  <div *ngFor="let feature of recommendation.keyFeatures" class="flex items-center gap-4 text-[14px] font-bold text-slate-700 bg-slate-50 p-4 rounded-2xl border border-slate-100/50">
                    <span class="w-6 h-6 rounded-full bg-emerald-500/10 flex items-center justify-center text-emerald-500 font-black text-xs">✓</span>
                    {{feature}}
                  </div>
                </div>
              </div>

              <div class="flex flex-col gap-3 pt-4 border-t border-slate-100">
                <button (click)="proceed(recommendation.policyProductId)"
                  class="w-full py-5 bg-[#E8584A] text-white rounded-[24px] font-black text-[16px] hover:bg-[#D4483A] active:scale-95 transition-all flex items-center justify-center gap-3 shadow-xl shadow-red-500/20">
                  Select This Plan
                  <svg class="w-6 h-6" fill="currentColor" viewBox="0 0 20 20"><path d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" /></svg>
                </button>
                <button (click)="recommendation = null" class="text-sm font-bold text-slate-400 hover:text-[#E8584A] transition-colors py-2 group">
                   Wait <span class="group-hover:-translate-x-1 inline-block transition-transform">←</span> Re-run AI analysis
                </button>
              </div>
            </div>
          </ng-template>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class RecommendationModalComponent {
  @Output() closeEvent = new EventEmitter<void>();
  @Output() selectPlan = new EventEmitter<number>();

  loading = false;
  request: any = {
    Destination: '',
    DurationDays: 10,
    Age: 30,
    Purpose: 'Leisure',
    Budget: 'Medium'
  };

  recommendation: any = null;
  displayedReason: string = '';
  private typingInterval: any;

  purposes = ['Leisure', 'Business', 'Adventure'];
  budgets = ['Low', 'Medium', 'High'];

  constructor(private customerService: CustomerService) {}

  getRecommendation() {
    this.loading = true;
    this.recommendation = null;
    this.displayedReason = '';
    
    if (this.typingInterval) clearInterval(this.typingInterval);

    this.customerService.getRecommendation(this.request).subscribe({
      next: (res) => {
        this.recommendation = res;
        this.loading = false;
        this.startTypingEffect(res.reason);
      },
      error: () => {
        alert('Failed to get recommendation. Please try again.');
        this.loading = false;
      }
    });
  }

  private startTypingEffect(reason: string) {
    let index = 0;
    this.displayedReason = '';
    this.typingInterval = setInterval(() => {
      if (index < reason.length) {
        this.displayedReason += reason.charAt(index);
        index++;
      } else {
        clearInterval(this.typingInterval);
      }
    }, 20); // 20ms per character for a smooth typing feel
  }

  proceed(productId: number) {
    this.selectPlan.emit(productId);
  }

  close() {
    this.closeEvent.emit();
  }
}
