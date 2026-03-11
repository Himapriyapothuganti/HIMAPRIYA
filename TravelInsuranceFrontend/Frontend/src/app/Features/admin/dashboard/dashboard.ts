import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../../Services/admin.service';
import { Spinner } from '../components/spinner/spinner';
import { Toast } from '../components/toast/toast';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, Spinner, Toast],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  stats: any = null;
  isLoading = true;
  error = '';

  constructor(
    private adminService: AdminService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard() {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.adminService.getDashboardStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.prepareCharts();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error fetching dashboard stats', err);
        this.error = 'Failed to load dashboard data.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // --- CHART LOGIC (CUSTOM SVG) ---
  policyChart: any[] = [];
  claimsChart: any[] = [];
  agentChart: any[] = [];

  prepareCharts() {
    if (!this.stats) return;

    // 1. Policy Status Donut
    const pTotal = this.stats.totalPolicies || 1;
    this.policyChart = [
      { label: 'Active', value: this.stats.activePolicies, color: '#22C55E', offset: 0 },
      { label: 'Pending', value: this.stats.pendingPaymentPolicies, color: '#F59E0B', offset: 0 },
      { label: 'Expired', value: this.stats.expiredPolicies, color: '#94A3B8', offset: 0 }
    ];
    this.calculateOffsets(this.policyChart, pTotal);

    // 2. Claims Status Donut
    const cTotal = this.stats.totalClaims || 1;
    this.claimsChart = [
      { label: 'Under Review', value: this.stats.underReviewClaims, color: '#3B82F6', offset: 0 },
      { label: 'Approved', value: this.stats.approvedClaims, color: '#10B981', offset: 0 },
      { label: 'Rejected', value: this.stats.rejectedClaims, color: '#EF4444', offset: 0 },
      { label: 'Closed', value: this.stats.closedClaims, color: '#64748B', offset: 0 }
    ];
    this.calculateOffsets(this.claimsChart, cTotal);

    // 3. Agent Performance Bars
    if (this.stats.agentPerformance && this.stats.agentPerformance.length > 0) {
      // Sort by revenue descending first to get top performers
      const sortedAgents = [...this.stats.agentPerformance]
        .sort((a, b) => (b.totalPremiumCollected || 0) - (a.totalPremiumCollected || 0))
        .slice(0, 5);

      const maxRevRaw = Math.max(...sortedAgents.map((a: any) => a.totalPremiumCollected || 0), 0);
      
      // Calculate a "proper" scale max (e.g., round up to next nice power of 10 or 1.2x headroom)
      const scaleMax = maxRevRaw > 0 ? maxRevRaw * 1.2 : 1000;
      
      this.agentChart = sortedAgents.map((a: any) => ({
        name: a.agentName,
        revenue: a.totalPremiumCollected || 0,
        height: ((a.totalPremiumCollected || 0) / scaleMax) * 100
      }));

      // Generate 4 scale markers (e.g., 0, 33%, 66%, 100% of scaleMax)
      this.scaleMarkers = [
        Math.round(scaleMax),
        Math.round(scaleMax * 0.66),
        Math.round(scaleMax * 0.33),
        0
      ];
    }
  }

  scaleMarkers: number[] = [];

  calculateOffsets(segments: any[], total: number) {
    let currentOffset = 0;
    segments.forEach(seg => {
      seg.percentage = (seg.value / total) * 100;
      seg.strokeDash = `${seg.percentage} 100`;
      seg.offset = -currentOffset;
      currentOffset += seg.percentage;
    });
  }

  closeToast() {
    this.error = '';
    this.cdr.detectChanges();
  }
}
