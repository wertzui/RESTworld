import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PostWithAuthorComponent } from './post-with-author.component';

describe('PostWithAuthorComponent', () => {
  let component: PostWithAuthorComponent;
  let fixture: ComponentFixture<PostWithAuthorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [PostWithAuthorComponent]
})
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PostWithAuthorComponent);
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
