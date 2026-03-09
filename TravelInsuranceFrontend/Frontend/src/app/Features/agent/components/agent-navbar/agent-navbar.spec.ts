import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AgentNavbar } from './agent-navbar.component';

describe('AgentNavbar', () => {
    let component: AgentNavbar;
    let fixture: ComponentFixture<AgentNavbar>;

    beforeEach(async () => {
        spyOn(localStorage, 'getItem').and.returnValue(null);

        await TestBed.configureTestingModule({
            imports: [AgentNavbar]
        }).compileComponents();

        fixture = TestBed.createComponent(AgentNavbar);
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
        component.title = 'Dashboard';
        fixture.detectChanges();
        expect(component.title).toBe('Dashboard');
    });

    it('should read fullName from localStorage in constructor', () => {
        (localStorage.getItem as jasmine.Spy).and.callFake((key: string) => {
            if (key === 'fullName') return 'John Doe';
            if (key === 'email') return 'john@test.com';
            return null;
        });

        // Re-create to trigger constructor
        fixture = TestBed.createComponent(AgentNavbar);
        component = fixture.componentInstance;
        expect(component.userName).toBe('John Doe');
        expect(component.email).toBe('john@test.com');
        expect(component.userNameInitials).toBe('J');
    });
});
