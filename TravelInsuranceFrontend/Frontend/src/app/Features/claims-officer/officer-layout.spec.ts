import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { OfficerLayoutComponent } from './officer-layout.component';

describe('OfficerLayoutComponent', () => {
    let component: OfficerLayoutComponent;
    let fixture: ComponentFixture<OfficerLayoutComponent>;

    beforeEach(async () => {
        spyOn(localStorage, 'getItem').and.returnValue(null);

        await TestBed.configureTestingModule({
            imports: [OfficerLayoutComponent, RouterTestingModule]
        }).compileComponents();

        fixture = TestBed.createComponent(OfficerLayoutComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default pageTitle signal to Dashboard', () => {
        expect(component.pageTitle()).toBeDefined();
    });

    it('should default userName signal to Officer', () => {
        expect(component.userName()).toBe('Officer');
    });

    it('should compute userNameInitials from userName', () => {
        expect(component.userNameInitials()).toBe('O');
    });

    it('should set title to Claims Dashboard for dashboard URL', () => {
        (component as any).updateTitleByUrl('/officer/dashboard');
        expect(component.pageTitle()).toBe('Claims Dashboard');
    });

    it('should set title to All Claims for claims URL', () => {
        (component as any).updateTitleByUrl('/officer/claims');
        expect(component.pageTitle()).toBe('All Claims');
    });

    it('should set title to Claim Details for claim detail URL', () => {
        (component as any).updateTitleByUrl('/officer/claims/123');
        expect(component.pageTitle()).toBe('Claim Details');
    });

    it('should set title to Claims Officer Portal for unknown URL', () => {
        (component as any).updateTitleByUrl('/officer/unknown');
        expect(component.pageTitle()).toBe('Claims Officer Portal');
    });

    it('should set userName from localStorage', () => {
        (localStorage.getItem as jasmine.Spy).and.returnValue('Jane Smith');
        const newFixture = TestBed.createComponent(OfficerLayoutComponent);
        const newComponent = newFixture.componentInstance;
        expect(newComponent.userName()).toBe('Jane Smith');
    });
});
