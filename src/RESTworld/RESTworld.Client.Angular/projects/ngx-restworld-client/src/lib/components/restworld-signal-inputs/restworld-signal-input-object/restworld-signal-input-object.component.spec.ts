import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { PropertyType, SignalFormService, Template } from '@wertzui/ngx-hal-client';

import { RestWorldSignalInputObjectComponent } from './restworld-signal-input-object.component';

describe('RestWorldSignalInputObjectComponent', () => {
  let component: RestWorldSignalInputObjectComponent<any>;
  let fixture: ComponentFixture<RestWorldSignalInputObjectComponent<any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldSignalInputObjectComponent],
      providers: [provideHttpClient(), MessageService]
    })
      .compileComponents();

    const signalFormService = TestBed.inject(SignalFormService);
    const template = new Template({
      properties: [
        {
          name: 'address',
          type: PropertyType.Object,
          value: undefined,
          _templates: {
            default: {
              properties: [
                { name: 'street', type: PropertyType.Text, value: 'Main Street' },
                { name: 'zip', type: PropertyType.Number, value: 12345 }
              ]
            }
          }
        }
      ]
    });
    const { form } = TestBed.runInInjectionContext(() => signalFormService.createSignalFormFromTemplate(template));

    fixture = TestBed.createComponent(RestWorldSignalInputObjectComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('property', template.properties[0]);
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('field', (form as any).address);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the nested template properties as inputs bound to their values', () => {
    const inputs = fixture.nativeElement.querySelectorAll('input') as NodeListOf<HTMLInputElement>;
    const values = Array.from(inputs).map(i => i.value);
    expect(values).toContain('Main Street');
    // p-inputNumber renders the number using locale formatting (e.g. "12.345" or "12,345" depending on locale),
    // so compare after stripping non-digit characters rather than the exact template value.
    expect(values.some(v => v.replace(/\D/g, '') === '12345')).toBe(true);
  });
});
