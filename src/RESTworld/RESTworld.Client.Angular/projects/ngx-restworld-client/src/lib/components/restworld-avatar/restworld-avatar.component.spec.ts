import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldAvatarComponent } from './restworld-avatar.component';

describe('RestWorldAvatarComponent', () => {
  let component: RestWorldAvatarComponent;
  let fixture: ComponentFixture<RestWorldAvatarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldAvatarComponent]
})
    .compileComponents();

    fixture = TestBed.createComponent(RestWorldAvatarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
