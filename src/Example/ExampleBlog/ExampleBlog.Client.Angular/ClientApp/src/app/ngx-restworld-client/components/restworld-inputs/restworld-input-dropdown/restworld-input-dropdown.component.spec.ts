import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldInputDropdownComponent } from '../restworld-inputs';

describe('RestWorldInputDropdownComponent', () => {
  let component: RestWorldInputDropdownComponent;
  let fixture: ComponentFixture<RestWorldInputDropdownComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldInputDropdownComponent]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldInputDropdownComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
