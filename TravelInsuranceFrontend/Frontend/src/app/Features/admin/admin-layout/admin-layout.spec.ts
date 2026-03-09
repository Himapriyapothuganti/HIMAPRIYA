import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterModule } from '@angular/router';
import { AdminLayout } from './admin-layout';

describe('AdminLayout', () => {
    let component: AdminLayout;
    let fixture: ComponentFixture<AdminLayout>;

    beforeEach(async () => {
        spyOn(localStorage, 'getItem').and.returnValue(null);

        await TestBed.configureTestingModule({
            imports: [AdminLayout, RouterModule.forRoot([])]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminLayout);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default adminName to Admin', () => {
        expect(component.adminName).toBe('Admin');
    });

    it('should set adminName from localStorage on init', () => {
        (localStorage.getItem as jasmine.Spy).and.returnValue('John Doe');
        component.ngOnInit();
        expect(component.adminName).toBe('John Doe');
    });
});
