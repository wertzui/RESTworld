import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PostsForBlogComponent } from './posts-for-blog.component';

describe('PostsForBlogComponent', () => {
  let component: PostsForBlogComponent;
  let fixture: ComponentFixture<PostsForBlogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PostsForBlogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PostsForBlogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
