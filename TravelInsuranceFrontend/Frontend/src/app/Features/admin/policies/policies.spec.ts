import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Policies } from './policies';
import { AdminService } from '../../../Services/admin.service';
import { of } from 'rxjs';

describe('Admin Policies', () => {
    let component: Policies;
    let fixture: ComponentFixture<Policies>;
    let adminServiceSpy: jasmine.SpyObj<AdminService>;

    beforeEach(async () => {
        adminServiceSpy = jasmine.createSpyObj('AdminService', [
            'getUsers', 'assignAgentToPolicy'
        ]);
        adminServiceSpy.getUsers.and.returnValue(of([]));

        await TestBed.configureTestingModule({
            imports: [Policies, HttpClientTestingModule, ReactiveFormsModule],
            providers: [
                { provide: AdminService, useValue: adminServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Policies);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have empty policies initially', () => {
        expect(component.policies).toEqual([]);
        expect(component.filteredPolicies).toEqual([]);
    });

    it('should filter policies by search term', () => {
        component.policies = [
            { customerName: 'Alice', policyName: 'Gold Plan', policyNumber: 'P001', agentName: 'Agent1' },
            { customerName: 'Bob', policyName: 'Silver Plan', policyNumber: 'P002', agentName: 'Agent2' }
        ];
        component.searchTerm = 'alice';
        component.onSearch();
        expect(component.filteredPolicies.length).toBe(1);
    });

    it('should open assign modal with policy data', () => {
        const policy = { policyId: 1, agentId: 'a1' };
        component.openAssignModal(policy);
        expect(component.showAssignModal).toBeTrue();
        expect(component.selectedPolicyId).toBe(1);
    });

    it('should close assign modal', () => {
        component.showAssignModal = true;
        component.selectedPolicyId = 1;
        component.closeAssignModal();
        expect(component.showAssignModal).toBeFalse();
        expect(component.selectedPolicyId).toBeNull();
    });

    it('should set toast message via showToast', () => {
        component.showToast('Success!', 'success');
        expect(component.toastMsg).toBe('Success!');
        expect(component.toastType).toBe('success');
    });

    it('should have assignForm with agentId control', () => {
        expect(component.assignForm.contains('agentId')).toBeTrue();
    });
});
