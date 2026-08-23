import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';

import { PostsForBlogSignalComponent } from './posts-for-blog-signal.component';

describe('PostsForBlogSignalComponent', () => {
  let component: PostsForBlogSignalComponent;
  let fixture: ComponentFixture<PostsForBlogSignalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [PostsForBlogSignalComponent],
    providers: [provideHttpClient(), MessageService]
})
    .compileComponents();

    fixture = TestBed.createComponent(PostsForBlogSignalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
