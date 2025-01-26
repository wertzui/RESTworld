import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestWorldImageComponent } from './restworld-image.component';

describe('RestWorldImageComponent', () => {
  let component: RestWorldImageComponent;
  let fixture: ComponentFixture<RestWorldImageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldImageComponent]
})
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RestWorldImageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
