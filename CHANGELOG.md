# CHANGELOG.md

Sequência de versões do componente.

## [0.0.1] - 2026-10-30

- **Adicionado**: Implementação inicial do `TEC.OpenApi` com suporte a OpenAPI nativo do ASP.NET Core e interface interativa Scalar.
- **Adicionado**: Alvo único `net10.0` com análise de Native AOT no build; o documento OpenAPI e os transformers valem sempre, sem `#if` de TFM. O pacote não depende de nenhum `TEC.*`.
- **Adicionado**: `TecOpenApiOptions` com parametrização de metadados corporativos, esquema de segurança JWT Bearer e temas visuais do Scalar. `ContactName`, `ContactEmail`, `LicenseName` e `LicenseUrl` têm padrão `null`: contato e licença só entram no documento se o consumidor definir. `RequireAuthorizationByDefault` só documenta, não aplica autorização às rotas.
- **Adicionado**: Métodos de extensão `AddTecOpenApi()`, `MapTecOpenApi()`, `MapTecScalar()` e `MapTecApiDocs()`. Os `MapTec*` devolvem `IEndpointConventionBuilder`, então dá para encadear `.RequireAuthorization()`; chamá-los sem `AddTecOpenApi()` lança `InvalidOperationException`. `MapTecScalar()` registra `DocumentName` no Scalar, e com nome diferente de `v1` a interface já abre o documento certo.
- **Adicionado**: Guarda de ambiente: fora de Development, os `MapTec*` não mapeiam nada sem `ExposeOutsideDevelopment = true` e registram um aviso (EventId 5001).
- **Adicionado**: `RequireDocumentationAuthorization` e `DocumentationAuthorizationPolicy`, aplicados ao documento e à interface Scalar.
- **Adicionado**: Validação em `AddTecOpenApi()`: `OpenIdConnectUrl` precisa ser URL absoluta HTTPS, sem usuário e senha; valor inválido derruba a subida. `DocumentName` vazio e `DocumentationAuthorizationPolicy` só com espaços também.
- **Adicionado**: Transformers canônicos `TecOpenApiDocumentTransformer` e `TecAuthorizeOperationTransformer`. O requisito de segurança das operações protegidas sai como `"security": [{"Bearer": []}]` (a referência ao esquema `Bearer` leva o documento hospedeiro).
- **Adicionado**: Suíte de testes unitários com TUnit.
