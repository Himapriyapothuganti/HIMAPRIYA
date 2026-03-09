import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OfficerNavbarComponent } from './officer-navbar.component';

describe('OfficerNavbarComponent', () => {
    let component: OfficerNavbarComponent;
    let fixture: ComponentFixture<OfficerNavbarComponent>;

    beforeEach(async () => {
        spyOn(localStorage, 'getItem').and.returnValue(null);

        await TestBed.configureTestingModule({
            imports: [OfficerNavbarComponent]
        }).compileComponents();

        fixture = TestBed.createComponent(OfficerNavbarComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default title signal to Claims Officer Portal', () => {
        expect(component.title()).toBe('Claims Officer Portal');
    });

    it('should default userName signal to Officer', () => {
        expect(component.userName()).toBe('Officer');
    });

    it('should default email signal to officer@example.com', () => {
        expect(component.email()).toBe('officer@example.com');
    });

    it('should compute userNameInitials from userName', () => {
        expect(component.userNameInitials()).toBe('O');
    });

    it('should update title via pageTitle input setter', () => {
        component.pageTitle = 'New Title';
        expect(component.title()).toBe('New Title');
    });

    it('should read fullName from localStorage', () => {
        (localStorage.getItem as jasmine.Spy).and.callFake((key: string) => {
            if (key === 'fullName') return 'Jane Officer';
            if (key === 'email') return 'jane@officer.com';
            return null;
        });

        const newFixture = TestBed.createComponent(OfficerNavbarComponent);
        const newComponent = newFixture.componentInstance;
        expect(newComponent.userName()).toBe('Jane Officer');
        expect(newComponent.email()).toBe('jane@officer.com');
        expect(newComponent.userNameInitials()).toBe('J');
    });
});
