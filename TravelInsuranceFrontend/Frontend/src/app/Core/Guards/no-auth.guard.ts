import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

// Prevents logged-in users from going back to /login or /register via the back button
export const noAuthGuard: CanActivateFn = () => {
    const router = inject(Router);
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');

    if (token && role) {
        // Already logged in — redirect to their own dashboard
        if (role === 'Admin') {
            router.navigate(['/admin/dashboard']);
        } else if (role === 'Customer') {
            router.navigate(['/customer/dashboard']);
        } else if (role === 'Agent') {
            router.navigate(['/agent/dashboard']);
        } else if (role === 'ClaimsOfficer') {
            router.navigate(['/officer/dashboard']);
        }
        return false;
    }

    // Not logged in — allow access to login/register
    return true;
};
