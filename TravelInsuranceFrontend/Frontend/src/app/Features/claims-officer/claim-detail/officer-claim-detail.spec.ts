import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { OfficerClaimDetailComponent } from './officer-claim-detail.component';
import { ClaimsOfficerService } from '../services/claims-officer.service';
import { of } from 'rxjs';

describe('OfficerClaimDetailComponent', () => {
    let component: OfficerClaimDetailComponent;
    let fixture: ComponentFixture<OfficerClaimDetailComponent>;
    let serviceSpy: jasmine.SpyObj<ClaimsOfficerService>;

    beforeEach(async () => {
        serviceSpy = jasmine.createSpyObj('ClaimsOfficerService', ['getAssignedClaims', 'reviewClaim']);
        serviceSpy.getAssignedClaims.and.returnValue(of([]));

        await TestBed.configureTestingModule({
            imports: [OfficerClaimDetailComponent, HttpClientTestingModule, RouterTestingModule, FormsModule],
            providers: [
                { provide: ClaimsOfficerService, useValue: serviceSpy },
                {
                    provide: ActivatedRoute,
                    useValue: { snapshot: { paramMap: { get: () => '1' } } }
                }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(OfficerClaimDetailComponent);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default isLoading to true', () => {
        expect(component.isLoading()).toBeTrue();
    });

    it('should default claim to null', () => {
        expect(component.claim()).toBeNull();
    });

    it('should set activeAction via openAction', () => {
        component.openAction('Approve');
        expect((component as any).activeAction).toBeDefined();
    });

    it('should return a string for getStatusBadgeClass', () => {
        const result = component.getStatusBadgeClass('Approved');
        expect(typeof result).toBe('string');
    });

    it('should return a string for getStatusBadgeClass with Rejected status', () => {
        const result = component.getStatusBadgeClass('Rejected');
        expect(typeof result).toBe('string');
    });
});
