import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { RegisterComponent } from './register.component';
import { AuthService } from '../../../Models/auth.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

describe('RegisterComponent', () => {
    let component: RegisterComponent;
    let fixture: ComponentFixture<RegisterComponent>;
    let authServiceSpy: jasmine.SpyObj<AuthService>;
    let router: Router;

    beforeEach(async () => {
        // Mock AuthService so we don't accidentally create real users during test runs
        authServiceSpy = jasmine.createSpyObj('AuthService', ['register']);
        authServiceSpy.register.and.returnValue(of({}));

        await TestBed.configureTestingModule({
            imports: [RegisterComponent, HttpClientTestingModule, RouterTestingModule, ReactiveFormsModule],
            providers: [
                { provide: AuthService, useValue: authServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(RegisterComponent);
        component = fixture.componentInstance;
        router = TestBed.inject(Router);
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should initialize registerForm with required controls', () => {
        expect(component.registerForm.contains('fullName')).toBeTrue();
        expect(component.registerForm.contains('email')).toBeTrue();
        expect(component.registerForm.contains('password')).toBeTrue();
        expect(component.registerForm.contains('confirmPassword')).toBeTrue();
    });

    it('should require fullName', () => {
        const control = component.registerForm.get('fullName');
        control?.setValue('');
        expect(control?.valid).toBeFalse();
        control?.setValue('John Doe');
        expect(control?.valid).toBeTrue();
    });

    it('should validate email format', () => {
        const control = component.registerForm.get('email');
        control?.setValue('invalid-email');
        expect(control?.hasError('email')).toBeTrue();
        control?.setValue('valid@example.com');
        expect(control?.valid).toBeTrue();
    });

    it('should require password with minimum length 6', () => {
        const control = component.registerForm.get('password');
        control?.setValue('12345');
        expect(control?.hasError('minlength')).toBeTrue();
    });

    it('should validate password match', () => {
        // Our custom passwordMatchValidator should trigger 'passwordMismatch' if fields differ
        component.registerForm.get('password')?.setValue('Password1!');
        component.registerForm.get('confirmPassword')?.setValue('Different1!');
        const result = component.passwordMatchValidator(component.registerForm);
        expect(result).toEqual({ passwordMismatch: true });
    });

    it('should return null when passwords match', () => {
        component.registerForm.get('password')?.setValue('Password1!');
        component.registerForm.get('confirmPassword')?.setValue('Password1!');
        const result = component.passwordMatchValidator(component.registerForm);
        expect(result).toBeNull();
    });

    it('should not call register when form is invalid', () => {
        // Form submission should be blocked if any required fields are missing
        component.registerForm.get('fullName')?.setValue('');
        component.onSubmit();
        expect(authServiceSpy.register).not.toHaveBeenCalled();
    });

    it('should set error message on registration failure', () => {
        authServiceSpy.register.and.returnValue(throwError(() => ({
            error: { message: 'Email already exists' }
        })));

        component.registerForm.get('fullName')?.setValue('John');
        component.registerForm.get('email')?.setValue('john@example.com');
        component.registerForm.get('password')?.setValue('Password1!');
        component.registerForm.get('confirmPassword')?.setValue('Password1!');
        component.onSubmit();
        expect(component.errorMessage).toBe('Email already exists');
    });
});
