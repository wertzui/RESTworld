import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { PropertyType, SignalFormService, Template } from '@wertzui/ngx-hal-client';

import { RestWorldSignalInputTemplateComponent } from './restworld-signal-input-template.component';

describe('RestWorldSignalInputTemplateComponent', () => {
  let component: RestWorldSignalInputTemplateComponent;
  let fixture: ComponentFixture<RestWorldSignalInputTemplateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldSignalInputTemplateComponent],
      providers: [provideHttpClient(), MessageService]
    })
      .compileComponents();

    const signalFormService = TestBed.inject(SignalFormService);
    const template = new Template({
      properties: [
        { name: 'name', type: PropertyType.Text, value: 'John Doe' },
        { name: 'age', type: PropertyType.Number, value: 42 }
      ]
    });
    const { form } = TestBed.runInInjectionContext(() => signalFormService.createSignalFormFromTemplate(template));

    fixture = TestBed.createComponent(RestWorldSignalInputTemplateComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('template', template);
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('field', form);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render one rw-signal-form-element per property', () => {
    const elements = fixture.nativeElement.querySelectorAll('rw-signal-form-element');
    expect(elements.length).toBe(2);
  });

  it('should bind each field to the correct value', () => {
    const inputs = fixture.nativeElement.querySelectorAll('input') as NodeListOf<HTMLInputElement>;
    const values = Array.from(inputs).map(i => i.value);
    expect(values).toContain('John Doe');
    expect(values).toContain('42');
  });
});
