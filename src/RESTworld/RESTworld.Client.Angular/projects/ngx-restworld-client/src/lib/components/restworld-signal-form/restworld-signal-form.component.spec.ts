import { HttpHeaders, HttpResponse, provideHttpClient } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ProblemDetails, PropertyType, Template } from '@wertzui/ngx-hal-client';
import type { PropertyDto, SimpleValue } from '@wertzui/ngx-hal-client';

import { RestWorldSignalFormComponent } from './restworld-signal-form.component';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';

Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => { },
    removeListener: () => { },
    addEventListener: () => { },
    removeEventListener: () => { },
    dispatchEvent: () => false
  })
});

describe('RestWorldSignalFormComponent', () => {
  let component: RestWorldSignalFormComponent<ReadonlyArray<PropertyDto<SimpleValue, string, string>>>;
  let fixture: ComponentFixture<RestWorldSignalFormComponent<ReadonlyArray<PropertyDto<SimpleValue, string, string>>>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldSignalFormComponent],
      providers: [provideHttpClient(), MessageService, ConfirmationService]
    })
      .compileComponents();

    fixture = TestBed.createComponent(RestWorldSignalFormComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('rel', 'test:rel');
    fixture.componentRef.setInput('template', new Template({
      properties: [
        { name: 'name', type: PropertyType.Text, value: 'John Doe' },
        { name: 'age', type: PropertyType.Number, value: 42 }
      ]
    }));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the template properties as inputs', () => {
    const inputs = fixture.nativeElement.querySelectorAll('input') as NodeListOf<HTMLInputElement>;
    const values = Array.from(inputs).map(i => i.value);
    expect(values).toContain('John Doe');
    expect(values).toContain('42');
  });

  it('should render the default Save, Reload and Delete buttons', () => {
    const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
    const labels = Array.from(buttons).map(b => b.textContent?.trim());
    expect(labels.some(l => l?.includes('Save'))).toBe(true);
    expect(labels.some(l => l?.includes('Reload'))).toBe(true);
    expect(labels.some(l => l?.includes('Delete'))).toBe(true);
  });
});

describe('RestWorldSignalFormComponent with a failing submit', () => {
  let component: RestWorldSignalFormComponent<ReadonlyArray<PropertyDto<SimpleValue, string, string>>>;
  let fixture: ComponentFixture<RestWorldSignalFormComponent<ReadonlyArray<PropertyDto<SimpleValue, string, string>>>>;

  beforeEach(async () => {
    const problemDetails = new ProblemDetails({
      _links: { self: [{ href: '' }] },
      detail: 'Please correct the errors below.',
      errors: { 'name': ['The name is already taken.'] }
    });

    const submitSpy = vi.fn().mockResolvedValue(new HttpResponse<ProblemDetails>({
      body: problemDetails,
      status: 400,
      statusText: 'Bad Request',
      headers: new HttpHeaders()
    }));

    const clientCollectionStub = {
      getClient: () => ({ submit: submitSpy })
    };

    await TestBed.configureTestingModule({
      imports: [RestWorldSignalFormComponent],
      providers: [
        provideHttpClient(),
        MessageService,
        ConfirmationService,
        { provide: RestWorldClientCollection, useValue: clientCollectionStub }
      ]
    })
      .compileComponents();

    fixture = TestBed.createComponent(RestWorldSignalFormComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('rel', 'test:rel');
    fixture.componentRef.setInput('template', new Template({
      target: 'https://example.com/test',
      method: 'PUT',
      properties: [
        { name: 'name', type: PropertyType.Text, value: 'John Doe' }
      ]
    }));
    fixture.detectChanges();
  });

  it('should attach the field-specific server error to the corresponding field', async () => {
    await component.submit();
    fixture.detectChanges();

    const nameField = (component.form() as any).name;
    expect(nameField().errors().some((e: { kind: string; message?: string }) => e.kind === 'remote' && e.message?.includes('The name is already taken.'))).toBe(true);
  });

  it('should attach the whole-form detail as a root-level error', async () => {
    await component.submit();
    fixture.detectChanges();

    const rootErrors = component.form()().errors();
    expect(rootErrors.some((e: { kind: string; message?: string }) => e.kind === 'remote' && e.message === 'Please correct the errors below.')).toBe(true);
  });

  it('should render the whole-form error banner in the template', async () => {
    await component.submit();
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Please correct the errors below.');
  });
});
