# CHANGELOG.md

Sequência de versões do componente.

## [0.0.1] - 2026-10-25

- **Adicionado**: Implementação inicial do `TEC.OpenApi` com suporte a OpenAPI nativo do ASP.NET Core e interface interativa Scalar.
- **Adicionado**: `TecOpenApiOptions` com parametrização de metadados corporativos, esquema de segurança JWT Bearer e temas visuais do Scalar.
- **Adicionado**: Métodos de extensão `AddTecOpenApi()`, `MapTecOpenApi()`, `MapTecScalar()` e `MapTecApiDocs()`.
- **Adicionado**: Transformers canônicos `TecOpenApiDocumentTransformer` e `TecAuthorizeOperationTransformer`.
- **Adicionado**: Suporte a multi-targeting `net8.0` e `net10.0` com compatibilidade Native AOT.
- **Adicionado**: Suíte de testes unitários com TUnit.
