import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldInputObjectComponent } from '../restworld-inputs';

describe('RestWorldInputObjectComponent', () => {
  let component: RestWorldInputObjectComponent<any>;
  let fixture: ComponentFixture<RestWorldInputObjectComponent<any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RestWorldInputObjectComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldInputObjectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
