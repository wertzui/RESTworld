import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldInputCollectionComponent } from '../restworld-inputs';

describe('RestWorldInputCollectionComponent', () => {
  let component: RestWorldInputCollectionComponent<any>;
  let fixture: ComponentFixture<RestWorldInputCollectionComponent<any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldInputCollectionComponent]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldInputCollectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
