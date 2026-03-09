import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Payment } from './payment';
import { CustomerService } from '../../../Services/customer.service';
import { of } from 'rxjs';

describe('Customer Payment', () => {
  let component: Payment;
  let fixture: ComponentFixture<Payment>;
  let customerServiceSpy: jasmine.SpyObj<CustomerService>;

  beforeEach(async () => {
    customerServiceSpy = jasmine.createSpyObj('CustomerService', ['getPolicyDetails', 'payPremium']);
    customerServiceSpy.getPolicyDetails.and.returnValue(of({ policyNumber: 'P001', status: 'PendingPayment', premium: 500 }));

    await TestBed.configureTestingModule({
      imports: [Payment, HttpClientTestingModule, RouterTestingModule, FormsModule],
      providers: [
        { provide: CustomerService, useValue: customerServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => '1' } } }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Payment);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have isLoading true initially', () => {
    expect(component.isLoading).toBeTrue();
  });

  it('should default paymentMethod to Credit Card', () => {
    expect(component.paymentMethod).toBe('Credit Card');
  });

  it('should have payment options defined', () => {
    expect(component.paymentOptions.length).toBeGreaterThan(0);
    expect(component.paymentOptions).toContain('Credit Card');
    expect(component.paymentOptions).toContain('UPI');
  });

  it('should clear error and success on closeToast', () => {
    component.error = 'Error';
    component.success = 'Done';
    component.closeToast();
    expect(component.error).toBe('');
    expect(component.success).toBe('');
  });

  it('should load policy on init', () => {
    fixture.detectChanges();
    expect(customerServiceSpy.getPolicyDetails).toHaveBeenCalledWith('1');
  });
});
