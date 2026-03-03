import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { AdminService } from '../../../Services/admin.service';
import { Spinner } from '../components/spinner/spinner';
import { Toast } from '../components/toast/toast';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, Spinner, Toast],
  templateUrl: './users.html',
  styleUrl: './users.css',
})
export class Users implements OnInit {
  users: any[] = [];
  filteredUsers: any[] = [];
  searchTerm: string = '';

  isLoading = false;
  toastMsg = '';
  toastType: 'success' | 'error' | 'info' = 'info';

  showCreateModal = false;
  createUserForm: FormGroup;
  isSubmitting = false;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.createUserForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.isLoading = true;
    this.cdr.detectChanges();
    this.adminService.getUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.filteredUsers = [...this.users];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.showToast('Failed to load users', 'error');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredUsers = this.users.filter(u =>
      u.fullName.toLowerCase().includes(term) ||
      u.email.toLowerCase().includes(term) ||
      u.role.toLowerCase().includes(term)
    );
  }

  toggleStatus(user: any) {
    const newStatus = !user.isActive;
    this.adminService.toggleUserStatus(user.userId || user.id, newStatus).subscribe({
      next: () => {
        user.isActive = newStatus;
        this.showToast(`User ${newStatus ? 'activated' : 'deactivated'} successfully`, 'success');
        this.cdr.detectChanges();
      },
      error: () => {
        this.showToast('Failed to update user status', 'error');
        this.cdr.detectChanges();
      }
    });
  }

  openCreateModal() {
    this.createUserForm.reset({ role: '' });
    this.showCreateModal = true;
  }

  closeCreateModal() {
    this.showCreateModal = false;
  }

  onCreateSubmit() {
    if (this.createUserForm.invalid) {
      this.createUserForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.cdr.detectChanges();
    this.adminService.createUser(this.createUserForm.value).subscribe({
      next: () => {
        this.showToast('User created successfully', 'success');
        this.closeCreateModal();
        this.loadUsers();
        this.isSubmitting = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.showToast(err.error?.message || 'Failed to create user', 'error');
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
