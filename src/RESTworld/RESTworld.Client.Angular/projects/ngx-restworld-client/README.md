# Ngx-RESTworld-Client
An Angular client to consume a RESTworld service that serves application/hal+json.
It works best when paired with a RESTworld backend, but can work with any HAL compatible backend.

## Components
The main components are the `restworld-list-view` and the `restworld-edit-view` These serve as the main views for the data in your application.
They both have `@Input()` declarations to tell them which resources to serve.

## Services
The main service is the `restworld-client-collection`. It contains a map of API-names and `restworld-client`s.
You can request the corresponding client for each API name (and that is exactly what the components do).
There is also the `settings` service which provides the mapping between API-name and API-root-uri.

## Usage
1. Define your routes and your menu. You can either use the very good `DeepLinkingWrapperComponent` from @jdrks/ngx-deep-linking to directly route to your components and extract parameters from the routes yourself. When using custom components, you can inject templates to make everything look like you want.
```
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
```

2. Provide a `/settings` endpoint which returns the API to URL mapping.
It must return with a json response reassembling the `ClientSettings` interface.

3. Enjoy you flexible frontend.