import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { MyPolicies } from './my-policies';
import { CustomerService } from '../../../Services/customer.service';
import { of } from 'rxjs';

describe('Customer MyPolicies', () => {
  let component: MyPolicies;
  let fixture: ComponentFixture<MyPolicies>;
  let customerServiceSpy: jasmine.SpyObj<CustomerService>;

  beforeEach(async () => {
    customerServiceSpy = jasmine.createSpyObj('CustomerService', ['getMyPolicies']);
    customerServiceSpy.getMyPolicies.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [MyPolicies, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: CustomerService, useValue: customerServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MyPolicies);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have isLoading true initially', () => {
    expect(component.isLoading).toBeTrue();
  });

  it('should default activeTab to All', () => {
    expect(component.activeTab).toBe('All');
  });

  it('should apply filter by tab', () => {
    component.policies = [
      { status: 'Active' },
      { status: 'PendingPayment' },
      { status: 'Active' }
    ];
    component.applyFilter('Active');
    expect(component.filteredPolicies.length).toBe(2);
    expect(component.activeTab).toBe('Active');
  });

  it('should show all policies when filter is All', () => {
    component.policies = [
      { status: 'Active' },
      { status: 'PendingPayment' }
    ];
    component.applyFilter('All');
    expect(component.filteredPolicies.length).toBe(2);
  });

  it('should return correct tier badge class for Silver', () => {
    expect(component.getTierBadgeClass('Silver')).toContain('bg-gray-100');
  });

  it('should return correct tier badge class for Gold', () => {
    expect(component.getTierBadgeClass('Gold')).toContain('bg-yellow-100');
  });

  it('should return correct status badge class for Active', () => {
    expect(component.getStatusBadgeClass('Active')).toContain('bg-green-100');
  });

  it('should return correct status badge class for Expired', () => {
    expect(component.getStatusBadgeClass('Expired')).toContain('bg-gray-100');
  });

  it('should clear error on closeToast', () => {
    component.error = 'Failed';
    component.closeToast();
    expect(component.error).toBe('');
  });
});
