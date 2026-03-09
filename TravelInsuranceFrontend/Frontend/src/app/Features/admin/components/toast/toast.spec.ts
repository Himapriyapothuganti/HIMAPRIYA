import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Toast } from './toast';

describe('Admin Toast', () => {
    let component: Toast;
    let fixture: ComponentFixture<Toast>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [Toast]
        }).compileComponents();

        fixture = TestBed.createComponent(Toast);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default message to empty string', () => {
        expect(component.message).toBe('');
    });

    it('should default type to info', () => {
        expect(component.type).toBe('info');
    });

    it('should default duration to 3000', () => {
        expect(component.duration).toBe(3000);
    });

    it('should return correct icon for success type', () => {
        component.type = 'success';
        expect(component.icon).toBe('✅');
    });

    it('should return correct icon for error type', () => {
        component.type = 'error';
        expect(component.icon).toBe('❌');
    });

    it('should return correct icon for info type', () => {
        component.type = 'info';
        expect(component.icon).toBe('ℹ️');
    });

    it('should emit closeToast event on close', fakeAsync(() => {
        spyOn(component.closeToast, 'emit');
        component.close();
        expect(component.isVisible).toBeFalse();
        tick(300);
        expect(component.closeToast.emit).toHaveBeenCalled();
    }));

    it('should include type-specific classes in containerClasses', () => {
        component.type = 'error';
        const classes = component.containerClasses;
        expect(classes).toContain('border-[#E8584A]');
    });
});
