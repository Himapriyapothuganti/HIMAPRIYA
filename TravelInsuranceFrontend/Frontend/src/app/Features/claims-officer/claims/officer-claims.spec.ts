import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule } from '@angular/forms';
import { OfficerClaimsComponent } from './officer-claims.component';
import { ClaimsOfficerService } from '../services/claims-officer.service';
import { of } from 'rxjs';

describe('OfficerClaimsComponent', () => {
    let component: OfficerClaimsComponent;
    let fixture: ComponentFixture<OfficerClaimsComponent>;
    let serviceSpy: jasmine.SpyObj<ClaimsOfficerService>;

    beforeEach(async () => {
        serviceSpy = jasmine.createSpyObj('ClaimsOfficerService', ['getAssignedClaims']);
        serviceSpy.getAssignedClaims.and.returnValue(of([]));

        await TestBed.configureTestingModule({
            imports: [OfficerClaimsComponent, HttpClientTestingModule, RouterTestingModule, FormsModule],
            providers: [
                { provide: ClaimsOfficerService, useValue: serviceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(OfficerClaimsComponent);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default isLoading to true', () => {
        expect(component.isLoading()).toBeTrue();
    });

    it('should default activeTab to All', () => {
        expect(component.activeTab()).toBe('All');
    });

    it('should default searchQuery to empty string', () => {
        expect(component.searchQuery()).toBe('');
    });

    it('should have filter tabs defined', () => {
        expect(component.filterTabs).toBeDefined();
        expect(component.filterTabs.length).toBeGreaterThan(0);
    });

    it('should load claims on init', () => {
        const mockClaims = [
            { status: 'UnderReview', customerName: 'Alice', policyNumber: 'P1' }
        ];
        serviceSpy.getAssignedClaims.and.returnValue(of(mockClaims));
        fixture.detectChanges();
        expect(component.allClaims().length).toBe(1);
        expect(component.isLoading()).toBeFalse();
    });

    it('should return a string for getStatusBadgeClass', () => {
        const result = component.getStatusBadgeClass('UnderReview');
        expect(typeof result).toBe('string');
    });
});
