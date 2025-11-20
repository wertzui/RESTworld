using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// This will read the connection string from the appsettings.json or secrets.json file or get it from an environment variable
// In this example it is stored in appsettings.json, because it just connects to lodaldb without a password.
var database = builder.AddConnectionString("BlogDatabase");

// Instead of using localDb, you can also use a SQL Server instance which is automatically created and started in a docker container.
//var database = builder.AddSqlServer("ExampleBlog-SQL-Server")
//    .AddDatabase("BlogDatabase");


// Add the backend API as a service
var apiService = builder.AddProject<ExampleBlog>(nameof(ExampleBlog))
    // Add a reference to the database
    // because the database has the name "BlogDatabase", this will inject an environment variable ConnectionStrings__BlogDatabase=Server=localhost;Database=BlogDatabase;Integrated Security=True
    .WithReference(database)
    // Every RESTworld project exposes three health checks which can be used in Kubernetes.
    // When using builder.AddDbContextFactoryWithDefaults<MyDb>() a health check for the database is automatically added.
    .WithKubernetesHealthProbes()
    .WithUrls(context =>
    {
        if (!context.Resource.TryGetEndpoints(out var endpoints))
            return;

        foreach (var endpoint in endpoints)
        {
            var uri = endpoint.AllocatedEndpoint?.UriString;
            if (uri is null)
                continue;

            context.Urls.Add(new ResourceUrlAnnotation { Url = $"https://hal-explorer.com/#uri={uri}", DisplayText = $"HAL-Explorer ({endpoint.Name})" });
            context.Urls.Add(new ResourceUrlAnnotation { Url = $"{uri}/swagger", DisplayText = $"Swagger ({endpoint.Name})" });
        }

    });

// Add the Frontend which hosts the Angular client
var frontendService = builder.AddProject<ExampleBlog_Client_Angular>("ExampleBlog-Client-Angular")
    // Add a reference to the backend API
    // This will inject an environment variable services__ExampleBlog__https__0=https://localhost:5432
    // The SettingsController from RestWorld.Client.AspNetCore will read this environment variable and replace the API URLs with the value,
    // so that the Angular client can call the API
    .WithReference(apiService)
    // Every RESTworld project exposes three health checks which can be used in Kubernetes.
    // For frontend projects this just means that the application is running
    .WithKubernetesHealthProbes()
    .WithIconName("TabDesktop");

// Add the MCP Inspector which can be used to inspect the mcp service provided by the API
var mcpInspector = builder.AddMcpInspector("McpInspector", new McpInspectorOptions { InspectorVersion = "0.17.2" })
    .WithMcpServer(apiService);

var app = builder.Build();

await app.RunAsync();
