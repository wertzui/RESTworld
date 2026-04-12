import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldLabelComponent } from './restworld-label.component';

describe('RestWorldLabelComponent', () => {
  let component: RestWorldLabelComponent<any>;
  let fixture: ComponentFixture<RestWorldLabelComponent<any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldLabelComponent]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldLabelComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('property', { name: 'test', type: 'text', prompt: 'Test' });
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
