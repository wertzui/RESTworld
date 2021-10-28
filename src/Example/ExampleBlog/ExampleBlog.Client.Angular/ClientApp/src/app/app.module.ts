import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
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

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    NavigationComponent,
    PostWithAuthorComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(AppRoutes, { onSameUrlNavigation: 'reload' }),
    CommonModule,
    MenubarModule,
    TableModule,
    TooltipModule,
    ScrollingModule,
    BrowserAnimationsModule,
    RestworldClientModule,
  ],
  providers: [
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
