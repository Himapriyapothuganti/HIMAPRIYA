import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule } from '@angular/forms';
import { MyPolicies } from './my-policies';
import { AgentService } from '../../../Services/agent.service';
import { of } from 'rxjs';

describe('Agent MyPolicies', () => {
    let component: MyPolicies;
    let fixture: ComponentFixture<MyPolicies>;
    let agentServiceSpy: jasmine.SpyObj<AgentService>;

    beforeEach(async () => {
        agentServiceSpy = jasmine.createSpyObj('AgentService', ['getMyPolicies']);
        agentServiceSpy.getMyPolicies.and.returnValue(of([]));

        await TestBed.configureTestingModule({
            imports: [MyPolicies, HttpClientTestingModule, RouterTestingModule, FormsModule],
            providers: [
                { provide: AgentService, useValue: agentServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(MyPolicies);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default activeTab to All', () => {
        expect(component.activeTab()).toBe('All');
    });

    it('should update activeTab via setTab', () => {
        component.setTab('Active');
        expect(component.activeTab()).toBe('Active');
    });

    it('should filter policies by tab', () => {
        component.policies.set([
            { policyNumber: 'P1', customerName: 'Alice', status: 'Active' },
            { policyNumber: 'P2', customerName: 'Bob', status: 'Expired' }
        ]);
        component.setTab('Active');
        expect(component.filteredPolicies().length).toBe(1);
        expect(component.filteredPolicies()[0].policyNumber).toBe('P1');
    });

    it('should filter policies by search query', () => {
        component.policies.set([
            { policyNumber: 'P1', customerName: 'Alice', status: 'Active' },
            { policyNumber: 'P2', customerName: 'Bob', status: 'Active' }
        ]);
        component.searchQuery.set('bob');
        expect(component.filteredPolicies().length).toBe(1);
    });

    it('should clear error on closeToast', () => {
        component.error.set('Some error');
        component.closeToast();
        expect(component.error()).toBe('');
    });

    it('should return correct badge class for Active status', () => {
        expect(component.getStatusBadgeClass('Active')).toContain('bg-green-100');
    });
});
