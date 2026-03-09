import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Dashboard } from './dashboard';
import { AdminService } from '../../../Services/admin.service';
import { of, throwError } from 'rxjs';

describe('Admin Dashboard', () => {
    let component: Dashboard;
    let fixture: ComponentFixture<Dashboard>;
    let adminServiceSpy: jasmine.SpyObj<AdminService>;

    beforeEach(async () => {
        adminServiceSpy = jasmine.createSpyObj('AdminService', ['getDashboardStats']);
        adminServiceSpy.getDashboardStats.and.returnValue(of({ totalUsers: 10, totalPolicies: 5 }));

        await TestBed.configureTestingModule({
            imports: [Dashboard, HttpClientTestingModule],
            providers: [
                { provide: AdminService, useValue: adminServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Dashboard);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have isLoading true initially', () => {
        expect(component.isLoading).toBeTrue();
    });

    it('should have empty error initially', () => {
        expect(component.error).toBe('');
    });

    it('should load dashboard data on init', () => {
        fixture.detectChanges();
        expect(adminServiceSpy.getDashboardStats).toHaveBeenCalled();
        expect(component.stats).toEqual({ totalUsers: 10, totalPolicies: 5 });
        expect(component.isLoading).toBeFalse();
    });

    it('should set error on service failure', () => {
        adminServiceSpy.getDashboardStats.and.returnValue(throwError(() => new Error('fail')));
        fixture.detectChanges();
        expect(component.error).toBe('Failed to load dashboard data.');
        expect(component.isLoading).toBeFalse();
    });

    it('should clear error on closeToast', () => {
        component.error = 'Some error';
        component.closeToast();
        expect(component.error).toBe('');
    });
});
