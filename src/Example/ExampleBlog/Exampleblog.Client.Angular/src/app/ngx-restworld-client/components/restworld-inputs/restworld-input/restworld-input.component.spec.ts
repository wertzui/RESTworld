import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldInputComponent } from '../restworld-inputs';

describe('RestWorldInputElementComponent', () => {
  let component: RestWorldInputComponent;
  let fixture: ComponentFixture<RestWorldInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldInputComponent]
})
    .overrideComponent(RestWorldInputComponent, { set: { template: '' } })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldInputComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('property', { name: 'test', type: 'text', value: null });
    fixture.componentRef.setInput('apiName', 'test');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
