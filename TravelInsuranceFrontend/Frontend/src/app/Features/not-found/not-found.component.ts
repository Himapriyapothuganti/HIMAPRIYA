import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

@Component({
    selector: 'app-not-found',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './not-found.component.html'
})
export class NotFoundComponent {
    constructor(private router: Router) { }

    goHome() {
        const role = localStorage.getItem('role');
        if (role === 'Admin') this.router.navigate(['/admin/dashboard']);
        else if (role === 'Customer') this.router.navigate(['/customer/dashboard']);
        else if (role === 'Agent') this.router.navigate(['/agent/dashboard']);
        else if (role === 'ClaimsOfficer') this.router.navigate(['/officer/dashboard']);
        else this.router.navigate(['/login']);
    }
}
