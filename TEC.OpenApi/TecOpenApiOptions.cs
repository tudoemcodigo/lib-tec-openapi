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

    /// <summary>
    /// Nome do documento OpenAPI registrado no ASP.NET Core. É o mesmo nome que o <c>MapTecScalar</c> registra no Scalar.
    /// Não pode ser vazio.
    /// </summary>
    public string DocumentName { get; set; } = "v1";

    /// <summary>Se habilitado, registra o esquema de segurança JWT Bearer (RFC 6750).</summary>
    public bool EnableJwtBearer { get; set; } = true;

    /// <summary>Nome do esquema de segurança JWT Bearer.</summary>
    public string JwtBearerSchemeName { get; set; } = "Bearer";

    /// <summary>Descrição explicativa do formato do token JWT.</summary>
    public string JwtBearerDescription { get; set; } = "Autenticação via token JWT Bearer. Exemplo: 'Bearer {seu_token}'";

    /// <summary>
    /// URL opcional de descoberta OpenID Connect (ex.: realm do Keycloak). Precisa ser uma URL absoluta HTTPS, sem usuário
    /// e senha embutidos; valor inválido derruba a subida em <c>AddTecOpenApi</c>.
    /// </summary>
    public string? OpenIdConnectUrl { get; set; }

    /// <summary>Nome do contato responsável pela API. Padrão: nenhum (o documento sai sem contato).</summary>
    public string? ContactName { get; set; }

    /// <summary>E-mail de contato da equipe responsável. Padrão: nenhum.</summary>
    public string? ContactEmail { get; set; }

    /// <summary>Nome da licença de uso da API. Padrão: nenhum (o documento sai sem licença).</summary>
    public string? LicenseName { get; set; }

    /// <summary>URL da licença de uso. Só entra no documento junto com <see cref="LicenseName"/>.</summary>
    public Uri? LicenseUrl { get; set; }

    /// <summary>Prefixo da rota onde a documentação interativa Scalar será exposta.</summary>
    public string ScalarRoutePrefix { get; set; } = "docs";

    /// <summary>Título da página da documentação interativa Scalar.</summary>
    public string ScalarTitle { get; set; } = "Referência da API | Tudo em Código";

    /// <summary>Tema visual da documentação interativa Scalar.</summary>
    public ScalarTheme ScalarTheme { get; set; } = ScalarTheme.Mars;

    /// <summary>
    /// Só <b>documenta</b>: marca no documento OpenAPI todas as operações sem <c>[AllowAnonymous]</c> como exigindo o esquema
    /// JWT Bearer (com 401 e 403), mesmo sem <c>[Authorize]</c>. Requer <see cref="EnableJwtBearer"/>. Não aplica
    /// autorização nenhuma às rotas da API: proteja-as com <c>RequireAuthorization()</c> ou <c>[Authorize]</c>.
    /// </summary>
    public bool RequireAuthorizationByDefault { get; set; }

    /// <summary>
    /// Mapeia o documento e a interface fora de Development. Padrão: <c>false</c>: em qualquer outro ambiente
    /// (Production, Staging...) os métodos <c>MapTec*</c> não mapeiam nada e registram um aviso. Ligar é decisão explícita;
    /// combine com <see cref="RequireDocumentationAuthorization"/> ou <see cref="DocumentationAuthorizationPolicy"/>.
    /// </summary>
    public bool ExposeOutsideDevelopment { get; set; }

    /// <summary>
    /// Exige usuário autorizado pela política padrão (<c>RequireAuthorization()</c>) no documento e na interface, em qualquer
    /// ambiente. A aplicação precisa de <c>UseAuthentication</c> e <c>UseAuthorization</c>.
    /// </summary>
    public bool RequireDocumentationAuthorization { get; set; }

    /// <summary>
    /// Nome da política de autorização aplicada ao documento e à interface, em qualquer ambiente. Quando definido, vale no
    /// lugar da política padrão (implica <see cref="RequireDocumentationAuthorization"/>). A política precisa estar
    /// registrada em <c>AddAuthorization</c>. Não pode ser só espaços.
    /// </summary>
    public string? DocumentationAuthorizationPolicy { get; set; }
}
