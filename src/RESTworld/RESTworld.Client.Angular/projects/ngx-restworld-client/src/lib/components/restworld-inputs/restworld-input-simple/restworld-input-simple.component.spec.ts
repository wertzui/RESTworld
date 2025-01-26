import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldInputSimpleComponent } from '../restworld-inputs';

describe('RestWorldInputSimpleComponent', () => {
  let component: RestWorldInputSimpleComponent<any>;
  let fixture: ComponentFixture<RestWorldInputSimpleComponent<any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldInputSimpleComponent]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldInputSimpleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
