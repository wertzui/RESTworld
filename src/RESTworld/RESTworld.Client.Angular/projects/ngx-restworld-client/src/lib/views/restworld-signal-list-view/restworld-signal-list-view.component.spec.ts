import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ResourceDto } from '@wertzui/ngx-hal-client';

import { RESTworldSignalListViewComponent } from './restworld-signal-list-view.component';

// `<rw-signal-table>` (nested by this component) uses PrimeNG's Table, which queries
// `window.matchMedia` - same mock as restworld-signal-table.component.spec.ts.
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

describe('RESTworldSignalListViewComponent', () => {
  let component: RESTworldSignalListViewComponent<ResourceDto & Record<string, unknown>>;
  let fixture: ComponentFixture<RESTworldSignalListViewComponent<ResourceDto & Record<string, unknown>>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RESTworldSignalListViewComponent],
    providers: [provideHttpClient(), provideRouter([]), MessageService, ConfirmationService]
})
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RESTworldSignalListViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should default editLink to /edit/signal (not /edit) so it points at the Signal Forms edit view', () => {
    // Regression test: this previously defaulted to the same "/edit" as the Reactive Forms
    // `RESTworldListViewComponent`, which would silently route row/create links to the Reactive Forms edit
    // view unless every consumer remembered to override `editLink` explicitly.
    expect(component.editLink()).toBe('/edit/signal');
  });

  it('should fall back to /edit/signal when editLink is explicitly set to null/undefined', () => {
    fixture.componentRef.setInput('editLink', null);
    expect(component.editLink()).toBe('/edit/signal');
  });

  it('should still allow overriding editLink to a custom value', () => {
    fixture.componentRef.setInput('editLink', '/custom-edit');
    expect(component.editLink()).toBe('/custom-edit');
  });
});
