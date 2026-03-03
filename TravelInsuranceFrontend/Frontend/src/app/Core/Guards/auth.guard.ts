import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
    const router = inject(Router);
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');

    // Expected role can be passed via route data (e.g., data: { role: 'Admin' })
    const expectedRole = route.data?.['role'];

    // 1. Not logged in
    if (!token) {
        router.navigate(['/login']);
        return false;
    }

    // 2. Logged in, but route requires a specific role that doesn't match
    if (expectedRole && role !== expectedRole) {
        // Decide fallback depending on actual role
        if (role === 'Admin') {
            router.navigate(['/admin/dashboard']);
        } else if (role === 'Customer') {
            router.navigate(['/customer/dashboard']);
        } else {
            router.navigate(['/login']);
        }
        return false;
    }

    // 3. Authorized
    return true;
};

