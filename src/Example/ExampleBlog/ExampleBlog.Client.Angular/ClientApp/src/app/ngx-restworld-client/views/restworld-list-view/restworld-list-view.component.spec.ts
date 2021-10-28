import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RESTworldListViewComponent } from './restworld-list-view.component';

describe('RestworldListViewComponent', () => {
  let component: RESTworldListViewComponent;
  let fixture: ComponentFixture<RESTworldListViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RESTworldListViewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RESTworldListViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
