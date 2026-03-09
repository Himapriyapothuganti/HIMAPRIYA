import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { OfficerDashboardComponent } from './officer-dashboard.component';
import { ClaimsOfficerService } from '../services/claims-officer.service';
import { of, throwError } from 'rxjs';

describe('OfficerDashboardComponent', () => {
    let component: OfficerDashboardComponent;
    let fixture: ComponentFixture<OfficerDashboardComponent>;
    let serviceSpy: jasmine.SpyObj<ClaimsOfficerService>;

    beforeEach(async () => {
        // Ensure we isolate testing by mocking the claims officer service
        serviceSpy = jasmine.createSpyObj('ClaimsOfficerService', ['getAssignedClaims']);
        serviceSpy.getAssignedClaims.and.returnValue(of([]));

        await TestBed.configureTestingModule({
            imports: [OfficerDashboardComponent, HttpClientTestingModule, RouterTestingModule],
            providers: [
                { provide: ClaimsOfficerService, useValue: serviceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(OfficerDashboardComponent);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default isLoading signal to true', () => {
        expect(component.isLoading()).toBeTrue();
    });

    it('should compute totalClaims as 0 initially', () => {
        expect(component.totalClaims()).toBe(0);
    });

    it('should load claims on init', () => {
        // The component should fetch assigned claims automatically when it mounts
        const mockClaims = [
            { status: 'UnderReview', submittedDate: '2024-01-01' },
            { status: 'Approved', submittedDate: '2024-01-02' }
        ];
        serviceSpy.getAssignedClaims.and.returnValue(of(mockClaims));
        fixture.detectChanges();
        expect(component.allClaims().length).toBe(2);
        expect(component.isLoading()).toBeFalse();
    });

    it('should return correct badge class for UnderReview', () => {
        expect(component.getStatusBadgeClass('UnderReview')).toBeDefined();
    });

    it('should return correct badge class for Approved', () => {
        expect(component.getStatusBadgeClass('Approved')).toBeDefined();
    });
});
