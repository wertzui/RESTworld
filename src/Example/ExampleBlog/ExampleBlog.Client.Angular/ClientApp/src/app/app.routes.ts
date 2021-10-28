import { MenuItem } from "primeng/api";
import { DeepLinkingRoute, DeepLinkingWrapperComponent } from '@jdrks/ngx-deep-linking';
import { RESTworldListViewComponent } from "./ngx-restworld-client/views/restworld-list-view/restworld-list-view.component";
import { RESTworldEditViewComponent } from "./ngx-restworld-client/views/restworld-edit-view/restworld-edit-view.component";
import { PostWithAuthorComponent } from "./blog-posts/post-with-author.component";
import { HomeComponent } from "./home/home.component";

export const AppRoutes: DeepLinkingRoute[] = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
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
        { name: 'editLink', type: 'string' }
      ]
    }
  },
  {
    path: 'edit/:apiName/:uri',
    component: DeepLinkingWrapperComponent,
    wrappedComponent: RESTworldEditViewComponent,
    deepLinking: {
      params: [
        { name: 'apiName', type: 'string' },
        { name: 'uri', type: 'string' }
      ]
    }
  },
  {
    path: 'postWithAuthor/:apiName/:uri',
    component: DeepLinkingWrapperComponent,
    wrappedComponent: PostWithAuthorComponent,
    deepLinking: {
      params: [
        { name: 'apiName', type: 'string' },
        { name: 'uri', type: 'string' }
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
        routerLink: ['list', 'ExampleBlog', 'MyEx:Post']
      }]
  },
  {
    label: "Examples with custom templates",
    items: [
      {
        label: "Posts with author",
        routerLink: ['list', 'ExampleBlog', 'MyEx:Post'],
        queryParams: { editLink: '/postWithAuthor'}
      }
    ]
  }];
