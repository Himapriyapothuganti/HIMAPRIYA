import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BrowsePlans } from './browse-plans';

describe('BrowsePlans', () => {
  let component: BrowsePlans;
  let fixture: ComponentFixture<BrowsePlans>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BrowsePlans]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BrowsePlans);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
