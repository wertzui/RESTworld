# Agent instructions for a RESTworld application

These instructions are for AI agents helping a developer who builds an application **with** the RESTworld libraries (this solution is the reference). Copy this file to the root of your own RESTworld solution.

## Layout

| Project | Contents |
|---|---|
| `*.Data` | Entities, `DbContext`, migrations |
| `*.Common` | DTOs and enums (shared with clients) |
| `*.Business` | Mappers, services, authorization handlers, validators |
| `*` (API) | `Program.cs`, custom controllers, HAL-FORMS customizations |
| `*.Client` / `*.Client.Angular` | ASP.NET Core SPA host and the Angular app |
| `*.Aspire.AppHost` | Local orchestration |

## Rules

- Prefer the built-in pipelines. Add a custom service, then a custom controller, only when the pipeline cannot do the job.
- Use Mapperly for new mappers. AutoMapper is legacy: keep it working in existing code, but migrate it to Mapperly when touching it (ExampleBlog keeps some AutoMapper pipelines on purpose to show the migration path).
- Do not hardcode URLs in clients. Follow HAL links.
- Update DTOs, the mapper and the migration together.
- Do not change `DisableAuthorization` or add anonymous access without being asked.

## Skills

Install them as the `restworld` Copilot plugin (`copilot plugin marketplace add wertzui/RESTworld-AI-plugin`, then `copilot plugin install restworld@restworld`) or copy the folders from `plugins/restworld/skills` of the RESTworld repository into your agent's skills folder.

Start with `restworld-add-entity-endpoint` for any new endpoint. It calls the others in order.

| Task | Skill |
|---|---|
| New solution | `restworld-bootstrap-solution` |
| Entity, `DbContext`, migration | `restworld-create-entity-and-dbcontext` |
| DTOs | `restworld-create-dtos` |
| Mapper | `restworld-create-mapper` |
| Pipeline choice and registration | `restworld-choose-pipeline` |
| Business logic | `restworld-custom-service` |
| Authorization | `restworld-authorization-handler` |
| Validation | `restworld-validation` |
| Custom endpoints | `restworld-custom-controller` |
| New API version | `restworld-add-api-version` |
| HAL-FORMS templates | `restworld-halforms-customization` |
| Swagger, MCP, CSV, JSON Patch, caching | `restworld-api-extras` |
| Tests | `restworld-testing` |
| .NET client | `restworld-dotnet-client` |
| Angular pages | `restworld-angular-add-page` |
| Angular customization | `restworld-angular-custom-components` |

Documentation: https://github.com/wertzui/RESTworld/blob/main/doc/how-to.md
