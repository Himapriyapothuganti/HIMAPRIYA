import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PolicyRequestService, AgentPolicyRequestResponse } from '../../../Services/policy-request.service';
import { Spinner } from '../../admin/components/spinner/spinner';

@Component({
  selector: 'app-policy-request-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, Spinner],
  templateUrl: './policy-request-detail.html'
})
export class PolicyRequestDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private location = inject(Location);
  private fb = inject(FormBuilder);
  private policyRequestService = inject(PolicyRequestService);

  request = signal<AgentPolicyRequestResponse | null>(null);
  isLoading = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);
  Math = Math;

  reviewForm: FormGroup;
  showRejectReason = false;

  constructor() {
    this.reviewForm = this.fb.group({
      status: ['Approved', Validators.required],
      rejectionReason: [''],
      agentNotes: ['']
    });

    this.reviewForm.get('status')?.valueChanges.subscribe(status => {
      this.showRejectReason = status === 'Rejected';
      const reasonControl = this.reviewForm.get('rejectionReason');
      if (this.showRejectReason) {
        reasonControl?.setValidators([Validators.required]);
      } else {
        reasonControl?.clearValidators();
      }
      reasonControl?.updateValueAndValidity();
    });
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadRequest(+id);
    } else {
      this.goBack();
    }
  }

  loadRequest(id: number) {
    this.isLoading.set(true);
    this.policyRequestService.getAgentRequestById(id).subscribe({
      next: (res) => {
        this.request.set(res);
        this.isLoading.set(false);

        // If already reviewed, patch the form
        if (res.status === 'Approved' || res.status === 'Rejected') {
          this.reviewForm.patchValue({
            status: res.status,
            rejectionReason: res.rejectionReason || '',
            agentNotes: res.agentNotes || ''
          });
        }
      },
      error: (err) => {
        console.error(err);
        this.isLoading.set(false);
        alert('Failed to load request.');
        this.goBack();
      }
    });
  }

  submitReview() {
    const req = this.request();
    if (this.reviewForm.invalid || !req) return;

    if (req.status !== 'Pending') {
      alert('This request has already been reviewed.');
      return;
    }

    this.isSubmitting.set(true);
    const dto = this.reviewForm.value;

    this.policyRequestService.reviewRequest(req.policyRequestId, dto).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        alert(`Request ${dto.status} successfully.`);
        this.request.set(res); // Update local data
        this.router.navigate(['/agent/policy-requests']);
      },
      error: (err) => {
        console.error(err);
        this.isSubmitting.set(false);
        alert(err.error?.message || 'Failed to review request.');
      }
    });
  }

  downloadDocument(docId: number) {
    const req = this.request();
    if (!req) return;
    const doc = req.documents.find(d => d.policyRequestDocumentId === docId);
    if (!doc) return;

    this.policyRequestService.downloadDocument(doc.fileUrl).subscribe({
      next: (blob: Blob) => {
        const downloadUrl = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = downloadUrl;
        a.download = doc.fileName;
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

  goBack() {
    this.location.back();
  }

  getRiskBadgeClass(level: string): string {
    switch (level) {
      case 'Low': return 'bg-green-50 text-green-700 border-green-200';
      case 'Medium': return 'bg-yellow-50 text-yellow-700 border-yellow-200';
      case 'High': return 'bg-red-50 text-red-700 border-red-200';
      default: return 'bg-gray-50 text-gray-700 border-gray-200';
    }
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Pending': return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'Approved': return 'bg-green-100 text-green-800 border-green-200';
      case 'Rejected': return 'bg-red-100 text-red-800 border-red-200';
      case 'Purchased': return 'bg-[#FDE8E0] text-[#E8584A] border-[#E8584A]/20';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
}
