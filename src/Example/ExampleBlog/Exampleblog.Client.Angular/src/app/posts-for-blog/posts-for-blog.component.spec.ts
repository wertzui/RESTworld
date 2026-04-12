import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';

import { PostsForBlogComponent } from './posts-for-blog.component';

describe('PostsForBlogComponent', () => {
  let component: PostsForBlogComponent;
  let fixture: ComponentFixture<PostsForBlogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [PostsForBlogComponent],
    providers: [provideHttpClient(), MessageService]
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
