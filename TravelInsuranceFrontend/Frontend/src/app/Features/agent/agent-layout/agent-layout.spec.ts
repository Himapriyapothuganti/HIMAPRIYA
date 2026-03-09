import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { AgentLayout } from './agent-layout';

describe('AgentLayout', () => {
    let component: AgentLayout;
    let fixture: ComponentFixture<AgentLayout>;
    let router: Router;

    beforeEach(async () => {
        spyOn(localStorage, 'getItem').and.returnValue(null);

        await TestBed.configureTestingModule({
            imports: [AgentLayout, RouterTestingModule]
        }).compileComponents();

        fixture = TestBed.createComponent(AgentLayout);
        component = fixture.componentInstance;
        router = TestBed.inject(Router);
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default pageTitle to Agent Portal', () => {
        expect(component.pageTitle).toBe('Agent Portal');
    });

    it('should set title to Dashboard for dashboard URL', () => {
        (component as any).updateTitleByUrl('/agent/dashboard');
        expect(component.pageTitle).toBe('Dashboard');
    });

    it('should set title to Assigned Policies for my-policies URL', () => {
        (component as any).updateTitleByUrl('/agent/my-policies');
        expect(component.pageTitle).toBe('Assigned Policies');
    });

    it('should set title to Policy Details for policy detail URL', () => {
        (component as any).updateTitleByUrl('/agent/my-policies/123');
        expect(component.pageTitle).toBe('Policy Details');
    });

    it('should set title to Agent Portal for unknown URL', () => {
        (component as any).updateTitleByUrl('/agent/unknown');
        expect(component.pageTitle).toBe('Agent Portal');
    });
});
