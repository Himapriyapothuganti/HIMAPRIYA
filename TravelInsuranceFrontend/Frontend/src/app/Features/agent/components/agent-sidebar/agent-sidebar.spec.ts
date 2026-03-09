import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { AgentSidebar } from './agent-sidebar.component';
import { AuthService } from '../../../../Models/auth.service';

describe('AgentSidebar', () => {
    let component: AgentSidebar;
    let fixture: ComponentFixture<AgentSidebar>;
    let authServiceSpy: jasmine.SpyObj<AuthService>;
    let router: Router;

    beforeEach(async () => {
        authServiceSpy = jasmine.createSpyObj('AuthService', ['logout']);
        spyOn(localStorage, 'getItem').and.returnValue(null);

        await TestBed.configureTestingModule({
            imports: [AgentSidebar, RouterTestingModule],
            providers: [
                { provide: AuthService, useValue: authServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AgentSidebar);
        component = fixture.componentInstance;
        router = TestBed.inject(Router);
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default userName to User', () => {
        expect(component.userName).toBe('User');
    });

    it('should default userNameInitials to U', () => {
        expect(component.userNameInitials).toBe('U');
    });

    it('should call AuthService.logout and navigate on logout', () => {
        spyOn(router, 'navigate');
        component.logout();
        expect(authServiceSpy.logout).toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });
});
