# Instruções para agentes — TEC.OpenApi

Este arquivo complementa as instruções herdadas de `AGENT_BASE.md`. O TEC.OpenApi padroniza a documentação das APIs ASP.NET Core: documento OpenAPI nativo com metadados e esquema JWT Bearer, interface interativa Scalar e proteção da documentação (guarda de ambiente e autorização).

## Escopo e segurança de alterações

- O escopo padrão é este repositório. Não altere outros repositórios `TEC.*`, não publique pacotes/tags/releases e não rode sincronizações que escrevam fora daqui sem pedido explícito.
- Antes de editar, confira `git status` e `git diff`. Preserve alterações não commitadas; não use reset, checkout destrutivo ou limpeza para descartar trabalho existente.
- Mudanças na assinatura dos métodos `AddTecOpenApi`/`MapTec*`, na guarda de ambiente, na autorização da documentação, nos transformers ou nos alvos .NET exigem revisão de compatibilidade, documentação, changelog e consumidores.
- Testes devem atravessar contratos públicos e verificar o documento real servido (`/openapi/{nome}.json`) e a interface (`/docs/{nome}`), não só que a rota responde. Para mudanças de comportamento, cubra entradas inválidas, ambiente, autorização (401/403) e falhas na subida.
- O documento expõe a superfície da API: fechado por padrão (nada mapeado fora de Development sem `ExposeOutsideDevelopment`). Não exponha segredos, PII ou conteúdo controlado pelo usuário em exceções, logs, URLs, no documento ou em sumários.
- Configuração inválida (`OpenIdConnectUrl` que não seja HTTPS absoluta, `DocumentName` vazio, política só com espaços) derruba a subida em `AddTecOpenApi`, nunca a primeira requisição.

## Mapa da implementação

- `TEC.OpenApi/`: biblioteca `net10.0`, dependências `Microsoft.AspNetCore.OpenApi` e `Scalar.AspNetCore`.
  - `TecOpenApiExtensions.cs`: `AddTecOpenApi` (valida as opções e registra o documento com os transformers), `MapTecOpenApi`, `MapTecScalar` e `MapTecApiDocs` (devolvem `IEndpointConventionBuilder`; guarda de ambiente e autorização das opções).
  - `TecOpenApiOptions.cs`: metadados, esquema JWT/OpenID, Scalar, `ExposeOutsideDevelopment`, `RequireDocumentationAuthorization`, `DocumentationAuthorizationPolicy`.
  - `Transformers/`: `TecOpenApiDocumentTransformer` (info, contato, licença, esquemas) e `TecAuthorizeOperationTransformer` (requisito `Bearer`, 401 e 403 nas operações protegidas).
  - `Internal/`: `TecEndpointConventionBuilder` (repassa convenções ao documento e à interface; vazio quando a guarda bloqueia) e `TecOpenApiLog` (EventId 5001).
- `TEC.OpenApi.Tests/`: testes TUnit de opções, transformers e integração com `TestServer` (autenticação de teste por header).
- `build/`: props/targets compartilhados (canônicos do `tec-workflows`; não edite aqui).

## Contratos relevantes

- `Directory.Build.props` é a fonte única de versão do pacote. Não introduza versões nos `.csproj`.
- O pacote não depende de nenhum `TEC.*`. Ao passar a depender, use `<TecReference Include="..." />` e declare a versão publicada no `Directory.Packages.props`.
- Contato e licença só entram no documento se o consumidor definir (padrões `null`). `RequireAuthorizationByDefault` só documenta; não protege rotas.
- Os arquivos estáticos do Scalar (`scalar.js`) não recebem as convenções de autorização: são código da biblioteca, sem dados da API.
- Workflows dos consumidores chamam `tudoemcodigo/tec-workflows` (`@v1`); confira as tags realmente publicadas antes de alterar referências.
- A licença é Apache-2.0. Mantenha `LICENSE`, `NOTICE`, badge/README e `<PackageLicenseExpression>` coerentes.

## Documentação

- `README.md` e `CHANGELOG.md` têm conteúdo próprio do TEC.OpenApi: siga o padrão do template, mantenha a marca Tudo em Código e não substitua o conteúdo específico por placeholders genéricos.
- Atualize a tabela de opções e a seção "Proteção da documentação" do `README.md`, e o `CHANGELOG.md` (`[Não lançado]`), quando uma mudança alterar comportamento ou contrato.

## Verificação local

```powershell
dotnet restore TEC.OpenApi.slnx
dotnet build TEC.OpenApi.slnx -c Release
dotnet test --solution TEC.OpenApi.slnx -c Release
git diff --check
```

O CI também valida convenções, pacote e workflows reutilizáveis. Não afirme que o CI está funcional apenas com build/testes locais: confirme que as tags chamadas existem e que os `permissions` do chamador incluem o mínimo exigido pelo workflow reutilizável.
