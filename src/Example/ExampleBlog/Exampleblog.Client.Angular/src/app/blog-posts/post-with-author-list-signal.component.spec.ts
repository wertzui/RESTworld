import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';

import { PostWithAuthorListSignalComponent } from './post-with-author-list-signal.component';

describe('PostWithAuthorListSignalComponent', () => {
  let component: PostWithAuthorListSignalComponent;
  let fixture: ComponentFixture<PostWithAuthorListSignalComponent>;

  beforeEach(async () => {
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
        dispatchEvent: () => false,
      }),
    });

    await TestBed.configureTestingModule({
    imports: [PostWithAuthorListSignalComponent],
    providers: [provideHttpClient(), provideRouter([]), MessageService, ConfirmationService]
})
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PostWithAuthorListSignalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
