import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { PropertyType, SignalFormService, Template } from '@wertzui/ngx-hal-client';

import { RestWorldSignalInputComponent } from './restworld-signal-input.component';

describe('RestWorldSignalInputComponent', () => {
  let component: RestWorldSignalInputComponent<any>;
  let fixture: ComponentFixture<RestWorldSignalInputComponent<any>>;

  async function createWithTemplate(template: Template) {
    const signalFormService = TestBed.inject(SignalFormService);
    const { form } = TestBed.runInInjectionContext(() => signalFormService.createSignalFormFromTemplate(template));

    fixture = TestBed.createComponent(RestWorldSignalInputComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('property', template.properties[0]);
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('field', (form as any)[template.properties[0].name]);
    fixture.detectChanges();
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldSignalInputComponent],
      providers: [provideHttpClient(), MessageService]
    })
      .compileComponents();
  });

  it('should create and render a simple text input for a plain property', async () => {
    const template = new Template({ properties: [{ name: 'name', type: PropertyType.Text, value: 'John Doe' }] });
    await createWithTemplate(template);

    expect(component).toBeTruthy();
    const input = fixture.nativeElement.querySelector('input');
    expect(input).toBeTruthy();
    expect(input.value).toBe('John Doe');
  });

  it('should render a dropdown for a property with options', async () => {
    const template = new Template({
      properties: [{
        name: 'category',
        type: PropertyType.Text,
        value: 1,
        options: { inline: [{ prompt: 'One', value: 1 }], selectedValues: [1] }
      }]
    });
    await createWithTemplate(template);

    expect(fixture.nativeElement.querySelector('p-select')).toBeTruthy();
  });
});
