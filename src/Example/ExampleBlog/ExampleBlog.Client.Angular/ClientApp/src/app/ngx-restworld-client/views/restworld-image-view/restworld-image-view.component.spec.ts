import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RESTWorldImageViewComponent } from './restworld-image-view.component';

describe('RESTWorldImageViewComponent', () => {
  let component: RESTWorldImageViewComponent;
  let fixture: ComponentFixture<RESTWorldImageViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RESTWorldImageViewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RESTWorldImageViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
