import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { BrowsePlans } from './browse-plans';
import { CustomerService } from '../../../Services/customer.service';
import { of } from 'rxjs';

describe('Customer BrowsePlans', () => {
  let component: BrowsePlans;
  let fixture: ComponentFixture<BrowsePlans>;
  let customerServiceSpy: jasmine.SpyObj<CustomerService>;

  beforeEach(async () => {
    customerServiceSpy = jasmine.createSpyObj('CustomerService', ['getPolicyProducts']);
    customerServiceSpy.getPolicyProducts.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [BrowsePlans, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: CustomerService, useValue: customerServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BrowsePlans);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have isLoading true initially', () => {
    expect(component.isLoading).toBeTrue();
  });

  it('should default activeFilter to All', () => {
    expect(component.activeFilter).toBe('All');
  });

  it('should apply filter correctly', () => {
    component.products = [
      { policyType: 'Single Trip', status: 'Available' },
      { policyType: 'Multi-Trip', status: 'Available' },
      { policyType: 'Single Trip', status: 'Available' }
    ];

    // Applying 'Single Trip' should only keep matching policy types
    component.applyFilter('Single Trip');
    expect(component.filteredProducts.length).toBe(2);
    expect(component.activeFilter).toBe('Single Trip');
  });

  it('should show all products when filter is All', () => {
    component.products = [
      { policyType: 'Single Trip', status: 'Available' },
      { policyType: 'Multi-Trip', status: 'Available' }
    ];
    component.applyFilter('All');
    expect(component.filteredProducts.length).toBe(2);
  });

  it('should return correct group icon for Single Trip', () => {
    expect(component.getGroupIcon('Single Trip')).toBe('✈️');
  });

  it('should return correct group icon for Family', () => {
    expect(component.getGroupIcon('Family')).toBe('👨‍👩‍👧‍👦');
  });

  it('should return correct tier badge class for Silver', () => {
    expect(component.getTierBadgeClass('Silver')).toContain('bg-gray-100');
  });

  it('should return correct tier badge class for Platinum', () => {
    expect(component.getTierBadgeClass('Platinum')).toContain('text-[#E8584A]');
  });

  it('should parse coverage list from string', () => {
    // The backend sends coverage details as a comma-separated string,
    // we need to split it so the UI can render a bulleted list
    const list = component.getCoverageList('Medical, Travel, Baggage');
    expect(list.length).toBe(3);
    expect(list[0]).toBe('Medical');
  });

  it('should return included items for Silver tier', () => {
    const items = component.getIncludedItems('Silver');
    expect(items.length).toBeGreaterThan(0);
    expect(items).toContain('Medical Expenses & Hospitalization');
  });

  it('should return more items for Gold tier', () => {
    const silverItems = component.getIncludedItems('Silver');
    const goldItems = component.getIncludedItems('Gold');
    expect(goldItems.length).toBeGreaterThan(silverItems.length);
  });

  it('should open details modal', () => {
    const plan = { name: 'Test Plan' };
    component.openDetails(plan);
    expect(component.isModalOpen).toBeTrue();
    expect(component.selectedPlan).toBe(plan);
  });

  it('should close details modal', () => {
    component.isModalOpen = true;
    component.selectedPlan = { name: 'Test' };
    component.closeDetails();
    expect(component.isModalOpen).toBeFalse();
    expect(component.selectedPlan).toBeNull();
  });
});
