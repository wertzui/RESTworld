import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldFormComponent } from './restworld-form.component';

describe('RestWorldFormComponent', () => {
  let component: RestWorldFormComponent;
  let fixture: ComponentFixture<RestWorldFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RestWorldFormComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
