import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Dashboard } from './dashboard';
import { CustomerService } from '../../../Services/customer.service';
import { of, throwError } from 'rxjs';

describe('Customer Dashboard', () => {
  let component: Dashboard;
  let fixture: ComponentFixture<Dashboard>;
  let customerServiceSpy: jasmine.SpyObj<CustomerService>;

  beforeEach(async () => {
    customerServiceSpy = jasmine.createSpyObj('CustomerService', ['getMyPolicies', 'getMyClaims']);
    customerServiceSpy.getMyPolicies.and.returnValue(of([]));
    customerServiceSpy.getMyClaims.and.returnValue(of([]));

    spyOn(localStorage, 'getItem').and.returnValue(null);

    await TestBed.configureTestingModule({
      imports: [Dashboard, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: CustomerService, useValue: customerServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have isLoading true initially', () => {
    expect(component.isLoading).toBeTrue();
  });

  it('should default userName to Traveller', () => {
    expect(component.userName).toBe('Traveller');
  });

  it('should process policy stats correctly', () => {
    component.policies = [
      { status: 'Active', createdAt: '2024-01-01' },
      { status: 'PendingPayment', createdAt: '2024-01-02' },
      { status: 'Active', createdAt: '2024-01-03' }
    ];
    component.processPolicyStats();
    expect(component.totalPolicies).toBe(3);
    expect(component.activePolicies).toBe(2);
    expect(component.pendingPaymentPolicies).toBe(1);
  });

  it('should sort recent policies by date descending', () => {
    component.policies = [
      { status: 'Active', createdAt: '2024-01-01' },
      { status: 'Active', createdAt: '2024-06-01' },
      { status: 'Active', createdAt: '2024-03-01' }
    ];
    component.processPolicyStats();
    expect(component.recentPolicies[0].createdAt).toBe('2024-06-01');
  });

  it('should process claim stats correctly', () => {
    component.claims = [
      { status: 'UnderReview', claimDate: '2024-01-01' },
      { status: 'Approved', claimDate: '2024-01-02' },
      { status: 'Approved', claimDate: '2024-01-03' }
    ];
    component.processClaimStats();
    expect(component.totalClaims).toBe(3);
    expect(component.underReviewClaims).toBe(1);
    expect(component.approvedClaims).toBe(2);
  });

  it('should clear error on closeToast', () => {
    component.error = 'Something went wrong';
    component.closeToast();
    expect(component.error).toBe('');
  });

  it('should load data on init', () => {
    fixture.detectChanges();
    expect(customerServiceSpy.getMyPolicies).toHaveBeenCalled();
  });
});
