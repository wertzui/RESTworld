import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldInputComponent } from '../restworld-inputs';

describe('RestWorldInputElementComponent', () => {
  let component: RestWorldInputComponent;
  let fixture: ComponentFixture<RestWorldInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RestWorldInputComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
