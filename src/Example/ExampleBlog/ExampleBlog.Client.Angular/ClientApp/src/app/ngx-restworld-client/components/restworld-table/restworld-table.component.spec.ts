import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldTableComponent } from './restworld-table.component';

describe('RestWorldTableComponent', () => {
  let component: RestWorldTableComponent<unknown>;
  let fixture: ComponentFixture<RestWorldTableComponent<unknown>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RestWorldTableComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
