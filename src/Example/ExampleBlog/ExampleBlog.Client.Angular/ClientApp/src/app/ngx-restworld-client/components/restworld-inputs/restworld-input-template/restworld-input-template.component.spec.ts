import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldInputTemplateComponent } from '../restworld-inputs';

describe('RestWorldFormComponent', () => {
  let component: RestWorldInputTemplateComponent<any>;
  let fixture: ComponentFixture<RestWorldInputTemplateComponent<any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldInputTemplateComponent]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldInputTemplateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
