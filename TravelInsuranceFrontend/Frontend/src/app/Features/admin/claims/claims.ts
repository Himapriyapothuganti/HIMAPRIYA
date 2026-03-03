import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Spinner } from '../components/spinner/spinner';
import { Toast } from '../components/toast/toast';

@Component({
  selector: 'app-claims',
  standalone: true,
  imports: [CommonModule, FormsModule, Spinner, Toast],
  templateUrl: './claims.html',
  styleUrl: './claims.css',
})
export class Claims implements OnInit {
  claims: any[] = [];
  filteredClaims: any[] = [];

  searchTerm: string = '';
  statusFilter: string = 'All';

  isLoading = false;
  toastMsg = '';
  toastType: 'success' | 'error' | 'info' = 'info';

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.loadClaims();
  }

  loadClaims() {
    this.isLoading = true;
    this.cdr.detectChanges();
    this.http.get<any[]>('https://localhost:7161/api/Claim/all').subscribe({
      next: (data) => {
        this.claims = data;
        this.applyFilters();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.showToast('Failed to load claims', 'error');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters() {
    let result = this.claims;

    if (this.statusFilter !== 'All') {
      result = result.filter(c => c.status === this.statusFilter);
    }

    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      result = result.filter(c =>
        c.customerName?.toLowerCase().includes(term) ||
        c.policyNumber?.toLowerCase().includes(term) ||
        c.claimType?.toLowerCase().includes(term)
      );
    }

    this.filteredClaims = result;
  }

  onSearch() {
    this.applyFilters();
  }

  onStatusChange() {
    this.applyFilters();
  }

  showToast(msg: string, type: 'success' | 'error' | 'info') {
    this.toastMsg = msg;
    this.toastType = type;
  }
}
