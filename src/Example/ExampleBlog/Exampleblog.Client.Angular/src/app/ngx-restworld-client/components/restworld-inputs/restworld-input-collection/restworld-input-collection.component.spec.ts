import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ControlContainer, FormArray, FormGroup } from '@angular/forms';

import { RestWorldInputCollectionComponent } from '../restworld-inputs';

describe('RestWorldInputCollectionComponent', () => {
  let component: RestWorldInputCollectionComponent<any>;
  let fixture: ComponentFixture<RestWorldInputCollectionComponent<any>>;

  const mockControlContainer = { control: new FormGroup({ test: new FormArray([]) }) };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldInputCollectionComponent],
    providers: [{ provide: ControlContainer, useValue: mockControlContainer }]
})
    .overrideComponent(RestWorldInputCollectionComponent, {
      set: {
        viewProviders: [{ provide: ControlContainer, useValue: mockControlContainer }],
        template: ''
      }
    })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldInputCollectionComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('property', { name: 'test', type: 'collection', _templates: { default: { properties: [] } } });
    fixture.componentRef.setInput('apiName', 'test');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
