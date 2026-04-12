import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';

import { RESTworldEditViewComponent } from './restworld-edit-view.component';

Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false
  })
});

describe('RESTworldEditViewComponent', () => {
  let component: RESTworldEditViewComponent;
  let fixture: ComponentFixture<RESTworldEditViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RESTworldEditViewComponent],
    providers: [provideHttpClient(), provideRouter([]), MessageService, ConfirmationService]
})
    .overrideComponent(RESTworldEditViewComponent, { set: { template: '' } })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RESTworldEditViewComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('rel', 'test');
    fixture.componentRef.setInput('uri', '/test/1');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
