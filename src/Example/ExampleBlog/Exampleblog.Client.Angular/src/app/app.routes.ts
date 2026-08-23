import { MenuItem } from "primeng/api";
import { RESTworldListViewComponent } from "./ngx-restworld-client/views/restworld-list-view/restworld-list-view.component";
import { RESTworldEditViewComponent } from "./ngx-restworld-client/views/restworld-edit-view/restworld-edit-view.component";
import { RESTworldSignalListViewComponent } from "./ngx-restworld-client/views/restworld-signal-list-view/restworld-signal-list-view.component";
import { RESTworldSignalEditViewComponent } from "./ngx-restworld-client/views/restworld-signal-edit-view/restworld-signal-edit-view.component";
import { PostWithAuthorComponent } from "./blog-posts/post-with-author.component";
import { PostWithAuthorSignalComponent } from "./blog-posts/post-with-author-signal.component";
import { HomeComponent } from "./home/home.component";
import { PostWithAuthorListComponent } from './blog-posts/post-with-autor-list.component';
import { PostWithAuthorListSignalComponent } from './blog-posts/post-with-author-list-signal.component';
import { PostsForBlogComponent } from "./posts-for-blog/posts-for-blog.component";
import { PostsForBlogSignalComponent } from "./posts-for-blog/posts-for-blog-signal.component";
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
    path: 'edit/postsForBlog/signal/:apiName/:rel',
    component: PostsForBlogSignalComponent,
  },
  {
    path: 'list/postWithAuthor',
    runGuardsAndResolvers: 'always',
    component: PostWithAuthorListComponent
  },
  {
    path: 'list/postWithAuthor/signal',
    runGuardsAndResolvers: 'always',
    component: PostWithAuthorListSignalComponent
  },
  {
    path: 'edit/postWithAuthor/:apiName/:rel/:uri',
    component: PostWithAuthorComponent,
  },
  {
    path: 'edit/postWithAuthor/signal/:apiName/:rel/:uri',
    component: PostWithAuthorSignalComponent,
  },
  {
    path: 'list/signal/:apiName/:rel',
    runGuardsAndResolvers: 'always',
    component: RESTworldSignalListViewComponent,
    data: { editLink: '/edit/signal' },
  },
  {
    path: 'edit/signal/:apiName/:rel/:uri',
    component: RESTworldSignalEditViewComponent,
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
        queryParams: { $orderby: 'id desc' }
      },
      {
        label: "Authors",
        routerLink: ['list', 'ExampleBlog', 'MyEx:Author']
      },
      {
        label: "Posts",
        routerLink: ['list', 'ExampleBlog', 'MyEx:Post'],
        queryParams: { $orderby: 'lastChangedAt desc' }
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
  },
  {
    label: "Examples with Signal Forms",
    items: [
      {
        label: "Blogs (Signal Forms)",
        routerLink: ['list/signal', 'ExampleBlog', 'MyEx:Blog'],
        queryParams: { $orderby: 'id desc' }
      },
      {
        label: "Authors (Signal Forms)",
        routerLink: ['list/signal', 'ExampleBlog', 'MyEx:Author']
      },
      {
        label: "Posts (Signal Forms)",
        routerLink: ['list/signal', 'ExampleBlog', 'MyEx:Post'],
        queryParams: { $orderby: 'lastChangedAt desc' }
      }
    ]
  },
  {
    label: "Examples with custom templates (Signal Forms)",
    items: [
      {
        label: "Posts with author (Signal Forms)",
        routerLink: ['list/postWithAuthor/signal']
      },
      {
        label: "TestEntries (Signal Forms)",
        routerLink: ['list/signal', 'ExampleBlog', 'MyEx:Test']
      },
      {
        label: "Bulk edit Posts (Signal Forms)",
        routerLink: ['edit', 'postsForBlog', 'signal', 'ExampleBlog', 'MyEx:Post'],
      }
    ]
  }];
