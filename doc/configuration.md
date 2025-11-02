# Configuration

RESTworld settings live under the `RESTworld` section in `appsettings.json`. Configure the API and client independently to control pagination, versioning, and endpoint discovery.

## API settings

```json
"RESTworld": {
  "MaxNumberForListEndpoint": 10,
  "Curie": "MyEx",
  "CalculateTotalCountForListEndpoint": true,
  "DisableAuthorization": false,
  "Versioning": {
    "AllowQueryParameterVersioning": false,
    "DefaultVersion": "1.0",
    "ParameterName": "v"
  }
}
```

### Key options

- `MaxNumberForListEndpoint` – caps the number of items returned by list endpoints.
- `Curie` – prefix used for link relations in HAL responses.
- `CalculateTotalCountForListEndpoint` – compute total counts eagerly so clients can parallelize pagination.
- `DisableAuthorization` – toggle for development scenarios (see [Authorization](authorization.md)).
- `Versioning` – controls how RESTworld infers versions from media types or query parameters. `DefaultVersion` accepts either a concrete version (e.g., `"1.0"`) or `"latest"`.

For more context on handling DTO versions, read [Mapping and Versioning](mapping-and-versioning.md).

## Client settings

When consuming an API via `RESTworld.Client.Net` or `@wertzui/ngx-restworld-client`, provide the list of target instances:

```json
"RESTworld": {
  "ClientSettings": {
    "ApiUrls": [
      {
        "Name": "MyApi",
        "Url": "https://myapi.example.com/",
        "Version": 42
      }
    ]
  }
}
```

The client libraries pick the configured API by name and negotiate the declared version through the `Accept` header. Learn how to apply these settings in [Angular Client Development](client-development-angular.md) or [.NET Client Development](client-development-dotnet.md).

## Environment-specific configuration

- Place development-only overrides in `appsettings.Development.json`.
- When deploying with Aspire, environment variables such as `ConnectionStrings__MyDatabase` and `services__MyApi__https__0` are injected automatically. See [Health and Operations](health-and-operations.md) for details.

Continue with [Authorization](authorization.md) to secure your endpoints or dive into [Mapping and Versioning](mapping-and-versioning.md) to finalize your DTO strategy.
