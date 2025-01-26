import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldTableColumnFilterElementComponent } from './rest-world-table-column-filter-element.component';

describe('RestWorldTableColumnFilterElementComponent', () => {
  let component: RestWorldTableColumnFilterElementComponent;
  let fixture: ComponentFixture<RestWorldTableColumnFilterElementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestWorldTableColumnFilterElementComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldTableColumnFilterElementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
