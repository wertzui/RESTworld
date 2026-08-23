import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { PropertyType, SignalFormService, Template } from '@wertzui/ngx-hal-client';

import { RestWorldSignalTableComponent } from './restworld-signal-table.component';

Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false
  })
});

describe('RestWorldSignalTableComponent', () => {
  let component: RestWorldSignalTableComponent<Record<string, any>>;
  let fixture: ComponentFixture<RestWorldSignalTableComponent<Record<string, any>>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldSignalTableComponent],
      providers: [provideRouter([]), provideHttpClient(), MessageService]
    })
      .compileComponents();

    fixture = TestBed.createComponent(RestWorldSignalTableComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('rows', []);
    fixture.componentRef.setInput('searchTemplate', new Template({ properties: [] }));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

describe('RestWorldSignalTableComponent with editable rows', () => {
  let component: RestWorldSignalTableComponent<Record<string, any>>;
  let fixture: ComponentFixture<RestWorldSignalTableComponent<Record<string, any>>>;

  const rows = [
    { name: 'Row 1' },
    { name: 'Row 2' }
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldSignalTableComponent],
      providers: [provideRouter([]), provideHttpClient(), MessageService]
    })
      .compileComponents();

    fixture = TestBed.createComponent(RestWorldSignalTableComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('rows', rows);
    // `lazy` intentionally left at its default (`true`): mirroring `<rw-table>`, the internal signal forms
    // (like `<rw-table>`'s `formArray`) are only (re-)built while `lazy()` is true.
    fixture.componentRef.setInput('searchTemplate', new Template({
      properties: [{ name: 'name', type: PropertyType.Text, value: '' }]
    }));
    fixture.componentRef.setInput('editTemplate', new Template({
      properties: [{ name: 'name', type: PropertyType.Text, value: '' }]
    }));
    fixture.detectChanges();
  });

  it('should create a signal form for every row and expose it via editedRows', () => {
    expect(component.editedRows().length).toBe(2);
    expect(component.editedRows().map(r => r['name'])).toEqual(['Row 1', 'Row 2']);
  });

  it('should render an editable input for each row', () => {
    const inputs = fixture.nativeElement.querySelectorAll('input') as NodeListOf<HTMLInputElement>;
    const values = Array.from(inputs).map(i => i.value);
    expect(values).toContain('Row 1');
    expect(values).toContain('Row 2');
  });
});

describe('RestWorldSignalTableComponent bound to an externally-owned field (mirrors <rw-table>\'s [formArrayName])', () => {
  let component: RestWorldSignalTableComponent<Record<string, any>>;
  let fixture: ComponentFixture<RestWorldSignalTableComponent<Record<string, any>>>;

  const editTemplate = new Template({
    properties: [{ name: 'name', type: PropertyType.Text, value: '' }]
  });

  function createParentTemplate() {
    return new Template({
      properties: [{
        name: 'tags',
        type: PropertyType.Collection,
        value: undefined,
        _templates: {
          default: { properties: [{ name: 'name', type: PropertyType.Text, value: '' }] },
          0: { title: '0', properties: [{ name: 'name', type: PropertyType.Text, value: 'First' }] },
          1: { title: '1', properties: [{ name: 'name', type: PropertyType.Text, value: 'Second' }] }
        }
      }]
    });
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldSignalTableComponent],
      providers: [provideRouter([]), provideHttpClient(), MessageService]
    })
      .compileComponents();

    const signalFormService = TestBed.inject(SignalFormService);
    const parentTemplate = createParentTemplate();
    const { form } = TestBed.runInInjectionContext(() => signalFormService.createSignalFormFromTemplate(parentTemplate));

    fixture = TestBed.createComponent(RestWorldSignalTableComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('apiName', 'test');
    // In bound mode, `rows()` still drives what `<p-table>` displays (mirrors `<rw-table>`'s own usage inside
    // `<rw-input-collection>`, which passes `rows()` derived from the bound `FormArray`'s own value).
    fixture.componentRef.setInput('rows', [{ name: 'First' }, { name: 'Second' }]);
    fixture.componentRef.setInput('searchTemplate', editTemplate);
    fixture.componentRef.setInput('editTemplate', editTemplate);
    fixture.componentRef.setInput('lazy', false);
    fixture.componentRef.setInput('field', (form as any).tags);
    fixture.detectChanges();
  });

  it('should be editable and read edited rows from the bound field instead of building internal signal forms', () => {
    expect(component.isEditable()).toBe(true);
    expect(component.editedRows().map(r => r['name'])).toEqual(['First', 'Second']);
  });

  it('should render an editable input for each row bound to the external field', () => {
    const inputs = fixture.nativeElement.querySelectorAll('input') as NodeListOf<HTMLInputElement>;
    const values = Array.from(inputs).map(i => i.value);
    expect(values).toContain('First');
    expect(values).toContain('Second');
  });

  it('should update the bound field value when editing a cell', () => {
    const field = component.getFieldAtIndex(0, 'name');
    expect(field).toBeTruthy();
    (field as any)().value.set('Changed');
    fixture.detectChanges();

    expect(component.editedRows()[0]['name']).toBe('Changed');
  });
});
