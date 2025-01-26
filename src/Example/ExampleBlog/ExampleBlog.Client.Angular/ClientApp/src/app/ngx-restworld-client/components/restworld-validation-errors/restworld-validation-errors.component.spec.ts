import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldValidationErrorsComponent } from './restworld-validation-errors.component';

describe('RestWorldValidationErrorsComponent', () => {
  let component: RestWorldValidationErrorsComponent;
  let fixture: ComponentFixture<RestWorldValidationErrorsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldValidationErrorsComponent]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldValidationErrorsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
