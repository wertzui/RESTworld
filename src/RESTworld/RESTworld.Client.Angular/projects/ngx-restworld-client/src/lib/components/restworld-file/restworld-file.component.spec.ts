import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldFileComponent } from './restworld-file.component';

describe('RestWorldFileComponent', () => {
  let component: RestWorldFileComponent;
  let fixture: ComponentFixture<RestWorldFileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RestWorldFileComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RestWorldFileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
