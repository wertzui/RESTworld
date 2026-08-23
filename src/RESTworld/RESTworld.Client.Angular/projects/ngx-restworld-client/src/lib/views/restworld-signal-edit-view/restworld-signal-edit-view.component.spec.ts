import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';

import { RESTworldSignalEditViewComponent } from './restworld-signal-edit-view.component';

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

describe('RESTworldSignalEditViewComponent', () => {
  let component: RESTworldSignalEditViewComponent;
  let fixture: ComponentFixture<RESTworldSignalEditViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RESTworldSignalEditViewComponent],
    providers: [provideHttpClient(), provideRouter([]), MessageService, ConfirmationService]
})
    .overrideComponent(RESTworldSignalEditViewComponent, { set: { template: '' } })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RESTworldSignalEditViewComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('rel', 'test');
    fixture.componentRef.setInput('uri', '/test/1');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should navigate to the Signal Forms edit route (not the Reactive Forms route) when creating a new resource', () => {
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    // newHref() is derived from the loaded resource, which is not set up here; set it directly for the test.
    (component as unknown as { newHref: () => string }).newHref = () => '/test/new';

    component.createNew();

    expect(navigateSpy).toHaveBeenCalledWith(['edit/signal', 'test', 'test', '/test/new']);
  });

  it('should navigate to the Signal Forms list route (not the Reactive Forms route) after a delete', async () => {
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    await component.afterDelete();

    expect(navigateSpy).toHaveBeenCalledWith(['list/signal', 'test', 'test']);
  });

  it('should navigate to the Signal Forms edit route (not the Reactive Forms route) after a submit that created a resource', async () => {
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    await component.afterSubmit({ location: '/test/1', status: 201 });

    expect(navigateSpy).toHaveBeenCalledWith(['edit/signal', 'test', 'test', '/test/1']);
  });
});
