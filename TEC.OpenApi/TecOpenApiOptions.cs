using System;
using Scalar.AspNetCore;

namespace TEC.OpenApi;

/// <summary>
/// Opções de configuração para OpenAPI e Scalar no padrão canônico Tudo em Código.
/// </summary>
public sealed class TecOpenApiOptions
{
    /// <summary>Título do documento OpenAPI exibido na interface.</summary>
    public string Title { get; set; } = "API Tudo em Código";

    /// <summary>Versão da API (ex.: "v1").</summary>
    public string Version { get; set; } = "v1";

    /// <summary>Descrição do propósito e escopo da API.</summary>
    public string Description { get; set; } = "Documentação interativa e contratos da API Tudo em Código.";

    /// <summary>Nome do documento OpenAPI registrado no ASP.NET Core.</summary>
    public string DocumentName { get; set; } = "v1";

    /// <summary>Se habilitado, registra o esquema de segurança JWT Bearer (RFC 6750).</summary>
    public bool EnableJwtBearer { get; set; } = true;

    /// <summary>Nome do esquema de segurança JWT Bearer.</summary>
    public string JwtBearerSchemeName { get; set; } = "Bearer";

    /// <summary>Descrição explicativa do formato do token JWT.</summary>
    public string JwtBearerDescription { get; set; } = "Autenticação via token JWT Bearer. Exemplo: 'Bearer {seu_token}'";

    /// <summary>URL opcional de descoberta OpenID Connect (ex.: realm do Keycloak).</summary>
    public string? OpenIdConnectUrl { get; set; }

    /// <summary>Nome do contato responsável pela API.</summary>
    public string? ContactName { get; set; } = "Suporte Tudo em Código";

    /// <summary>E-mail de contato da equipe responsável.</summary>
    public string? ContactEmail { get; set; } = "suporte@tudoemcodigo.com";

    /// <summary>Nome da licença de uso da API.</summary>
    public string? LicenseName { get; set; } = "Apache 2.0";

    /// <summary>URL da licença de uso.</summary>
    public Uri? LicenseUrl { get; set; } = new("https://www.apache.org/licenses/LICENSE-2.0");

    /// <summary>Prefixo da rota onde a documentação interativa Scalar será exposta.</summary>
    public string ScalarRoutePrefix { get; set; } = "docs";

    /// <summary>Título da página da documentação interativa Scalar.</summary>
    public string ScalarTitle { get; set; } = "Referência da API | Tudo em Código";

    /// <summary>Tema visual da documentação interativa Scalar.</summary>
    public ScalarTheme ScalarTheme { get; set; } = ScalarTheme.Mars;

    /// <summary>Se verdadeiro, exige autenticação para todos os endpoints por padrão.</summary>
    public bool RequireAuthorizationByDefault { get; set; }
}
