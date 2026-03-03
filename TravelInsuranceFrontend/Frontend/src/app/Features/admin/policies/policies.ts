import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AdminService } from '../../../Services/admin.service';
import { Spinner } from '../components/spinner/spinner';
import { Toast } from '../components/toast/toast';

@Component({
  selector: 'app-policies',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, Spinner, Toast],
  templateUrl: './policies.html',
  styleUrl: './policies.css',
})
export class Policies implements OnInit {
  policies: any[] = [];
  filteredPolicies: any[] = [];
  searchTerm: string = '';

  agents: any[] = [];

  isLoading = false;
  toastMsg = '';
  toastType: 'success' | 'error' | 'info' = 'info';

  showAssignModal = false;
  assignForm: FormGroup;
  isSubmitting = false;
  selectedPolicyId: number | null = null;

  constructor(
    private adminService: AdminService,
    private http: HttpClient,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.assignForm = this.fb.group({
      agentId: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.loadPolicies();
    this.loadAgents();
  }

  loadPolicies() {
    this.isLoading = true;
    this.cdr.detectChanges();
    this.http.get<any[]>('https://localhost:7161/api/Policy/all').subscribe({
      next: (data) => {
        this.policies = data;
        this.filteredPolicies = [...this.policies];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.showToast('Failed to load policies', 'error');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadAgents() {
    this.adminService.getUsers().subscribe({
      next: (users) => {
        this.agents = users.filter(u => u.role === 'Agent' && u.isActive);
        this.cdr.detectChanges();
      }
    });
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredPolicies = this.policies.filter(p =>
      p.customerName?.toLowerCase().includes(term) ||
      p.policyName?.toLowerCase().includes(term) ||
      p.policyNumber?.toLowerCase().includes(term) ||
      p.agentName?.toLowerCase().includes(term)
    );
  }

  openAssignModal(policy: any) {
    this.selectedPolicyId = policy.policyId;
    this.assignForm.reset({ agentId: policy.agentId || '' });
    this.showAssignModal = true;
  }

  closeAssignModal() {
    this.showAssignModal = false;
    this.selectedPolicyId = null;
  }

  onAssignSubmit() {
    if (this.assignForm.invalid || !this.selectedPolicyId) return;

    this.isSubmitting = true;
    this.cdr.detectChanges();
    const payload = {
      policyId: this.selectedPolicyId,
      agentId: this.assignForm.value.agentId
    };

    this.adminService.assignAgentToPolicy(payload).subscribe({
      next: () => {
        this.showToast('Agent assigned successfully', 'success');
        this.closeAssignModal();
        this.loadPolicies();
        this.isSubmitting = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.showToast(err.error?.message || 'Failed to assign agent', 'error');
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
