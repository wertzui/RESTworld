import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RESTworldEditViewComponent } from './restworld-edit-view.component';

describe('RESTworldEditViewComponent', () => {
  let component: RESTworldEditViewComponent;
  let fixture: ComponentFixture<RESTworldEditViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RESTworldEditViewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RESTworldEditViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
