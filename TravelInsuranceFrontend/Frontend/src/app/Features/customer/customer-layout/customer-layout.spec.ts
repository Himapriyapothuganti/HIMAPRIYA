import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { CustomerLayoutComponent } from './customer-layout.component';
import { AuthService } from '../../../Models/auth.service';

describe('CustomerLayoutComponent', () => {
  let component: CustomerLayoutComponent;
  let fixture: ComponentFixture<CustomerLayoutComponent>;

  beforeEach(async () => {
    spyOn(localStorage, 'getItem').and.returnValue(null);
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['logout']);

    await TestBed.configureTestingModule({
      imports: [CustomerLayoutComponent, RouterTestingModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomerLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should default pageTitle to Dashboard', () => {
    expect(component.pageTitle).toBeDefined();
  });

  it('should default userName to Customer', () => {
    expect(component.userName).toBe('Customer');
  });

  it('should default userNameInitials to C', () => {
    expect(component.userNameInitials).toBe('C');
  });

  it('should set userName from localStorage', () => {
    (localStorage.getItem as jasmine.Spy).and.returnValue('Jane Doe');
    const newFixture = TestBed.createComponent(CustomerLayoutComponent);
    const newComponent = newFixture.componentInstance;
    expect(newComponent.userName).toBe('Jane Doe');
    expect(newComponent.userNameInitials).toBe('J');
  });

  it('should set title to Browse Plans for browse-plans URL', () => {
    (component as any).updateTitleByUrl('/customer/browse-plans');
    expect(component.pageTitle).toBe('Browse Plans');
  });

  it('should set title to My Policies for my-policies URL', () => {
    (component as any).updateTitleByUrl('/customer/my-policies');
    expect(component.pageTitle).toBe('My Policies');
  });

  it('should set title to My Claims for my-claims URL', () => {
    (component as any).updateTitleByUrl('/customer/my-claims');
    expect(component.pageTitle).toBe('My Claims');
  });

  it('should set title to Customer Portal for unknown URL', () => {
    (component as any).updateTitleByUrl('/customer/unknown');
    expect(component.pageTitle).toBe('Customer Portal');
  });
});
