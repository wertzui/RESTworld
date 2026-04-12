import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ResourceDto } from '@wertzui/ngx-hal-client';

import { RESTworldListViewComponent } from './restworld-list-view.component';

describe('RestworldListViewComponent', () => {
  let component: RESTworldListViewComponent<ResourceDto & Record<string, unknown>>;
  let fixture: ComponentFixture<RESTworldListViewComponent<ResourceDto & Record<string, unknown>>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RESTworldListViewComponent],
    providers: [provideHttpClient(), provideRouter([]), MessageService, ConfirmationService]
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
