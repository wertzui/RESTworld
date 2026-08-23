import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PostWithAuthorSignalComponent } from './post-with-author-signal.component';

describe('PostWithAuthorSignalComponent', () => {
  let component: PostWithAuthorSignalComponent;
  let fixture: ComponentFixture<PostWithAuthorSignalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [PostWithAuthorSignalComponent]
})
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PostWithAuthorSignalComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('uri', '');
    fixture.componentRef.setInput('apiName', 'test');
    fixture.componentRef.setInput('rel', 'test');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
