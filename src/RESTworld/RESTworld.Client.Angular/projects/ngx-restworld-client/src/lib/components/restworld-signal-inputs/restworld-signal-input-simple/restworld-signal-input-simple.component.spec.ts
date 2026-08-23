import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { PropertyType, SignalFormService, Template } from '@wertzui/ngx-hal-client';

import { RestWorldSignalInputSimpleComponent } from './restworld-signal-input-simple.component';

describe('RestWorldSignalInputSimpleComponent', () => {
  let component: RestWorldSignalInputSimpleComponent;
  let fixture: ComponentFixture<RestWorldSignalInputSimpleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldSignalInputSimpleComponent],
      providers: [provideHttpClient()]
    })
      .compileComponents();

    const signalFormService = TestBed.inject(SignalFormService);
    const template = new Template({
      properties: [{ name: 'name', type: PropertyType.Text, value: 'John Doe' }]
    });
    const { form } = TestBed.runInInjectionContext(() => signalFormService.createSignalFormFromTemplate(template));

    fixture = TestBed.createComponent(RestWorldSignalInputSimpleComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('property', template.properties[0]);
    fixture.componentRef.setInput('field', form.name);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render an input bound to the field value', () => {
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(input).toBeTruthy();
    expect(input.value).toBe('John Doe');
  });
});
