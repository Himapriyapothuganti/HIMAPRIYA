import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Claims } from './claims';

describe('Admin Claims', () => {
    let component: Claims;
    let fixture: ComponentFixture<Claims>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [Claims, HttpClientTestingModule]
        }).compileComponents();

        fixture = TestBed.createComponent(Claims);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have empty claims array initially', () => {
        expect(component.claims).toEqual([]);
        expect(component.filteredClaims).toEqual([]);
    });

    it('should default statusFilter to All', () => {
        expect(component.statusFilter).toBe('All');
    });

    it('should filter claims by status', () => {
        component.claims = [
            { status: 'Approved', customerName: 'Alice', policyNumber: 'P001', claimType: 'Medical' },
            { status: 'Rejected', customerName: 'Bob', policyNumber: 'P002', claimType: 'Travel' },
            { status: 'Approved', customerName: 'Charlie', policyNumber: 'P003', claimType: 'Medical' }
        ];
        component.statusFilter = 'Approved';
        component.applyFilters();
        expect(component.filteredClaims.length).toBe(2);
    });

    it('should filter claims by search term', () => {
        component.claims = [
            { status: 'Approved', customerName: 'Alice', policyNumber: 'P001', claimType: 'Medical' },
            { status: 'Rejected', customerName: 'Bob', policyNumber: 'P002', claimType: 'Travel' }
        ];
        component.searchTerm = 'Alice';
        component.statusFilter = 'All';
        component.applyFilters();
        expect(component.filteredClaims.length).toBe(1);
        expect(component.filteredClaims[0].customerName).toBe('Alice');
    });

    it('should set toast message and type via showToast', () => {
        component.showToast('Test message', 'error');
        expect(component.toastMsg).toBe('Test message');
        expect(component.toastType).toBe('error');
    });

    it('should call applyFilters on onSearch', () => {
        spyOn(component, 'applyFilters');
        component.onSearch();
        expect(component.applyFilters).toHaveBeenCalled();
    });

    it('should call applyFilters on onStatusChange', () => {
        spyOn(component, 'applyFilters');
        component.onStatusChange();
        expect(component.applyFilters).toHaveBeenCalled();
    });
});
