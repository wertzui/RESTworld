# Health and Operations

RESTworld provides built-in health endpoints and works nicely with Aspire to streamline local environments. Use these tools to keep deployments observable and resilient.

## Health endpoints

Three endpoints integrate with Kubernetes probes and other monitoring solutions:

1. `/health/startup` – reports healthy after all `AddDbContextFactoryWithDefaults<TContext>()` contexts have applied pending migrations.
2. `/health/live` – reports healthy once the application starts (no checks configured by default).
3. `/health/ready` – reports healthy when each configured database connection succeeds.

Tag custom health checks with `startup`, `live`, or `ready` to add them to the respective endpoint.

## Local environments with Aspire

Aspire can host your API, Angular frontend, and backing services for an integrated test harness:

```csharp
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddConnectionString("MyDatabase");

// Optional SQL Server container
// var database = builder.AddSqlServer("SQL-Server")
//     .AddDatabase("MyDatabase");

var apiService = builder.AddProject<MyApi>("MyApi")
    .WithReference(database);

// Angular frontend hosted via ASP.NET Core
// Note: OTLP for Angular currently requires gRPC support (see https://github.com/dotnet/aspire/pull/4197)
var frontendService = builder.AddProject<MyApi_Client_Angular>("MyApi-Client-Angular")
    .WithReference(apiService);

var app = builder.Build();

await app.RunAsync();
```

Aspire injects environment variables such as:

- `ConnectionStrings__MyDatabase` – consumed by `services.AddDbContextFactoryWithDefaults<TContext>()` so your DbContext factory picks up the right connection string.
- `services__MyApi__https__0` – picked up by `RESTworld.Client.AspNetCore` to rewrite API URLs for the Angular SPA and the .NET client implementation.

## Next steps

- Wire configuration values using the guidance in [Configuration](configuration.md).
- Monitor your clients' behavior using the techniques in [Angular Client Development](client-development-angular.md) and [.NET Client Development](client-development-dotnet.md).
- Revisit [Pipeline Overview](pipeline-overview.md) when customizing read or CRUD flows.
