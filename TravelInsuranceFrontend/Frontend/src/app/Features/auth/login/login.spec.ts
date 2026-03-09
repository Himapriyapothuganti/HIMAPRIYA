import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { LoginComponent } from './login.component';
import { AuthService } from '../../../Models/auth.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

describe('LoginComponent', () => {
    let component: LoginComponent;
    let fixture: ComponentFixture<LoginComponent>;
    let authServiceSpy: jasmine.SpyObj<AuthService>;
    let router: Router;

    beforeEach(async () => {
        // Mock AuthService so we don't hit the real backend during unit tests
        authServiceSpy = jasmine.createSpyObj('AuthService', ['login']);
        authServiceSpy.login.and.returnValue(of({ token: 'abc', role: 'Customer', fullName: 'Test User', email: 'test@test.com' }));

        await TestBed.configureTestingModule({
            imports: [LoginComponent, HttpClientTestingModule, RouterTestingModule, ReactiveFormsModule],
            providers: [
                { provide: AuthService, useValue: authServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(LoginComponent);
        component = fixture.componentInstance;
        router = TestBed.inject(Router);
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should initialize loginForm with email and password controls', () => {
        // Ensure ReactiveFormsModule set up the form group correctly on init
        expect(component.loginForm.contains('email')).toBeTrue();
        expect(component.loginForm.contains('password')).toBeTrue();
    });

    it('should default errorMessage to empty string', () => {
        expect(component.errorMessage).toBe('');
    });

    it('should require email', () => {
        const emailControl = component.loginForm.get('email');
        emailControl?.setValue('');
        expect(emailControl?.valid).toBeFalse();
    });

    it('should validate email format', () => {
        const emailControl = component.loginForm.get('email');
        emailControl?.setValue('invalid');
        expect(emailControl?.hasError('email')).toBeTrue();
        emailControl?.setValue('test@example.com');
        expect(emailControl?.valid).toBeTrue();
    });

    it('should require password', () => {
        const passwordControl = component.loginForm.get('password');
        passwordControl?.setValue('');
        expect(passwordControl?.valid).toBeFalse();
    });

    it('should not call login when form is invalid', () => {
        // Prevent submission if validators fail (e.g. empty fields)
        component.loginForm.get('email')?.setValue('');
        component.loginForm.get('password')?.setValue('');
        component.onSubmit();
        expect(authServiceSpy.login).not.toHaveBeenCalled();
    });

    it('should call login when form is valid', () => {
        component.loginForm.get('email')?.setValue('test@example.com');
        component.loginForm.get('password')?.setValue('password123');

        // Spy on router to prevent actual navigation during the test
        spyOn(router, 'navigate');
        component.onSubmit();

        expect(authServiceSpy.login).toHaveBeenCalled();
    });

    it('should set error message on login failure', () => {
        authServiceSpy.login.and.returnValue(throwError(() => ({ error: { message: 'Invalid credentials' } })));
        component.loginForm.get('email')?.setValue('test@example.com');
        component.loginForm.get('password')?.setValue('wrong');
        component.onSubmit();
        expect(component.errorMessage).toBe('Invalid credentials');
    });
});
