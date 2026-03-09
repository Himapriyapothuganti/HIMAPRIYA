import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { OfficerSidebarComponent } from './officer-sidebar.component';
import { AuthService } from '../../../../Models/auth.service';

describe('OfficerSidebarComponent', () => {
    let component: OfficerSidebarComponent;
    let fixture: ComponentFixture<OfficerSidebarComponent>;
    let authServiceSpy: jasmine.SpyObj<AuthService>;
    let router: Router;

    beforeEach(async () => {
        authServiceSpy = jasmine.createSpyObj('AuthService', ['logout']);
        spyOn(localStorage, 'getItem').and.returnValue(null);

        await TestBed.configureTestingModule({
            imports: [OfficerSidebarComponent, RouterTestingModule],
            providers: [
                { provide: AuthService, useValue: authServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(OfficerSidebarComponent);
        component = fixture.componentInstance;
        router = TestBed.inject(Router);
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default userName signal to Officer', () => {
        expect(component.userName()).toBe('Officer');
    });

    it('should compute userNameInitials from userName', () => {
        expect(component.userNameInitials()).toBe('O');
    });

    it('should call AuthService.logout and navigate on logout', () => {
        spyOn(router, 'navigate');
        component.logout();
        expect(authServiceSpy.logout).toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });

    it('should read fullName from localStorage', () => {
        (localStorage.getItem as jasmine.Spy).and.returnValue('Jane Officer');
        const newFixture = TestBed.createComponent(OfficerSidebarComponent);
        const newComponent = newFixture.componentInstance;
        expect(newComponent.userName()).toBe('Jane Officer');
        expect(newComponent.userNameInitials()).toBe('J');
    });
});
