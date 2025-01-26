import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldFormComponent } from './restworld-form.component';
import type { PropertyDto, SimpleValue } from "@wertzui/ngx-hal-client";

describe('RestWorldFormComponent', () => {
  let component: RestWorldFormComponent<ReadonlyArray<PropertyDto<SimpleValue, string, string>>>;
  let fixture: ComponentFixture<RestWorldFormComponent<ReadonlyArray<PropertyDto<SimpleValue, string, string>>>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldFormComponent]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
