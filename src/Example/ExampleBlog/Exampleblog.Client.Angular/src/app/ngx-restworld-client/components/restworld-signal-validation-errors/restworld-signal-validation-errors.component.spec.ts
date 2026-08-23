import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { form, required } from '@angular/forms/signals';

import { RestWorldSignalValidationErrorsComponent } from './restworld-signal-validation-errors.component';

describe('RestWorldSignalValidationErrorsComponent', () => {
  let component: RestWorldSignalValidationErrorsComponent<string>;
  let fixture: ComponentFixture<RestWorldSignalValidationErrorsComponent<string>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldSignalValidationErrorsComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(RestWorldSignalValidationErrorsComponent<string>);
    component = fixture.componentInstance;

    const testForm = TestBed.runInInjectionContext(() => {
      const model = signal({ name: '' });
      return form(model, path => {
        required(path.name, { message: 'Name is required' });
      });
    });

    fixture.componentRef.setInput('field', testForm.name);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not show errors before the field is touched', () => {
    expect(component.showErrors()).toBe(false);
  });

  it('should show errors after the field is touched and invalid', () => {
    component.field()().markAsTouched();
    fixture.detectChanges();

    expect(component.showErrors()).toBe(true);
    expect(component.errors().map(e => e.kind)).toContain('required');
  });
});
