import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Dashboard } from './dashboard';
import { AgentService } from '../../../Services/agent.service';
import { of, throwError } from 'rxjs';

describe('Agent Dashboard', () => {
    let component: Dashboard;
    let fixture: ComponentFixture<Dashboard>;
    let agentServiceSpy: jasmine.SpyObj<AgentService>;

    beforeEach(async () => {
        // Mock the dashboard stats so we don't rely on the live database
        agentServiceSpy = jasmine.createSpyObj('AgentService', ['getDashboard']);
        agentServiceSpy.getDashboard.and.returnValue(of({
            totalPoliciesAssigned: 5,
            activePolicies: 3,
            pendingPaymentPolicies: 1,
            expiredPolicies: 1,
            totalPremiumCollected: 10000,
            totalCommissionEarned: 500,
            assignedPolicies: []
        }));

        await TestBed.configureTestingModule({
            imports: [Dashboard, HttpClientTestingModule, RouterTestingModule],
            providers: [
                { provide: AgentService, useValue: agentServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Dashboard);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default isLoading signal to true', () => {
        expect(component.isLoading()).toBeTrue();
    });

    it('should default error signal to empty string', () => {
        expect(component.error()).toBe('');
    });

    it('should load dashboard data on init', () => {
        fixture.detectChanges();
        expect(agentServiceSpy.getDashboard).toHaveBeenCalled();
        expect(component.isLoading()).toBeFalse();
    });

    it('should set error on service failure', () => {
        // If the backend call fails, the UI should display the error gracefully
        agentServiceSpy.getDashboard.and.returnValue(throwError(() => new Error('fail')));
        fixture.detectChanges();
        expect(component.error()).toContain('Failed to load dashboard data');
        expect(component.isLoading()).toBeFalse();
    });

    it('should clear error on closeToast', () => {
        component.error.set('Some error');
        component.closeToast();
        expect(component.error()).toBe('');
    });

    it('should return correct badge class for Active status', () => {
        expect(component.getStatusBadgeClass('Active')).toContain('bg-green-100');
    });

    it('should return correct badge class for PendingPayment status', () => {
        expect(component.getStatusBadgeClass('PendingPayment')).toContain('bg-yellow-100');
    });

    it('should return correct badge class for Expired status', () => {
        expect(component.getStatusBadgeClass('Expired')).toContain('bg-gray-100');
    });

    it('should return default badge class for unknown status', () => {
        expect(component.getStatusBadgeClass('Unknown')).toContain('bg-gray-100');
    });
});
