import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MyClaims } from './my-claims';
import { CustomerService } from '../../../Services/customer.service';
import { of } from 'rxjs';

describe('Customer MyClaims', () => {
  let component: MyClaims;
  let fixture: ComponentFixture<MyClaims>;
  let customerServiceSpy: jasmine.SpyObj<CustomerService>;

  beforeEach(async () => {
    customerServiceSpy = jasmine.createSpyObj('CustomerService', ['getMyClaims', 'getMyPolicies', 'submitClaim']);
    customerServiceSpy.getMyClaims.and.returnValue(of([]));
    customerServiceSpy.getMyPolicies.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [MyClaims, HttpClientTestingModule, ReactiveFormsModule],
      providers: [
        { provide: CustomerService, useValue: customerServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MyClaims);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have isLoading true initially', () => {
    expect(component.isLoading).toBeTrue();
  });

  it('should initialize claim form with required controls', () => {
    fixture.detectChanges();
    expect(component.claimForm).toBeDefined();
    expect(component.claimForm.contains('policyId')).toBeTrue();
    expect(component.claimForm.contains('claimType')).toBeTrue();
    expect(component.claimForm.contains('description')).toBeTrue();
    expect(component.claimForm.contains('claimedAmount')).toBeTrue();
  });

  it('should open modal and reset form', () => {
    fixture.detectChanges();
    component.openModal();
    expect(component.isModalOpen).toBeTrue();
    expect(component.selectedFiles).toEqual([]);
  });

  it('should close modal', () => {
    component.isModalOpen = true;
    component.closeModal();
    expect(component.isModalOpen).toBeFalse();
  });

  it('should remove file at given index', () => {
    component.selectedFiles = [new File([''], 'a.pdf'), new File([''], 'b.pdf'), new File([''], 'c.pdf')];
    component.removeFile(1);
    expect(component.selectedFiles.length).toBe(2);
  });

  it('should return correct status class for UnderReview', () => {
    expect(component.getStatusClass('UnderReview')).toContain('bg-blue-100');
  });

  it('should return correct status class for Approved', () => {
    expect(component.getStatusClass('Approved')).toContain('bg-green-100');
  });

  it('should return correct status class for Rejected', () => {
    expect(component.getStatusClass('Rejected')).toContain('bg-red-100');
  });

  it('should clear error and success on closeToast', () => {
    component.error = 'Error';
    component.success = 'Success';
    component.closeToast();
    expect(component.error).toBe('');
    expect(component.success).toBe('');
  });

  it('should have claimTypes defined', () => {
    expect(component.claimTypes.length).toBeGreaterThan(0);
    expect(component.claimTypes).toContain('Medical');
  });
});
