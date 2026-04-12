import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldFormElementComponent } from '../restworld-inputs';

describe('RestWorldFormElementComponent', () => {
  let component: RestWorldFormElementComponent<any>;
  let fixture: ComponentFixture<RestWorldFormElementComponent<any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldFormElementComponent]
})
    .overrideComponent(RestWorldFormElementComponent, { set: { template: '' } })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldFormElementComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('property', { name: 'test', type: 'text', value: null });
    fixture.componentRef.setInput('apiName', 'test');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
