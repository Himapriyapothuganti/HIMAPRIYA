import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { AgentService } from '../../../Services/agent.service';
import { Spinner } from '../../admin/components/spinner/spinner';
import { Toast } from '../../admin/components/toast/toast';

@Component({
  selector: 'app-agent-policy-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, Spinner, Toast],
  templateUrl: './policy-detail.html'
})
export class PolicyDetail implements OnInit {
  isLoading = signal<boolean>(true);
  error = signal<string>('');

  policy = signal<any>(null);
  policyId: string = '';
  agentName: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private agentService: AgentService
  ) {
    this.agentName = localStorage.getItem('fullName') || 'Agent';
  }

  ngOnInit() {
    this.policyId = this.route.snapshot.paramMap.get('id') || '';
    if (this.policyId) {
      this.loadPolicy();
    } else {
      this.error.set('Invalid Policy ID');
      this.isLoading.set(false);
    }
  }

  loadPolicy() {
    this.isLoading.set(true);
    this.agentService.getPolicyById(this.policyId).subscribe({
      next: (data) => {
        this.policy.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching policy', err);
        this.error.set('Failed to load policy details. Make sure the API exists.');
        this.isLoading.set(false);
      }
    });
  }

  getCoverageList(): string[] {
    const policyData = this.policy();
    if (!policyData || !policyData.coverageDetails) return [];

    return policyData.coverageDetails.split(',').map((item: string) => item.trim());
  }

  downloadInvoice() {
    // Generate Invoice HTML and open print dialog
    const pol = this.policy();
    if (!pol) return;

    const invoiceWindow = window.open('', '_blank');
    if (!invoiceWindow) {
      this.error.set('Please allow popups to generate invoices.');
      return;
    }

    const htmlContent = `
      <html>
      <head>
        <title>Invoice - ${pol.policyNumber}</title>
        <style>
          body { font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; padding: 40px; color: #333; line-height: 1.6; }
          .header { display: flex; justify-content: space-between; align-items: flex-end; border-bottom: 2px solid #E8584A; padding-bottom: 20px; margin-bottom: 40px; }
          .header-left h1 { margin: 0; color: #E8584A; font-size: 32px; font-weight: 800; display: flex; align-items: center; gap: 10px; }
          .header-right { text-align: right; }
          .invoice-title { font-size: 24px; font-weight: bold; letter-spacing: 2px; color: #111; margin-bottom: 5px; }
          h3 { color: #E8584A; font-size: 14px; text-transform: uppercase; letter-spacing: 1px; border-bottom: 1px solid #eee; padding-bottom: 8px; margin-top: 30px; }
          .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
          .field { margin-bottom: 10px; }
          .label { font-size: 12px; color: #888; text-transform: uppercase; font-weight: bold; }
          .value { font-size: 16px; font-weight: bold; color: #111; }
          .premium-box { background: #fdf4f0; border: 1px solid #fbdcce; padding: 25px; border-radius: 12px; margin-top: 40px; }
          .premium-row { display: flex; justify-content: space-between; font-size: 16px; margin-bottom: 15px; }
          .premium-total { display: flex; justify-content: space-between; font-size: 24px; font-weight: bold; color: #E8584A; border-top: 2px solid #fbdcce; padding-top: 15px; margin-top: 10px; }
          .footer { margin-top: 60px; text-align: center; color: #888; font-size: 12px; border-top: 1px solid #eee; padding-top: 20px; }
          @media print { body { padding: 0; } }
        </style>
      </head>
      <body>
        <div class="header">
          <div class="header-left">
            <h1>Talk&Travel</h1>
            <div style="font-size: 14px; color: #666; font-weight: bold;">Travel Insurance Invoice</div>
          </div>
          <div class="header-right">
            <div class="invoice-title">INVOICE</div>
            <div class="field"><span class="label">Date:</span> <span class="value">${new Date().toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })}</span></div>
            <div class="field"><span class="label">Invoice No:</span> <span class="value">INV-${pol.policyNumber}</span></div>
            <div class="field"><span class="label">Agent:</span> <span class="value">${this.agentName}</span></div>
          </div>
        </div>

        <h3>Policy Details</h3>
        <div class="grid">
          <div class="field"><div class="label">Policy Number</div><div class="value">${pol.policyNumber}</div></div>
          <div class="field"><div class="label">Status</div><div class="value">${pol.status}</div></div>
          <div class="field"><div class="label">Plan</div><div class="value">${pol.policyName} (${pol.planTier})</div></div>
          <div class="field"><div class="label">Policy Type</div><div class="value">${pol.policyType}</div></div>
        </div>

        <h3>Customer & Traveller Information</h3>
        <div class="grid">
          <div class="field"><div class="label">Customer Name</div><div class="value">${pol.customerName}</div></div>
          <div class="field"><div class="label">Traveller Name</div><div class="value">${pol.travellerName} (Age: ${pol.travellerAge})</div></div>
          <div class="field"><div class="label">Passport No</div><div class="value">${pol.passportNumber || 'N/A'}</div></div>
          <div class="field"><div class="label">KYC Document</div><div class="value">${pol.kycType || 'N/A'} - ${pol.kycNumber || 'N/A'}</div></div>
        </div>

        <h3>Travel Details</h3>
        <div class="grid">
          <div class="field"><div class="label">Destination</div><div class="value">${pol.destination}</div></div>
          <div class="field"><div class="label">Travel Dates</div><div class="value">${new Date(pol.startDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })} to ${new Date(pol.endDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })}</div></div>
        </div>

        <div class="premium-box">
          <div class="premium-row">
            <div class="label" style="font-size: 16px;">Base Premium</div>
            <div class="value font-weight-normal">₹${Math.round(pol.premiumAmount / 1.18).toLocaleString('en-IN')}</div>
          </div>
          <div class="premium-row">
            <div class="label" style="font-size: 16px;">GST (18%)</div>
            <div class="value font-weight-normal">₹${Math.round(pol.premiumAmount - (pol.premiumAmount / 1.18)).toLocaleString('en-IN')}</div>
          </div>
          <div class="premium-total">
            <div>Total Premium Paid</div>
            <div>₹${pol.premiumAmount.toLocaleString('en-IN')}</div>
          </div>
        </div>

        <div class="footer">
          This is a system generated invoice. No physical signature is required.<br/>
          Thank you for choosing Talk&Travel.
        </div>
        
        <script>
          window.onload = function() { window.print(); }
        </script>
      </body>
      </html>
    `;

    invoiceWindow.document.write(htmlContent);
    invoiceWindow.document.close();
  }

  copyToClipboard(text: string) {
    navigator.clipboard.writeText(text);
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
