import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { PolicyDetail } from './policy-detail';
import { AgentService } from '../../../Services/agent.service';

describe('Agent PolicyDetail', () => {
    let component: PolicyDetail;
    let fixture: ComponentFixture<PolicyDetail>;
    let agentServiceSpy: jasmine.SpyObj<AgentService>;

    beforeEach(async () => {
        agentServiceSpy = jasmine.createSpyObj('AgentService', ['getPolicyById']);
        agentServiceSpy.getPolicyById.and.returnValue(of({}));

        await TestBed.configureTestingModule({
            imports: [PolicyDetail, HttpClientTestingModule, RouterTestingModule],
            providers: [
                { provide: AgentService, useValue: agentServiceSpy },
                {
                    provide: ActivatedRoute,
                    useValue: { snapshot: { paramMap: { get: () => '1' } } }
                }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(PolicyDetail);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default isLoading signal to true', () => {
        expect(component.isLoading()).toBeTrue();
    });

    it('should split coverage details into list', () => {
        component.policy.set({ coverageDetails: 'Medical, Travel, Baggage' });
        const list = component.getCoverageList();
        expect(list.length).toBe(3);
        expect(list[0]).toBe('Medical');
    });

    it('should clear error on closeToast', () => {
        component.error.set('Error');
        component.closeToast();
        expect(component.error()).toBe('');
    });

    it('should return correct badge class for Active status', () => {
        expect(component.getStatusBadgeClass('Active')).toContain('bg-green-100');
    });

    it('should return correct badge class for Cancelled status', () => {
        expect(component.getStatusBadgeClass('Cancelled')).toContain('bg-red-100');
    });
});
