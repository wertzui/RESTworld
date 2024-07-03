import { BrowserModule } from '@angular/platform-browser';
import { ErrorHandler, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { provideHttpClient, withFetch, withInterceptorsFromDi } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { NavigationComponent } from './navigation/navigation.component';
import { MenubarModule } from 'primeng/menubar';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip'
import { ScrollingModule } from '@angular/cdk/scrolling';
import { RestworldClientModule } from './ngx-restworld-client/restworld-client.module'
import { AppRoutes } from './app.routes';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { PostWithAuthorComponent } from './blog-posts/post-with-author.component';
import { PostWithAuthorListComponent } from './blog-posts/post-with-autor-list.component';
import { DialogModule } from 'primeng/dialog';
import { AvatarGenerator } from './ngx-restworld-client/services/avatar-generator';
import { ExampleAvatarGenerator } from './ExampleAvatarGenerator';
import { PostsForBlogComponent } from './posts-for-blog/posts-for-blog.component';
import { ToastModule } from 'primeng/toast';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    NavigationComponent,
    PostWithAuthorComponent,
    PostWithAuthorListComponent,
    PostsForBlogComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot(AppRoutes, { onSameUrlNavigation: 'reload', bindToComponentInputs: true }),
    CommonModule,
    MenubarModule,
    TableModule,
    TooltipModule,
    ScrollingModule,
    BrowserAnimationsModule,
    DialogModule,
    RestworldClientModule,
    ReactiveFormsModule,
    ToastModule,
  ],
  providers: [
    {
      provide: AvatarGenerator,
      useClass: ExampleAvatarGenerator
    },
    provideHttpClient(withInterceptorsFromDi())
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
