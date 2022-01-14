import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestworldEditFormComponent } from './restworld-edit-form.component';

describe('RestworldEditFormComponent', () => {
  let component: RestworldEditFormComponent;
  let fixture: ComponentFixture<RestworldEditFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RestworldEditFormComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RestworldEditFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
