import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Template } from '@wertzui/ngx-hal-client';

import { RestWorldTableComponent } from './restworld-table.component';

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

describe('RestWorldTableComponent', () => {
  let component: RestWorldTableComponent<Record<string, any>>;
  let fixture: ComponentFixture<RestWorldTableComponent<Record<string, any>>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldTableComponent],
    providers: [provideRouter([])]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldTableComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('rows', []);
    fixture.componentRef.setInput('searchTemplate', new Template({ properties: [] }));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
