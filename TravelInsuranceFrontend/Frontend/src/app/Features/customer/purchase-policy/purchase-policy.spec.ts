import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PurchasePolicy } from './purchase-policy';

describe('PurchasePolicy', () => {
  let component: PurchasePolicy;
  let fixture: ComponentFixture<PurchasePolicy>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PurchasePolicy]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PurchasePolicy);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
