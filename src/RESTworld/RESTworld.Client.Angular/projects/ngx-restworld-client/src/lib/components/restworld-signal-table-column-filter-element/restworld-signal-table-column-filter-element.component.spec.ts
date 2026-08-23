import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { Property } from '@wertzui/ngx-hal-client';

import { RestWorldSignalTableColumnFilterElementComponent } from './restworld-signal-table-column-filter-element.component';

describe('RestWorldSignalTableColumnFilterElementComponent', () => {
  let component: RestWorldSignalTableColumnFilterElementComponent<Property>;
  let fixture: ComponentFixture<RestWorldSignalTableColumnFilterElementComponent<Property>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldSignalTableColumnFilterElementComponent],
      providers: [provideHttpClient(), MessageService]
    })
      .compileComponents();

    fixture = TestBed.createComponent(RestWorldSignalTableColumnFilterElementComponent<Property>);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('filterConstraint', { value: null });
    fixture.componentRef.setInput('property', { name: 'test', type: 'text', value: null });
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('value', null);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render an input for the property', () => {
    const input = fixture.nativeElement.querySelector('input');
    expect(input).toBeTruthy();
  });

  it('should update the filter constraint value when the field value changes', () => {
    const field = component.field();
    expect(field).toBeTruthy();
    field!().value.set('hello');
    fixture.detectChanges();

    expect(component.filterConstraint().value).toBe('hello');
  });
});
