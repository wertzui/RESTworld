import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldFormElementComponent } from '../restworld-inputs';

describe('RestWorldFormElementComponent', () => {
  let component: RestWorldFormElementComponent<any>;
  let fixture: ComponentFixture<RestWorldFormElementComponent<any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldFormElementComponent]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldFormElementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
