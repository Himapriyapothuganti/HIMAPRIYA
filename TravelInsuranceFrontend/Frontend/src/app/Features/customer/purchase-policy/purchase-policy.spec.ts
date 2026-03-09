import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { PurchasePolicy } from './purchase-policy';
import { CustomerService } from '../../../Services/customer.service';
import { of } from 'rxjs';

describe('Customer PurchasePolicy', () => {
  let component: PurchasePolicy;
  let fixture: ComponentFixture<PurchasePolicy>;
  let customerServiceSpy: jasmine.SpyObj<CustomerService>;

  beforeEach(async () => {
    customerServiceSpy = jasmine.createSpyObj('CustomerService', ['getPolicyProducts', 'purchasePolicy']);
    customerServiceSpy.getPolicyProducts.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [PurchasePolicy, HttpClientTestingModule, RouterTestingModule, ReactiveFormsModule],
      providers: [
        // DatePipe is injected in the component constructor for formatting dates
        DatePipe,
        { provide: CustomerService, useValue: customerServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => '1' } } }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PurchasePolicy);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize purchaseForm with required controls', () => {
    expect(component.purchaseForm).toBeDefined();
    expect(component.purchaseForm.contains('destination')).toBeTrue();
    expect(component.purchaseForm.contains('startDate')).toBeTrue();
    expect(component.purchaseForm.contains('endDate')).toBeTrue();
    expect(component.purchaseForm.contains('travellerName')).toBeTrue();
    expect(component.purchaseForm.contains('travellerAge')).toBeTrue();
    expect(component.purchaseForm.contains('passportNumber')).toBeTrue();
    expect(component.purchaseForm.contains('kycType')).toBeTrue();
    expect(component.purchaseForm.contains('kycNumber')).toBeTrue();
  });

  it('should require destination', () => {
    const control = component.purchaseForm.get('destination');
    control?.setValue('');
    expect(control?.valid).toBeFalse();
    control?.setValue('London');
    expect(control?.valid).toBeTrue();
  });

  it('should validate passport number format', () => {
    const control = component.purchaseForm.get('passportNumber');
    control?.setValue('invalid');
    expect(control?.valid).toBeFalse();
    control?.setValue('A1234567');
    expect(control?.valid).toBeTrue();
  });

  it('should validate date range (end after start)', () => {
    // The custom dateRangeValidator should catch when endDate is before startDate
    component.purchaseForm.get('startDate')?.setValue('2025-06-01');
    component.purchaseForm.get('endDate')?.setValue('2025-05-01');
    const result = component.dateRangeValidator(component.purchaseForm);
    expect(result).toEqual({ dateRangeInvalid: true });
  });

  it('should return null for valid date range', () => {
    component.purchaseForm.get('startDate')?.setValue('2025-05-01');
    component.purchaseForm.get('endDate')?.setValue('2025-06-01');
    const result = component.dateRangeValidator(component.purchaseForm);
    expect(result).toBeNull();
  });

  it('should calculate premium based on age > 60', () => {
    // Travellers over 60 incur a senior citizen markup on the base premium
    component.product = { basePremium: 1000 };
    component.purchaseForm.get('travellerAge')?.setValue('65');
    component.purchaseForm.get('startDate')?.setValue('2025-05-01');
    component.purchaseForm.get('endDate')?.setValue('2025-05-31'); // 30 days travel
    component.calculateEstimatedPremium();
    expect(component.estimatedPremium).toBeGreaterThan(1000);
  });

  it('should not calculate premium without product', () => {
    component.product = null;
    component.calculateEstimatedPremium();
    expect(component.estimatedPremium).toBe(0);
  });

  it('should clear error on closeToast', () => {
    component.error = 'Error';
    component.closeToast();
    expect(component.error).toBe('');
  });

  it('should default kycType to PAN', () => {
    expect(component.purchaseForm.get('kycType')?.value).toBe('PAN');
  });
});
