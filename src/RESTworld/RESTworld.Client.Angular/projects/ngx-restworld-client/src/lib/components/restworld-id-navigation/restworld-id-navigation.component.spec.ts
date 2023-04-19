import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldIdNavigationComponent } from './restworld-id-navigation.component';

describe('RestWorldIdNavigationComponent', () => {
  let component: RestWorldIdNavigationComponent;
  let fixture: ComponentFixture<RestWorldIdNavigationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RestWorldIdNavigationComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldIdNavigationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
