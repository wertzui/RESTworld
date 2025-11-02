# Angular Client Core Concepts

Understand the REST-first conventions that the `@wertzui/ngx-restworld-client` package relies on before wiring up components or routes.

## Start with HAL discovery

Always call the home endpoint (`/`) before requesting specific resources. Cache discovered routes per session or per day, but avoid hardcoding URLs. Refer to the HAL specification at <https://stateless.group/hal_specification.html> for link semantics.

## Link templating

RESTworld links may include URI templates. Follow RFC 6570 (<https://datatracker.ietf.org/doc/html/rfc6570>) and leverage client libraries to expand templates safely.

## OData queries for list endpoints

List endpoints support the OData query language, allowing filtering, sorting, and pagination on the server side. If you're new to the syntax, start with the <https://www.odata.org/getting-started/basic-tutorial/#queryData> tutorial.

## Using the `/new` endpoint

When creating new entities, fetch the `/new` endpoint first to obtain sensible defaults and guidance for required fields. Present the returned data to users to streamline input forms.

## Batch operations

`POST` (create) and `PUT` (update) endpoints accept either single objects or arrays. Sending batches reduces round trips and keeps operations atomic.

## Versioning from the client perspective

Always advertise supported versions in the `Accept` header, e.g., `application/hal+json; v=42`. Inspect the `api-deprecated-versions` header in responses and alert users when they rely on a deprecated version. Review [Mapping and Versioning](mapping-and-versioning.md) to understand how the server processes these headers.

Continue with [Angular Client Setup](client-development-angular-setup.md) to apply these concepts in an application, or jump to [Angular Client Components](client-development-angular-components.md) to reuse the ready-made UI building blocks.
