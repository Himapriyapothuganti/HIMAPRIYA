import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Modal } from './modal.component';

describe('Admin Modal', () => {
  let component: Modal;
  let fixture: ComponentFixture<Modal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Modal]
    }).compileComponents();

    fixture = TestBed.createComponent(Modal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should default isOpen to false', () => {
    expect(component.isOpen).toBeFalse();
  });

  it('should default title to empty string', () => {
    expect(component.title).toBe('');
  });

  it('should accept isOpen input', () => {
    component.isOpen = true;
    fixture.detectChanges();
    expect(component.isOpen).toBeTrue();
  });

  it('should accept title input', () => {
    component.title = 'Test Modal';
    fixture.detectChanges();
    expect(component.title).toBe('Test Modal');
  });

  it('should emit closeModal event', () => {
    spyOn(component.closeModal, 'emit');
    component.closeModal.emit();
    expect(component.closeModal.emit).toHaveBeenCalled();
  });
});
