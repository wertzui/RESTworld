import { MenuItem } from "primeng/api";
import { RESTworldListViewComponent } from "./ngx-restworld-client/views/restworld-list-view/restworld-list-view.component";
import { RESTworldEditViewComponent } from "./ngx-restworld-client/views/restworld-edit-view/restworld-edit-view.component";
import { PostWithAuthorComponent } from "./blog-posts/post-with-author.component";
import { HomeComponent } from "./home/home.component";
import { PostWithAuthorListComponent } from './blog-posts/post-with-autor-list.component';
import { PostsForBlogComponent } from "./posts-for-blog/posts-for-blog.component";
import { Route, RouterLink } from "@angular/router";

export const AppRoutes: Route[] = [
  {
    path: '',
    component: HomeComponent, pathMatch: 'full' },
  {
    path: 'edit/postsForBlog/:apiName/:rel',
    component: PostsForBlogComponent,
  },
  {
    path: 'list/postWithAuthor',
    runGuardsAndResolvers: 'always',
    component: PostWithAuthorListComponent
  },
  {
    path: 'edit/postWithAuthor/:apiName/:rel/:uri',
    component: PostWithAuthorComponent,
  },
  {
    path: 'list/:apiName/:rel',
    runGuardsAndResolvers: 'always',
    component: RESTworldListViewComponent,
  },
  {
    path: 'edit/:apiName/:rel/:uri',
    component: RESTworldEditViewComponent,
  }
];

export const AppMenu: MenuItem[] = [
  {
    label: "Examples with defaults",
    items: [
      {
        label: "Blogs",
        routerLink: ['list', 'ExampleBlog', 'MyEx:Blog'],
        queryParams: { initialOrderby: 'id desc' }
      },
      {
        label: "Authors",
        routerLink: ['list', 'ExampleBlog', 'MyEx:Author']
      },
      {
        label: "Posts",
        routerLink: ['list', 'ExampleBlog', 'MyEx:Post'],
        queryParams: { orderBy: 'lastChangedAt desc' }
      },
      {
        label: "Statistics",
        routerLink: ['list', 'ExampleBlog', 'MyEx:AuthorStatistics']
      }]
  },
  {
    label: "Examples with custom templates",
    items: [
      {
        label: "Posts with author",
        routerLink: ['list/postWithAuthor']
      },
      {
        label: "TestEntries",
        routerLink: ['list', 'ExampleBlog', 'MyEx:Test']
      },
      {
        label: "Bulk edit Posts",
        routerLink: ['edit', 'postsForBlog', 'ExampleBlog', 'MyEx:Post'],
      }
    ]
  }];
