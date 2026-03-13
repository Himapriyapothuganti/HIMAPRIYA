import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CustomerService } from '../../../Services/customer.service';
import { PolicyRequestService } from '../../../Services/policy-request.service';
import { Spinner } from '../../admin/components/spinner/spinner';
import { Modal } from '../../admin/components/modal/modal.component';
import { PolicyRequestModalComponent } from '../policy-request-modal/policy-request-modal.component';

@Component({
  selector: 'app-browse-plans',
  standalone: true,
  imports: [CommonModule, Spinner, Modal, PolicyRequestModalComponent],
  templateUrl: './browse-plans.html'
})
export class BrowsePlans implements OnInit {
  products: any[] = [];
  filteredProducts: any[] = [];
  isLoading = true;

  ownedProductIds: Set<number> = new Set();
  requestedProductIds: Set<number> = new Set();

  // Modals
  isModalOpen = false;
  selectedPlan: any = null;

  isRequestModalOpen = false;

  // Filters
  activeFilter: string = 'All';
  filterOptions = ['All', 'Single Trip', 'Multi-Trip', 'Family', 'Student'];

  // Group order
  private typeOrder = ['Single Trip', 'Multi-Trip', 'Family', 'Student'];

  constructor(
    private customerService: CustomerService,
    private policyRequestService: PolicyRequestService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) { }

  ngOnInit() {
    this.loadUserData();
    this.loadProducts();
  }

  loadUserData() {
    this.customerService.getMyPolicies().subscribe({
      next: (policies) => {
        setTimeout(() => {
          policies.filter(p => p.status === 'Active' || p.status === 'PendingPayment')
            .forEach(p => this.ownedProductIds.add(p.policyProductId));
          this.cdr.detectChanges();
        });
      }
    });

    this.policyRequestService.getMyRequests().subscribe({
      next: (requests) => {
        setTimeout(() => {
          requests.filter(r => r.status === 'Pending' || r.status === 'Approved')
            .forEach(r => this.requestedProductIds.add(r.policyProductId));
          this.cdr.detectChanges();
        });
      }
    });
  }

  loadProducts() {
    this.isLoading = true;
    this.customerService.getPolicyProducts().subscribe({
      next: (data) => {
        this.ngZone.run(() => {
          this.products = data.filter(p => p.status === 'Available');
          this.applyFilter(this.activeFilter);
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        console.error('Failed to load products', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilter(filterType: string) {
    this.activeFilter = filterType;
    if (filterType === 'All') {
      this.filteredProducts = [...this.products];
    } else {
      this.filteredProducts = this.products.filter(p => p.policyType === filterType);
    }
  }

  get groupedPlans(): { type: string; plans: any[] }[] {
    const tierOrder: Record<string, number> = { Silver: 0, Gold: 1, Platinum: 2 };
    const types = this.activeFilter === 'All' ? this.typeOrder : [this.activeFilter];

    return types
      .map(type => ({
        type,
        plans: this.filteredProducts
          .filter(p => p.policyType === type)
          .sort((a, b) => (tierOrder[a.planTier] ?? 99) - (tierOrder[b.planTier] ?? 99))
      }))
      .filter(g => g.plans.length > 0);
  }

  getGroupIcon(type: string): string {
    const map: Record<string, string> = {
      'Single Trip': '✈️',
      'Multi-Trip': '🔄',
      'Family': '👨‍👩‍👧‍👦',
      'Student': '🎓'
    };
    return map[type] ?? '🌍';
  }

  getGroupSubtitle(type: string): string {
    const map: Record<string, string> = {
      'Single Trip': 'Perfect for one-time travellers. Coverage from departure to return.',
      'Multi-Trip': 'Ideal for frequent flyers. Annual plan covering multiple trips.',
      'Family': 'Complete protection for 2 Adults + 2 Children travelling together.',
      'Student': 'Long-term coverage for students studying abroad. Includes study interruption.'
    };
    return map[type] ?? '';
  }

  getTierBadgeClass(tier: string): string {
    switch (tier) {
      case 'Silver': return 'bg-gray-100 text-gray-600 border-gray-200';
      case 'Gold': return 'bg-yellow-50 text-yellow-700 border-yellow-200';
      case 'Platinum': return 'bg-[#FDE8E0] text-[#E8584A] border-[#E8584A]/20';
      default: return 'bg-blue-50 text-blue-600 border-blue-100';
    }
  }

  getCoverageList(details: string): string[] {
    if (!details) return [];
    return details.split(/,\s*(?![^()]*\))/).map(d => d.trim()).filter(d => d.length > 0);
  }

  buyNow(productId: number) {
    this.router.navigate(['/customer/purchase', productId]);
  }

  openDetails(plan: any) {
    this.selectedPlan = plan;
    this.isModalOpen = true;
  }

  closeDetails() {
    this.isModalOpen = false;
    this.selectedPlan = null;
  }

  openRequestModal(plan: any) {
    this.selectedPlan = plan;
    this.isModalOpen = false;
    this.isRequestModalOpen = true;
  }

  closeRequestModal() {
    this.isRequestModalOpen = false;
    this.selectedPlan = null;
  }

  onRequestSubmitted(response: any) {
    // SUCCESS toast is handled by the PolicyRequestModal internally
    // We could navigate if we wanted to
  }
}
