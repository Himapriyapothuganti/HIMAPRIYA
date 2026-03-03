import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { AdminService } from '../../../Services/admin.service';
import { Spinner } from '../components/spinner/spinner';
import { Toast } from '../components/toast/toast';

@Component({
  selector: 'app-policy-products',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, Spinner, Toast],
  templateUrl: './policy-products.html',
  styleUrl: './policy-products.css',
})
export class PolicyProducts implements OnInit {
  products: any[] = [];
  filteredProducts: any[] = [];
  searchTerm: string = '';

  isLoading = false;
  toastMsg = '';
  toastType: 'success' | 'error' | 'info' = 'info';

  showCreateModal = false;
  createProductForm: FormGroup;
  isSubmitting = false;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.createProductForm = this.fb.group({
      policyName: ['', Validators.required],
      policyType: ['Single Trip', Validators.required],
      planTier: ['Silver', Validators.required],
      coverageDetails: ['', Validators.required],
      coverageLimit: [0, [Validators.required, Validators.min(1)]],
      basePremium: [0, [Validators.required, Validators.min(0.01)]],
      tenure: [30, [Validators.required, Validators.min(1)]],
      claimLimit: [0, [Validators.required, Validators.min(0)]],
      destinationZone: ['Worldwide', Validators.required]
    });
  }

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.isLoading = true;
    this.cdr.detectChanges();
    this.adminService.getPolicyProducts().subscribe({
      next: (data) => {
        this.products = data;
        this.filteredProducts = [...this.products];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.showToast('Failed to load policy products', 'error');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredProducts = this.products.filter(p =>
      p.policyName?.toLowerCase().includes(term) ||
      p.coverageDetails?.toLowerCase().includes(term)
    );
  }

  toggleStatus(product: any) {
    const newStatus = product.status !== 'Available';
    this.adminService.togglePolicyProductStatus(product.policyProductId, newStatus).subscribe({
      next: () => {
        product.status = newStatus ? 'Available' : 'Inactive';
        this.showToast(`Product ${newStatus ? 'activated' : 'deactivated'} successfully`, 'success');
        this.cdr.detectChanges();
      },
      error: () => {
        this.showToast('Failed to update product status', 'error');
        this.cdr.detectChanges();
      }
    });
  }

  deleteProduct(id: number) {
    if (!confirm('Are you sure you want to delete this policy product? This action cannot be undone.')) return;

    this.isLoading = true;
    this.cdr.detectChanges();
    this.adminService.deletePolicyProduct(id).subscribe({
      next: () => {
        this.showToast('Product deleted successfully', 'success');
        this.loadProducts();
      },
      error: () => {
        this.showToast('Failed to delete product', 'error');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openCreateModal() {
    this.createProductForm.reset({
      policyType: 'Single Trip',
      planTier: 'Silver',
      tenure: 30,
      coverageLimit: 0,
      basePremium: 0,
      claimLimit: 0,
      destinationZone: 'Worldwide'
    });
    this.showCreateModal = true;
  }

  closeCreateModal() {
    this.showCreateModal = false;
  }

  onCreateSubmit() {
    if (this.createProductForm.invalid) {
      this.createProductForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.cdr.detectChanges();
    this.adminService.createPolicyProduct(this.createProductForm.value).subscribe({
      next: () => {
        this.showToast('Product created successfully', 'success');
        this.closeCreateModal();
        this.loadProducts();
        this.isSubmitting = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.showToast(err.error?.message || 'Failed to create product', 'error');
        this.isSubmitting = false;
        this.cdr.detectChanges();
      }
    });
  }

  showToast(msg: string, type: 'success' | 'error' | 'info') {
    this.toastMsg = msg;
    this.toastType = type;
    this.cdr.detectChanges();
  }
}
