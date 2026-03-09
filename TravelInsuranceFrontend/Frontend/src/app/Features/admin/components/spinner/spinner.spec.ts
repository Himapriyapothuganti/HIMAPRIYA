import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Spinner } from './spinner';

describe('Admin Spinner', () => {
    let component: Spinner;
    let fixture: ComponentFixture<Spinner>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [Spinner]
        }).compileComponents();

        fixture = TestBed.createComponent(Spinner);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default isLoading to false', () => {
        expect(component.isLoading).toBeFalse();
    });

    it('should accept isLoading input as true', () => {
        component.isLoading = true;
        fixture.detectChanges();
        expect(component.isLoading).toBeTrue();
    });
});
