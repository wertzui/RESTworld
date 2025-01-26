import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ResourceDto } from '@wertzui/ngx-hal-client';

import { RESTworldListViewComponent } from './restworld-list-view.component';

describe('RestworldListViewComponent', () => {
  let component: RESTworldListViewComponent<ResourceDto>;
  let fixture: ComponentFixture<RESTworldListViewComponent<ResourceDto>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RESTworldListViewComponent]
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
