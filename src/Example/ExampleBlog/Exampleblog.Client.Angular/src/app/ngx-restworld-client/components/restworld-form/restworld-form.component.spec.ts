import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Template } from '@wertzui/ngx-hal-client';
import type { PropertyDto, SimpleValue } from "@wertzui/ngx-hal-client";

import { RestWorldFormComponent } from './restworld-form.component';

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

describe('RestWorldFormComponent', () => {
  let component: RestWorldFormComponent<ReadonlyArray<PropertyDto<SimpleValue, string, string>>>;
  let fixture: ComponentFixture<RestWorldFormComponent<ReadonlyArray<PropertyDto<SimpleValue, string, string>>>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldFormComponent],
    providers: [provideHttpClient(), MessageService, ConfirmationService]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldFormComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('template', new Template({ properties: [] }));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
