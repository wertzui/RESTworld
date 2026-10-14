# Choosing a Pipeline

RESTworld is highly configurable and offers multiple pipelines for different scenarios. If you are unsure which option fits best, start with the [Pipeline Overview](pipeline-overview.md) for context, then use the flowchart below to pick the right implementation strategy.

```mermaid
flowchart TD
    A((Start)) --> B{Do you want to read one resource or a list of resources mapped to entities from a database?}
    B -->|No| C{Do you want to return a resource?}
    C -->|No| E{Do you want to return HAL?}
    E -->|No| M[Create a custom controller. Derive from Controller.]
    E -->|Yes| N[Create a custom controller. Derive from HalControllerBase.]
    C -->|Yes| F[Create a custom controller. Derive from RestControllerBase.]
    B -->|Yes| D{Do you want to modify resources}
    D -->|No| G{Do you need more functionality than just mapping?}
    D -->|Yes| H{Do you need more functionality than just mapping?}
    F --> J{Do you need anything from a database?}
    J -->|No| K[Create your own service. Derive from ServiceBase.]
    J -->|Yes| L[Create your own service. Derive from DbServiceBase.]
    G -->|No| I[Use a ReadPipeline.]
    G -->|Yes| O{Do you need more than the read endpoints?}
    H -->|No| P[Use a CrudPipeline.]
    H -->|Yes| Q{Do you need more than the CRUD endpoints?}
    O -->|No| R[Use a ReadPipelineWithCustomService. Derive your service from ReadServiceBase and override the *InternalAsync methods.]
    O -->|Yes| S[Create a custom controller. Derive from ReadController or RestControllerBase. Derive your service from ReadServiceBase, override the *InternalAsync methods, and add methods for your extra endpoint.]
    Q -->|No| T[Use a CrudPipelineWithCustomService. Derive your service from CrudServiceBase and override the *InternalAsync methods.]
    Q -->|Yes| U[Create a custom controller. Derive from CrudController or RestControllerBase. Derive your service from CrudServiceBase, override the *InternalAsync methods, and add methods for your extra endpoint.]
```

Notes:

- Controllers are discovered automatically by ASP.NET Core. Register only the service, mapper and authorization handler. See [Custom Controllers](custom-controllers.md) and [Services](services.md).
- The `...WithCustomService` registration methods have no `ApiVersion` parameter. A versioned endpoint that needs a custom service uses a custom controller with `[ApiVersion]`.
