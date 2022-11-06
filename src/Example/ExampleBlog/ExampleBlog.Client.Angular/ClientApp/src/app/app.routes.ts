import { MenuItem } from "primeng/api";
import { DeepLinkingRoute, DeepLinkingWrapperComponent } from '@jdrks/ngx-deep-linking';
import { RESTworldListViewComponent } from "./ngx-restworld-client/views/restworld-list-view/restworld-list-view.component";
import { RESTworldEditViewComponent } from "./ngx-restworld-client/views/restworld-edit-view/restworld-edit-view.component";
import { PostWithAuthorComponent } from "./blog-posts/post-with-author.component";
import { HomeComponent } from "./home/home.component";
import { PostWithAuthorListComponent } from './blog-posts/post-with-autor-list.component';

export const AppRoutes: DeepLinkingRoute[] = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  {
    path: 'list/postWithAuthor',
    runGuardsAndResolvers: 'always',
    component: PostWithAuthorListComponent
  },
  {
    path: 'edit/postWithAuthor/:apiName/:rel/:uri',
    component: DeepLinkingWrapperComponent,
    wrappedComponent: PostWithAuthorComponent,
    deepLinking: {
      params: [
        { name: 'apiName', type: 'string' },
        { name: 'uri', type: 'string' },
        { name: 'rel', type: 'string' }
      ]
    }
  },
  {
    path: 'list/:apiName/:rel',
    runGuardsAndResolvers: 'always',
    component: DeepLinkingWrapperComponent,
    wrappedComponent: RESTworldListViewComponent,
    deepLinking: {
      params: [
        { name: 'apiName', type: 'string' },
        { name: 'rel', type: 'string' }
      ],
      queryParams: [
        { name: 'editLink', type: 'string' },
        { name: 'sortField', type: 'string' },
        { name: 'sortOrder', type: 'number' }
      ]
    }
  },
  {
    path: 'edit/:apiName/:rel/:uri',
    component: DeepLinkingWrapperComponent,
    wrappedComponent: RESTworldEditViewComponent,
    deepLinking: {
      params: [
        { name: 'apiName', type: 'string' },
        { name: 'uri', type: 'string' },
        { name: 'rel', type: 'string' }
      ]
    }
  }
];

export const AppMenu: MenuItem[] = [
  {
    label: "Examples with defaults",
    items: [
      {
        label: "Blogs",
        routerLink: ['list', 'ExampleBlog', 'MyEx:Blog']
      },
      {
        label: "Authors",
        routerLink: ['list', 'ExampleBlog', 'MyEx:Author']
      },
      {
        label: "Posts",
        routerLink: ['list', 'ExampleBlog', 'MyEx:Post'],
        queryParams: { sortField: 'lastChangedAt', sortOrder: -1 }
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
      }
    ]
  }];
