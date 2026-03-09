import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CustomerNavbar } from './navbar.component';

describe('CustomerNavbar', () => {
  let component: CustomerNavbar;
  let fixture: ComponentFixture<CustomerNavbar>;

  beforeEach(async () => {
    spyOn(localStorage, 'getItem').and.returnValue(null);

    await TestBed.configureTestingModule({
      imports: [CustomerNavbar]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomerNavbar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should default title to Welcome', () => {
    expect(component.title).toBe('Welcome');
  });

  it('should default userName to User', () => {
    expect(component.userName).toBe('User');
  });

  it('should default email to user@example.com', () => {
    expect(component.email).toBe('user@example.com');
  });

  it('should default userNameInitials to U', () => {
    expect(component.userNameInitials).toBe('U');
  });

  it('should accept title input', () => {
    component.title = 'My Dashboard';
    fixture.detectChanges();
    expect(component.title).toBe('My Dashboard');
  });

  it('should read fullName from localStorage in constructor', () => {
    (localStorage.getItem as jasmine.Spy).and.callFake((key: string) => {
      if (key === 'fullName') return 'Alice Smith';
      if (key === 'email') return 'alice@test.com';
      return null;
    });

    const newFixture = TestBed.createComponent(CustomerNavbar);
    const newComponent = newFixture.componentInstance;
    expect(newComponent.userName).toBe('Alice Smith');
    expect(newComponent.email).toBe('alice@test.com');
    expect(newComponent.userNameInitials).toBe('A');
  });
});
