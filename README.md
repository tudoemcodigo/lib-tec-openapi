<div align="center">

<img src="https://www.tudoemcodigo.com/marca/PNG/tudoemcodigo-icone-512.png" alt="TEC.OpenApi" width="120" />

# 📖 TEC.OpenApi

**Documentação OpenAPI nativa e interface interativa Scalar no padrão corporativo Tudo em Código: metadados, esquemas JWT Bearer e temas customizados.**

Scalar · OpenAPI Nativo (.NET 10 / ASP.NET Core) · JWT Bearer RFC 6750 · OpenID Connect · .NET 8 e 10 · Native AOT

[![CI](https://github.com/tudoemcodigo/lib-tec-openapi/actions/workflows/ci.yml/badge.svg)](https://github.com/tudoemcodigo/lib-tec-openapi/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-512BD4?logo=dotnet&logoColor=white)](#-compatibilidade)
[![Native AOT](https://img.shields.io/badge/Native%20AOT-compat%C3%ADvel-512BD4)](#-compatibilidade)
[![Versão](https://img.shields.io/badge/vers%C3%A3o-0.0.1-blue)](CHANGELOG.md)
[![Licença: Apache 2.0](https://img.shields.io/badge/licen%C3%A7a-Apache%202.0-blue)](LICENSE)

[📥 Instalação](#-instalação) · [🚀 Início rápido](#-início-rápido) · [📝 Changelog](CHANGELOG.md) · [📄 Licença](LICENSE)

</div>

---

## 📑 Sumário

- [✨ Por que usar](#-por-que-usar)
- [📦 Pacotes](#-pacotes)
- [📥 Instalação](#-instalação)
- [🚀 Início rápido](#-início-rápido)
- [⚙️ Configuração avançada](#️-configuração-avançada)
- [⚡ Compatibilidade](#-compatibilidade)
- [📄 Licença](#-licença)

---

## ✨ Por que usar

| Sem o TEC.OpenApi | Com o TEC.OpenApi |
|---|---|
| Configuração manual e verbosa de OpenAPI e Swagger em cada microsserviço | Linha única `AddTecOpenApi()` e `MapTecApiDocs()` |
| Swagger UI pesado, lento e visualmente defasado | **Scalar**: interface interativa ultrarrápida, moderna e responsiva |
| Falta de padronização nos metadados, termos, licença e segurança de cada API | Injeção centralizada de informações corporativas, termos e licenças |
| Configuração repetitiva do esquema de segurança JWT Bearer e OpenID Connect | Configuração automática de `Bearer` e botões de autenticação no Scalar |
| Incompatibilidade com Native AOT por dependências de reflexão em tempo de execução | Totalmente compatível com Native AOT e trims |

---

## 📦 Pacotes

| Pacote | O que contém | Onde usar |
|---|---|---|
| `TEC.OpenApi` | Extensões para DI e mapeamento de rotas OpenAPI e Scalar, transformers corporativos de documento e operações | Em APIs ASP.NET Core (`Program.cs`) |

---

## 📥 Instalação

### Central Package Management (`Directory.Packages.props`)

```xml
<ItemGroup>
  <PackageVersion Include="TEC.OpenApi" Version="0.0.1" />
</ItemGroup>
```

No projeto Web API (`*.csproj`):

```xml
<ItemGroup>
  <PackageReference Include="TEC.OpenApi" />
</ItemGroup>
```

---

## 🚀 Início rápido

No seu `Program.cs`:

```csharp
using TEC.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Registra OpenAPI e Scalar com os padrões da Tudo em Código
builder.Services.AddTecOpenApi(options =>
{
    options.Title = "Template Vendas API";
    options.Description = "API de gerenciamento de clientes, pedidos e itens.";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Mapeia /openapi/v1.json e a interface interativa em /docs
    app.MapTecApiDocs();
}

app.Run();
```

Abra seu navegador em `/docs` (ou `/docs/v1`) para interagir com a interface Scalar da sua API!

---

## ⚙️ Configuração avançada

```csharp
builder.Services.AddTecOpenApi(options =>
{
    options.Title = "E-Commerce Gateway";
    options.Version = "v1";
    options.Description = "Serviços de vendas e checkout Tudo em Código";
    options.ScalarRoutePrefix = "documentacao";
    options.ScalarTheme = ScalarTheme.Moon;
    options.EnableJwtBearer = true;
    options.JwtBearerDescription = "Informe o token JWT obtido no Keycloak corporativo";
    options.OpenIdConnectUrl = "https://auth.minhaempresa.com.br/realms/corporativo/.well-known/openid-configuration";
});
```

---

## ⚡ Compatibilidade

- **Runtimes:** .NET 8.0 e .NET 10.0 (LTS/STS).
- **Trimming / Native AOT:** Compatível.
- **Ambientes:** Windows, Linux e macOS (x64 e ARM64).

---

## 📄 Licença

Distribuído sob a licença [Apache 2.0](LICENSE).
