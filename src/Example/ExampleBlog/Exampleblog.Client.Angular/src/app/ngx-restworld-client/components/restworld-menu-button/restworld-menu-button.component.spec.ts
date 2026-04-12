import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { RestWorldMenuButtonComponent } from './restworld-menu-button.component';

describe('RestWorldMenuButtonComponent', () => {
  let component: RestWorldMenuButtonComponent;
  let fixture: ComponentFixture<RestWorldMenuButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldMenuButtonComponent],
    providers: [provideRouter([])]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldMenuButtonComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('items', []);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
