import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldTableColumnFilterElementComponent } from './rest-world-table-column-filter-element.component';
import { Property } from '@wertzui/ngx-hal-client';

describe('RestWorldTableColumnFilterElementComponent', () => {
  let component: RestWorldTableColumnFilterElementComponent<Property>;
  let fixture: ComponentFixture<RestWorldTableColumnFilterElementComponent<Property>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldTableColumnFilterElementComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldTableColumnFilterElementComponent<Property>);
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
});
