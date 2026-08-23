import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { PropertyType, SignalFormService, Template } from '@wertzui/ngx-hal-client';

import { RestWorldSignalInputCollectionComponent } from './restworld-signal-input-collection.component';

// `<rw-signal-table>` (nested by this component, mirroring `<rw-input-collection>` nesting `<rw-table>`) uses
// PrimeNG's Table, which queries `window.matchMedia` - same mock as restworld-signal-table.component.spec.ts.
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

describe('RestWorldSignalInputCollectionComponent', () => {
  let component: RestWorldSignalInputCollectionComponent<any>;
  let fixture: ComponentFixture<RestWorldSignalInputCollectionComponent<any>>;

  function createProperty() {
    // `_templates` entries can be plain object literals here: `Property`'s own constructor (invoked by
    // `Template.mapProperties` below, when this object is nested inside `new Template({ properties: [...] })`)
    // automatically wraps every `_templates` entry in a real `Template` instance. This matters because the
    // nested `<rw-signal-table>` relies on `Template.propertiesRecord` (a getter only present on the class,
    // not on a plain object) for its `editProperties()`/`showInputField()` logic, which decides whether a
    // cell renders an editable `<rw-signal-input>` or falls back to a read-only `<rw-display>`.
    return {
      name: 'tags',
      type: PropertyType.Collection,
      value: undefined,
      _templates: {
        default: { properties: [{ name: 'name', type: PropertyType.Text, value: '' }] },
        0: { title: '0', properties: [{ name: 'name', type: PropertyType.Text, value: 'First' }] },
        1: { title: '1', properties: [{ name: 'name', type: PropertyType.Text, value: 'Second' }] }
      }
    };
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldSignalInputCollectionComponent],
      providers: [provideRouter([]), provideHttpClient(), MessageService]
    })
      .compileComponents();

    const signalFormService = TestBed.inject(SignalFormService);
    const template = new Template({ properties: [createProperty()] });
    // Use the real, already-wrapped `Property` instance produced by the parent `Template`'s constructor
    // (not the raw `createProperty()` object literal) so `property()._templates.default` is a real `Template`.
    const property = template.properties[0] as any;
    const { form } = TestBed.runInInjectionContext(() => signalFormService.createSignalFormFromTemplate(template));

    fixture = TestBed.createComponent(RestWorldSignalInputCollectionComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('property', property);
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('field', (form as any).tags);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render one input row per existing collection item via the nested rw-signal-table', () => {
    const table = fixture.nativeElement.querySelector('rw-signal-table');
    expect(table).toBeTruthy();
    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
  });

  it('should add a new item to the collection when addItem is called', () => {
    component.addItem();
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(3);
  });

  it('should remove an item from the collection when removeItem is called', () => {
    component.removeItem(0);
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(1);
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(input.value).toBe('Second');
  });
});
