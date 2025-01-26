import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldMenuButtonComponent } from './restworld-menu-button.component';

describe('RestWorldMenuButtonComponent', () => {
  let component: RestWorldMenuButtonComponent;
  let fixture: ComponentFixture<RestWorldMenuButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldMenuButtonComponent]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldMenuButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
