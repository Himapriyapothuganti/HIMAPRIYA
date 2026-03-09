import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { PolicyProducts } from './policy-products';
import { AdminService } from '../../../Services/admin.service';
import { of, throwError } from 'rxjs';

describe('Admin PolicyProducts', () => {
    let component: PolicyProducts;
    let fixture: ComponentFixture<PolicyProducts>;
    let adminServiceSpy: jasmine.SpyObj<AdminService>;

    beforeEach(async () => {
        adminServiceSpy = jasmine.createSpyObj('AdminService', [
            'getPolicyProducts', 'createPolicyProduct', 'togglePolicyProductStatus', 'deletePolicyProduct'
        ]);
        adminServiceSpy.getPolicyProducts.and.returnValue(of([]));

        await TestBed.configureTestingModule({
            imports: [PolicyProducts, HttpClientTestingModule, ReactiveFormsModule],
            providers: [
                { provide: AdminService, useValue: adminServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(PolicyProducts);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have empty products initially', () => {
        expect(component.products).toEqual([]);
    });

    it('should filter products by search term', () => {
        component.products = [
            { policyName: 'Gold Trip', coverageDetails: 'Medical' },
            { policyName: 'Silver Plan', coverageDetails: 'Baggage' }
        ];
        component.searchTerm = 'gold';
        component.onSearch();
        expect(component.filteredProducts.length).toBe(1);
    });

    it('should open create modal with default form values', () => {
        component.openCreateModal();
        expect(component.showCreateModal).toBeTrue();
        expect(component.createProductForm.get('policyType')?.value).toBe('Single Trip');
        expect(component.createProductForm.get('planTier')?.value).toBe('Silver');
    });

    it('should close create modal', () => {
        component.showCreateModal = true;
        component.closeCreateModal();
        expect(component.showCreateModal).toBeFalse();
    });

    it('should require policyName in form', () => {
        const control = component.createProductForm.get('policyName');
        control?.setValue('');
        expect(control?.valid).toBeFalse();
        control?.setValue('Test Plan');
        expect(control?.valid).toBeTrue();
    });

    it('should set toast message via showToast', () => {
        component.showToast('Created!', 'success');
        expect(component.toastMsg).toBe('Created!');
        expect(component.toastType).toBe('success');
    });
});
