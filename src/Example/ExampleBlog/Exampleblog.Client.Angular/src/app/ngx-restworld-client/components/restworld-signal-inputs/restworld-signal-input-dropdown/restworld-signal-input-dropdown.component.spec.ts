import { Component, ViewChild } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { PropertyType, SignalFormService, Template } from '@wertzui/ngx-hal-client';

import { RestWorldSignalInputDropdownComponent } from './restworld-signal-input-dropdown.component';

describe('RestWorldSignalInputDropdownComponent', () => {
  let component: RestWorldSignalInputDropdownComponent<any, any>;
  let fixture: ComponentFixture<RestWorldSignalInputDropdownComponent<any, any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldSignalInputDropdownComponent],
      providers: [provideHttpClient(), MessageService]
    })
      .compileComponents();

    const signalFormService = TestBed.inject(SignalFormService);
    const template = new Template({
      properties: [{
        name: 'test',
        type: PropertyType.Text,
        value: 1,
        options: {
          inline: [{ prompt: 'One', value: 1 }, { prompt: 'Two', value: 2 }],
          selectedValues: [1]
        }
      }]
    });
    const { form: signalForm } = TestBed.runInInjectionContext(() => signalFormService.createSignalFormFromTemplate(template));

    fixture = TestBed.createComponent(RestWorldSignalInputDropdownComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('property', template.properties[0]);
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('field', signalForm.test);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load the inline options', () => {
    expect(component.optionsManager.items.value()).toEqual([{ prompt: 'One', value: 1 }, { prompt: 'Two', value: 2 }]);
  });
});

describe('RestWorldSignalInputDropdownComponent with a custom #inputOptionsSingle template', () => {
  // Regression test: `inputOptionsSingleRef`/`inputOptionsMultipleRef` were previously declared but never wired
  // into the template via `*ngTemplateOutlet`, so a projected `#inputOptionsSingle`/`#inputOptionsMultiple`
  // template was silently ignored. This also verifies the outlet context exposes `property`, `apiName`,
  // `field`, and `items` (matching the Reactive Forms `<rw-input-dropdown>`'s context shape, minus the
  // Reactive-Forms-only `useTemplateDrivenForms`/`model`).
  @Component({
    selector: 'rw-test-host',
    imports: [RestWorldSignalInputDropdownComponent],
    template: `
      <rw-signal-input-dropdown [property]="property" [apiName]="apiName" [field]="field">
        <ng-template #inputOptionsSingle let-property="property" let-apiName="apiName" let-field="field" let-items="items">
          <div class="custom-single-template">Custom: {{ property().name }} / {{ apiName() }} / {{ items.value()?.length ?? 0 }}</div>
        </ng-template>
      </rw-signal-input-dropdown>
    `
  })
  class TestHostComponent {
    @ViewChild(RestWorldSignalInputDropdownComponent) dropdown!: RestWorldSignalInputDropdownComponent<any, any>;
    property: any;
    apiName = 'test';
    field: any;
  }

  let hostFixture: ComponentFixture<TestHostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
      providers: [provideHttpClient(), MessageService]
    })
      .compileComponents();

    const signalFormService = TestBed.inject(SignalFormService);
    const template = new Template({
      properties: [{
        name: 'test',
        type: PropertyType.Text,
        value: 1,
        options: {
          inline: [{ prompt: 'One', value: 1 }, { prompt: 'Two', value: 2 }],
          selectedValues: [1]
        }
      }]
    });
    const { form: signalForm } = TestBed.runInInjectionContext(() => signalFormService.createSignalFormFromTemplate(template));

    hostFixture = TestBed.createComponent(TestHostComponent);
    hostFixture.componentInstance.property = template.properties[0];
    hostFixture.componentInstance.field = (signalForm as any).test;
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    hostFixture.detectChanges();
  });

  it('should render the projected custom template instead of the default p-select', () => {
    const custom = hostFixture.nativeElement.querySelector('.custom-single-template');
    expect(custom).toBeTruthy();
    expect(hostFixture.nativeElement.querySelector('p-select')).toBeFalsy();
  });

  it('should pass property, apiName and the loaded items to the custom template context', () => {
    const custom = hostFixture.nativeElement.querySelector('.custom-single-template') as HTMLElement;
    expect(custom.textContent).toContain('test');
    expect(custom.textContent).toContain('2');
  });
});
