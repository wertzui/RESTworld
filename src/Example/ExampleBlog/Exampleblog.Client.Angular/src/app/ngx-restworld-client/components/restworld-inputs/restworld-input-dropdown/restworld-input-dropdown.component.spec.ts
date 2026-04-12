import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ControlContainer, FormControl, FormGroup } from '@angular/forms';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';

import { RestWorldInputDropdownComponent } from '../restworld-inputs';

describe('RestWorldInputDropdownComponent', () => {
  let component: RestWorldInputDropdownComponent<any, any>;
  let fixture: ComponentFixture<RestWorldInputDropdownComponent<any, any>>;

  const mockControlContainer = { control: new FormGroup({ test: new FormControl(null) }) };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldInputDropdownComponent],
    providers: [
      provideHttpClient(),
      MessageService,
      { provide: ControlContainer, useValue: mockControlContainer }
    ]
})
    .overrideComponent(RestWorldInputDropdownComponent, {
      set: {
        viewProviders: [{ provide: ControlContainer, useValue: mockControlContainer }],
        template: ''
      }
    })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldInputDropdownComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('property', { name: 'test', type: 'text', options: { inline: [], selectedValues: null } });
    fixture.componentRef.setInput('apiName', 'test');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
