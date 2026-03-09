import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Users } from './users';
import { AdminService } from '../../../Services/admin.service';
import { of } from 'rxjs';

describe('Admin Users', () => {
    let component: Users;
    let fixture: ComponentFixture<Users>;
    let adminServiceSpy: jasmine.SpyObj<AdminService>;

    beforeEach(async () => {
        adminServiceSpy = jasmine.createSpyObj('AdminService', [
            'getUsers', 'createUser', 'toggleUserStatus'
        ]);
        adminServiceSpy.getUsers.and.returnValue(of([]));

        await TestBed.configureTestingModule({
            imports: [Users, HttpClientTestingModule, ReactiveFormsModule],
            providers: [
                { provide: AdminService, useValue: adminServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Users);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have empty users initially', () => {
        expect(component.users).toEqual([]);
        expect(component.filteredUsers).toEqual([]);
    });

    it('should filter users by search term', () => {
        component.users = [
            { fullName: 'Alice Smith', email: 'alice@test.com', role: 'Admin' },
            { fullName: 'Bob Jones', email: 'bob@test.com', role: 'Agent' }
        ];
        component.searchTerm = 'bob';
        component.onSearch();
        expect(component.filteredUsers.length).toBe(1);
        expect(component.filteredUsers[0].fullName).toBe('Bob Jones');
    });

    it('should open create modal', () => {
        component.openCreateModal();
        expect(component.showCreateModal).toBeTrue();
    });

    it('should close create modal', () => {
        component.showCreateModal = true;
        component.closeCreateModal();
        expect(component.showCreateModal).toBeFalse();
    });

    it('should validate email format in createUserForm', () => {
        const emailControl = component.createUserForm.get('email');
        emailControl?.setValue('invalid');
        expect(emailControl?.valid).toBeFalse();
        emailControl?.setValue('test@example.com');
        expect(emailControl?.valid).toBeTrue();
    });

    it('should require minimum password length', () => {
        const passwordControl = component.createUserForm.get('password');
        passwordControl?.setValue('12345');
        expect(passwordControl?.valid).toBeFalse();
        passwordControl?.setValue('123456');
        expect(passwordControl?.valid).toBeTrue();
    });

    it('should set toast via showToast', () => {
        component.showToast('User created', 'success');
        expect(component.toastMsg).toBe('User created');
        expect(component.toastType).toBe('success');
    });
});
