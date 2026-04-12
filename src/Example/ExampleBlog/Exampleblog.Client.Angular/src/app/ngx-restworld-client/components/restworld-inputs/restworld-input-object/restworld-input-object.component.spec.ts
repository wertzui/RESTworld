import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ControlContainer, FormControl, FormGroup } from '@angular/forms';

import { RestWorldInputObjectComponent } from '../restworld-inputs';

describe('RestWorldInputObjectComponent', () => {
  let component: RestWorldInputObjectComponent<any>;
  let fixture: ComponentFixture<RestWorldInputObjectComponent<any>>;

  const mockControlContainer = { control: new FormGroup({ test: new FormGroup({}) }) };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldInputObjectComponent],
    providers: [{ provide: ControlContainer, useValue: mockControlContainer }]
})
    .overrideComponent(RestWorldInputObjectComponent, {
      set: {
        viewProviders: [{ provide: ControlContainer, useValue: mockControlContainer }],
        template: ''
      }
    })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldInputObjectComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('property', { name: 'test', type: 'object', _templates: { default: { properties: [] } } });
    fixture.componentRef.setInput('apiName', 'test');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
