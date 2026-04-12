import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { MessageService } from 'primeng/api';

import { RestWorldIdNavigationComponent } from './restworld-id-navigation.component';

describe('RestWorldIdNavigationComponent', () => {
  let component: RestWorldIdNavigationComponent;
  let fixture: ComponentFixture<RestWorldIdNavigationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldIdNavigationComponent],
    providers: [provideHttpClient(), provideRouter([]), MessageService]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldIdNavigationComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('rel', 'test');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
