import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CustomerService } from '../../../Services/customer.service';
import { Spinner } from '../../admin/components/spinner/spinner';
import { Modal } from '../../admin/components/modal/modal.component';

@Component({
  selector: 'app-browse-plans',
  standalone: true,
  imports: [CommonModule, Spinner, Modal],
  templateUrl: './browse-plans.html'
})
export class BrowsePlans implements OnInit {
  products: any[] = [];
  filteredProducts: any[] = [];
  isLoading = true;

  // Modal
  isModalOpen = false;
  selectedPlan: any = null;

  // Filters
  activeFilter: string = 'All';
  filterOptions = ['All', 'Single Trip', 'Multi-Trip', 'Family', 'Student'];

  // Group order
  private typeOrder = ['Single Trip', 'Multi-Trip', 'Family', 'Student'];

  constructor(
    private customerService: CustomerService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) { }

  ngOnInit() {
    this.loadProducts();
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

  // Returns groups sorted by typeOrder, each with type + plans sorted by tier
  get groupedPlans(): { type: string; plans: any[] }[] {
    const tierOrder: Record<string, number> = { Silver: 0, Gold: 1, Platinum: 2 };

    // Determine which types to include
    const types = this.activeFilter === 'All'
      ? this.typeOrder
      : [this.activeFilter];

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

  getTierEmoji(tier: string): string {
    const map: Record<string, string> = { Silver: '🥈', Gold: '🥇', Platinum: '💎' };
    return map[tier] ?? '';
  }

  getCoverageList(details: string): string[] {
    return details.split(',').map(d => d.trim()).filter(d => d.length > 0);
  }

  getIncludedItems(tier: string): string[] {
    const silver = [
      'Medical Expenses & Hospitalization',
      'Emergency Medical Evacuation',
      'Repatriation of Remains',
      'Trip Cancellation',
      'Personal Accident',
      'Loss of Passport'
    ];
    const goldExtra = [
      'Baggage Loss & Delay',
      'Flight Delay (>4hrs)',
      'Missed Flight Connection',
      'Emergency Dental Treatment',
      'Hijack Distress Allowance'
    ];
    const platinumExtra = [
      'Adventure Sports Coverage',
      'Pre-existing Conditions',
      'COVID-19 Coverage',
      'Daily Hospital Cash Allowance',
      'Emergency Hotel Extension'
    ];

    if (tier === 'Silver') return silver;
    if (tier === 'Gold') return [...silver, ...goldExtra];
    if (tier === 'Platinum') return [...silver, ...goldExtra, ...platinumExtra];
    return silver;
  }

  getExcludedItems(tier: string): string[] {
    if (tier === 'Silver') return [
      'Baggage Loss & Delay',
      'Flight Delay Coverage',
      'Adventure Sports',
      'Pre-existing Conditions',
      'COVID-19 Coverage',
      'Emergency Dental Treatment'
    ];
    if (tier === 'Gold') return [
      'Adventure Sports',
      'Pre-existing Conditions',
      'COVID-19 Coverage',
      'Emergency Hotel Extension'
    ];
    if (tier === 'Platinum') return [
      'War & Terror Zone Coverage'
    ];
    return [];
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
}
