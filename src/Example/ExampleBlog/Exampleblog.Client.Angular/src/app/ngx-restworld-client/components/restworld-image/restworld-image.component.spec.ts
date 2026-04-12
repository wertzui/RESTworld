import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideAnimations } from '@angular/platform-browser/animations';

import { RestWorldImageComponent } from './restworld-image.component';

const mockProperty = {
  name: 'test',
  type: 'image',
  restWorldImage: {
    alignImage: 'center' as const,
    aspectRatio: 1,
    canvasRotation: 0,
    containWithinAspectRatio: false,
    cropperMaxHeight: 0,
    cropperMaxWidth: 0,
    cropperMinHeight: 0,
    cropperMinWidth: 0,
    cropperStaticHeight: 0,
    cropperStaticWidth: 0,
    format: 'png' as const,
    imageQuality: 100,
    initialStepSize: 1,
    maintainAspectRatio: false,
    onlyScaleDown: false,
    resizeToHeight: 0,
    resizeToWidth: 0,
    roundCropper: false,
  }
};

describe('RestWorldImageComponent', () => {
  let component: RestWorldImageComponent;
  let fixture: ComponentFixture<RestWorldImageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [RestWorldImageComponent],
    providers: [provideAnimations()]
})
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RestWorldImageComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('property', mockProperty);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
