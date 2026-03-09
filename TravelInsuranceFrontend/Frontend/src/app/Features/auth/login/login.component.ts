import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../Models/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
    loginForm: FormGroup;
    errorMessage: string = '';
    captchaNum1: number = 0;
    captchaNum2: number = 0;
    captchaError: boolean = false;

    constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
        this.loginForm = this.fb.group({
            email: ['', [Validators.required, Validators.email]],
            password: ['', Validators.required],
            captcha: ['', Validators.required]
        });
    }

    ngOnInit() {
        this.generateCaptcha();
    }

    generateCaptcha() {
        this.captchaNum1 = Math.floor(Math.random() * 10) + 1;
        this.captchaNum2 = Math.floor(Math.random() * 10) + 1;
        this.loginForm.patchValue({ captcha: '' });
        this.captchaError = false;
    }

    onSubmit() {
        if (this.loginForm.valid) {
            const captchaInput = parseInt(this.loginForm.get('captcha')?.value, 10);
            if (captchaInput !== (this.captchaNum1 + this.captchaNum2)) {
                this.captchaError = true;
                this.generateCaptcha();
                return;
            }
            this.captchaError = false;

            this.authService.login(this.loginForm.value).subscribe({
                next: (res: any) => {
                    localStorage.setItem('token', res.token);
                    localStorage.setItem('role', res.role);
                    localStorage.setItem('fullName', res.fullName);
                    localStorage.setItem('email', res.email);
                    if (res.userId) {
                        localStorage.setItem('userId', res.userId);
                    }

                    if (res.role === 'Admin') {
                        this.router.navigate(['/admin/dashboard']);
                    } else if (res.role === 'Customer') {
                        this.router.navigate(['/customer/dashboard']);
                    } else if (res.role === 'Agent') {
                        this.router.navigate(['/agent/dashboard']);
                    } else if (res.role === 'ClaimsOfficer') {
                        this.router.navigate(['/officer/dashboard']);
                    } else {
                        this.router.navigate(['/']); // Redirect to landing page on successful login
                    }
                },
                error: (err) => {
                    this.errorMessage = err.error?.message || 'Login failed. Please check your credentials.';
                }
            });
        }
    }
}
