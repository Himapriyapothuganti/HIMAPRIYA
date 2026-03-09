import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { Sidebar } from './sidebar';

describe('Admin Sidebar', () => {
    let component: Sidebar;
    let fixture: ComponentFixture<Sidebar>;
    let router: Router;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [Sidebar, RouterTestingModule]
        }).compileComponents();

        fixture = TestBed.createComponent(Sidebar);
        component = fixture.componentInstance;
        router = TestBed.inject(Router);
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should navigate to /login and clear localStorage on logout', () => {
        spyOn(localStorage, 'clear');
        spyOn(router, 'navigate');
        component.onLogout();
        expect(localStorage.clear).toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });
});
