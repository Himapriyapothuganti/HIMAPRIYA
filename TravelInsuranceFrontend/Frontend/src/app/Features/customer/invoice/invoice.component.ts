import { Component, EventEmitter, Output, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-policy-invoice',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed inset-0 z-[60] flex items-center justify-center p-4 bg-black/70 backdrop-blur-md overflow-y-auto">
      <div id="invoice-container" class="bg-white w-full max-w-4xl shadow-2xl relative animate-in fade-in zoom-in duration-500">
        
        <!-- Controls -->
        <div class="absolute -top-14 right-0 flex gap-3 no-print">
          <button (click)="print()" class="px-6 py-2 bg-white text-black font-bold rounded-full hover:bg-gray-100 transition-all flex items-center gap-2">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" /></svg>
            Print Invoice
          </button>
          <button (click)="close()" class="p-2 bg-white/20 text-white rounded-full hover:bg-white/40 transition-colors">
            <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
          </button>
        </div>

        <!-- Invoice Body -->
        <div class="p-12 font-inter text-slate-800">
          
          <!-- Header Branding -->
          <div class="flex justify-between items-start border-b-2 border-slate-100 pb-10 mb-10">
            <div class="space-y-4">
              <div class="flex items-center gap-3">
                <div class="w-12 h-12 bg-black rounded-2xl flex items-center justify-center text-white font-black text-2xl shadow-lg ring-4 ring-black/5">TT</div>
                <h1 class="text-3xl font-black tracking-tight text-black">{{invoice.companyName}}</h1>
              </div>
              <div class="text-sm text-slate-500 max-w-xs leading-relaxed">
                {{invoice.companyAddress}}<br>
              </div>
            </div>
            <div class="text-right">
              <h2 class="text-6xl font-black text-slate-100 mb-4 select-none">INVOICE</h2>
              <div class="space-y-1">
                <p class="text-sm text-slate-400 font-bold uppercase tracking-widest">Invoice Number</p>
                <p class="text-xl font-mono font-bold text-black">{{invoice.invoiceNumber}}</p>
              </div>
            </div>
          </div>

          <!-- Info Grid -->
          <div class="grid grid-cols-2 gap-12 mb-12">
            <div class="space-y-6">
              <div>
                <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-3">Bill To</h3>
                <p class="text-2xl font-bold text-black">{{invoice.customerName}}</p>
                <p class="text-slate-500 font-medium">Customer ID: {{invoice.invoiceNumber.split('-')[2]}}</p>
              </div>
              <div class="bg-slate-50 rounded-2xl p-6 border border-slate-100">
                <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-3">Trip Details</h3>
                <div class="space-y-2">
                  <div class="flex justify-between text-sm">
                    <span class="text-slate-500">Destination:</span>
                    <span class="font-bold text-black">{{invoice.destination}}</span>
                  </div>
                  <div class="flex justify-between text-sm">
                    <span class="text-slate-500">Traveller:</span>
                    <span class="font-bold text-black">{{invoice.travellerName}}</span>
                  </div>
                  <div class="flex justify-between text-sm">
                    <span class="text-slate-500">Period:</span>
                    <span class="font-bold text-black">{{invoice.startDate | date}} - {{invoice.endDate | date}}</span>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="space-y-6">
              <div class="text-right space-y-4">
                <div>
                  <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-1">Issue Date</h3>
                  <p class="text-lg font-bold text-black">{{invoice.purchaseDate | date:'mediumDate'}}</p>
                </div>
                <div>
                  <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-1">Policy Number</h3>
                  <p class="text-lg font-bold text-[#E8584A]">{{invoice.policyNumber}}</p>
                </div>
                <div>
                  <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-1">Plan Name</h3>
                  <p class="text-lg font-bold text-black">{{invoice.policyName}}</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Table -->
          <div class="mb-12">
            <table class="w-full text-left">
              <thead>
                <tr class="bg-black text-white">
                  <th class="px-6 py-4 rounded-l-xl font-bold uppercase text-xs tracking-wider">Description</th>
                  <th class="px-6 py-4 font-bold uppercase text-xs tracking-wider text-right">Quantity</th>
                  <th class="px-6 py-4 rounded-r-xl font-bold uppercase text-xs tracking-wider text-right">Amount</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr>
                  <td class="px-6 py-8">
                    <p class="font-bold text-lg text-black">{{invoice.policyName}}</p>
                    <p class="text-sm text-slate-500">Comprehensive Travel Insurance Premium</p>
                  </td>
                  <td class="px-6 py-8 text-right font-medium text-slate-600">1</td>
                  <td class="px-6 py-8 text-right font-bold text-black text-lg">₹{{invoice.basePremium | number:'1.2-2'}}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Total Section -->
          <div class="flex justify-end">
            <div class="w-full max-w-xs space-y-4">
              <div class="flex justify-between text-slate-500 font-medium pb-4 border-b border-slate-100">
                <span>Base Premium</span>
                <span>₹{{invoice.basePremium | number:'1.2-2'}}</span>
              </div>
              <div class="flex justify-between items-center bg-[#E8584A] text-white p-6 rounded-2xl shadow-xl shadow-red-500/20">
                <span class="font-bold text-lg">Total Amount</span>
                <span class="text-3xl font-black italic">₹{{invoice.totalAmount | number:'1.2-2'}}</span>
              </div>
              <div class="text-right pt-2">
                <p class="text-[10px] text-slate-400 font-bold uppercase tracking-widest">Payment via {{invoice.paymentMethod}}</p>
              </div>
            </div>
          </div>

          <!-- Footer -->
          <div class="mt-20 pt-10 border-t border-slate-100 flex justify-between items-end">
            <div class="space-y-4">
              <div class="h-20 w-48 bg-slate-50 rounded-lg flex items-center justify-center border-2 border-dashed border-slate-100 italic text-slate-300 text-xs">
                Authorised Signatory
              </div>
              <div class="space-y-1">
                <p class="text-sm font-bold text-black">Thank you for choosing {{invoice.companyName}}</p>
                <p class="text-xs text-slate-400">This is a computer-generated invoice and doesn't require a physical signature.</p>
              </div>
            </div>
            <div class="text-right">
              <p class="text-xs font-bold text-slate-300 uppercase tracking-widest mb-2">Support</p>
              <p class="text-sm font-medium text-slate-600">support@talktravel.com</p>
              <p class="text-sm font-medium text-slate-600">+91 1800-TRAVEL-SMART</p>
            </div>
          </div>

        </div>
      </div>
    </div>
  `,
  styles: [`
    .font-inter { font-family: 'Inter', sans-serif; }
    @media print {
      .no-print { display: none !important; }
      #invoice-container { box-shadow: none !important; margin: 0 !important; max-width: 100% !important; }
      body { background: white !important; }
    }
  `]
})
export class InvoiceComponent {
  @Input() invoice: any;
  @Output() closeEvent = new EventEmitter<void>();

  print() {
    window.print();
  }

  close() {
    this.closeEvent.emit();
  }
}
