import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RESTWorldFileViewComponent } from './restworld-file-view.component';

describe('RESTWorldFileViewComponent', () => {
  let component: RESTWorldFileViewComponent;
  let fixture: ComponentFixture<RESTWorldFileViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RESTWorldFileViewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RESTWorldFileViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
