import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldLabelComponent } from './rest-world-label.component';

describe('RestWorldLabelComponent', () => {
  let component: RestWorldLabelComponent;
  let fixture: ComponentFixture<RestWorldLabelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RestWorldLabelComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldLabelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
