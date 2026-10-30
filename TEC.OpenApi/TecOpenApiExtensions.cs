using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using TEC.OpenApi.Internal;
using TEC.OpenApi.Transformers;

namespace TEC.OpenApi;

/// <summary>
/// Métodos de extensão para registro e mapeamento canônico de OpenAPI e Scalar.
/// </summary>
public static class TecOpenApiExtensions
{
    /// <summary>
    /// Registra os serviços de geração de documentação OpenAPI e configurações do Scalar. Valida as opções aqui: valor
    /// inválido (ex.: <see cref="TecOpenApiOptions.OpenIdConnectUrl"/> que não seja URL absoluta HTTPS) derruba a subida
    /// com <see cref="InvalidOperationException"/>, nunca a primeira requisição ao documento.
    /// </summary>
    /// <param name="services">A coleção de serviços da aplicação.</param>
    /// <param name="configure">Ação opcional para customizar as opções do <see cref="TecOpenApiOptions"/>.</param>
    /// <returns>A mesma coleção de serviços para encadeamento fluente.</returns>
    /// <exception cref="InvalidOperationException">Opções inválidas.</exception>
    public static IServiceCollection AddTecOpenApi(
        this IServiceCollection services,
        Action<TecOpenApiOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new TecOpenApiOptions();
        configure?.Invoke(options);
        Validate(options);

        services.AddSingleton(options);
        services.AddSingleton<IOptions<TecOpenApiOptions>>(new OptionsWrapper<TecOpenApiOptions>(options));

        services.AddOpenApi(options.DocumentName, openApiOptions =>
        {
            openApiOptions.AddDocumentTransformer(new TecOpenApiDocumentTransformer(options));
            if (options.EnableJwtBearer)
            {
                openApiOptions.AddOperationTransformer(new TecAuthorizeOperationTransformer(options));
            }
        });

        return services;
    }

    /// <summary>
    /// Mapeia o endpoint do documento OpenAPI em formato JSON (ex.: <c>/openapi/v1.json</c>). Fora de Development só mapeia
    /// com <see cref="TecOpenApiOptions.ExposeOutsideDevelopment"/>; a autorização configurada nas opções é aplicada.
    /// </summary>
    /// <param name="app">O roteador de endpoints da aplicação.</param>
    /// <param name="pattern">Padrão opcional de rota (padrão: <c>/openapi/{documentName}.json</c>).</param>
    /// <returns>
    /// O construtor de convenções do endpoint (ex.: <c>.RequireAuthorization("politica")</c>). Quando o ambiente bloqueia o
    /// mapeamento, é um construtor vazio: as convenções aplicadas nele não têm efeito.
    /// </returns>
    /// <exception cref="InvalidOperationException"><see cref="AddTecOpenApi"/> não foi chamado.</exception>
    public static IEndpointConventionBuilder MapTecOpenApi(
        this IEndpointRouteBuilder app,
        string? pattern = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = GetOptions(app);
        return CanMap(app, options)
            ? MapOpenApiCore(app, options, pattern)
            : TecEndpointConventionBuilder.Empty;
    }

    /// <summary>
    /// Mapeia a interface interativa de documentação Scalar (ex.: <c>/docs</c>) apontando para o documento
    /// <see cref="TecOpenApiOptions.DocumentName"/>. Fora de Development só mapeia com
    /// <see cref="TecOpenApiOptions.ExposeOutsideDevelopment"/>; a autorização configurada nas opções é aplicada.
    /// </summary>
    /// <param name="app">O roteador de endpoints da aplicação.</param>
    /// <param name="configureScalar">Ação opcional de customização adicional das opções do Scalar.</param>
    /// <returns>
    /// O construtor de convenções dos endpoints do Scalar. Quando o ambiente bloqueia o mapeamento, é um construtor vazio.
    /// </returns>
    /// <exception cref="InvalidOperationException"><see cref="AddTecOpenApi"/> não foi chamado.</exception>
    public static IEndpointConventionBuilder MapTecScalar(
        this IEndpointRouteBuilder app,
        Action<ScalarOptions>? configureScalar = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = GetOptions(app);
        return CanMap(app, options)
            ? MapScalarCore(app, options, configureScalar)
            : TecEndpointConventionBuilder.Empty;
    }

    /// <summary>
    /// Mapeia o documento JSON do OpenAPI e a interface interativa Scalar de forma combinada, com a mesma guarda de
    /// ambiente e a mesma autorização de <see cref="MapTecOpenApi"/> e <see cref="MapTecScalar"/>.
    /// </summary>
    /// <param name="app">O roteador de endpoints da aplicação.</param>
    /// <param name="configureScalar">Ação opcional de customização adicional das opções do Scalar.</param>
    /// <returns>
    /// Um construtor de convenções que repassa cada convenção ao documento e à interface (ex.: <c>.RequireAuthorization()</c>
    /// protege os dois). Quando o ambiente bloqueia o mapeamento, é um construtor vazio.
    /// </returns>
    /// <exception cref="InvalidOperationException"><see cref="AddTecOpenApi"/> não foi chamado.</exception>
    public static IEndpointConventionBuilder MapTecApiDocs(
        this IEndpointRouteBuilder app,
        Action<ScalarOptions>? configureScalar = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = GetOptions(app);
        if (!CanMap(app, options))
            return TecEndpointConventionBuilder.Empty;

        return new TecEndpointConventionBuilder(
            [MapOpenApiCore(app, options, pattern: null), MapScalarCore(app, options, configureScalar)]);
    }

    private static IEndpointConventionBuilder MapOpenApiCore(IEndpointRouteBuilder app, TecOpenApiOptions options, string? pattern)
    {
        var builder = string.IsNullOrWhiteSpace(pattern) ? app.MapOpenApi() : app.MapOpenApi(pattern);
        return ApplyAuthorization(builder, options);
    }

    private static IEndpointConventionBuilder MapScalarCore(
        IEndpointRouteBuilder app, TecOpenApiOptions options, Action<ScalarOptions>? configureScalar)
    {
        var builder = app.MapScalarApiReference(options.ScalarRoutePrefix, scalarOptions =>
        {
            scalarOptions
                .WithTitle(options.ScalarTitle)
                .WithTheme(options.ScalarTheme)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

            // Sem isso o Scalar procura "v1": com outro DocumentName a interface abriria sem documento
            scalarOptions.AddDocument(options.DocumentName);

            if (options.EnableJwtBearer)
            {
                scalarOptions.AddPreferredSecuritySchemes(options.JwtBearerSchemeName);
            }

            configureScalar?.Invoke(scalarOptions);
        });

        return ApplyAuthorization(builder, options);
    }

    private static IEndpointConventionBuilder ApplyAuthorization(IEndpointConventionBuilder builder, TecOpenApiOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.DocumentationAuthorizationPolicy))
            return builder.RequireAuthorization(options.DocumentationAuthorizationPolicy);

        return options.RequireDocumentationAuthorization ? builder.RequireAuthorization() : builder;
    }

    private static TecOpenApiOptions GetOptions(IEndpointRouteBuilder app) =>
        app.ServiceProvider.GetService<TecOpenApiOptions>()
            ?? throw new InvalidOperationException("Chame services.AddTecOpenApi() antes de mapear a documentação (MapTecOpenApi, MapTecScalar ou MapTecApiDocs).");

    /// <summary>Guarda de ambiente: sem <see cref="IHostEnvironment"/> o ambiente é tratado como não Development (falha fechada).</summary>
    private static bool CanMap(IEndpointRouteBuilder app, TecOpenApiOptions options)
    {
        var environment = app.ServiceProvider.GetService<IHostEnvironment>();
        if (environment?.IsDevelopment() == true || options.ExposeOutsideDevelopment)
            return true;

        var logger = app.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("TEC.OpenApi");
        if (logger is not null)
            TecOpenApiLog.DocumentationNotMapped(logger, environment?.EnvironmentName ?? "(desconhecido)");

        return false;
    }

    private static void Validate(TecOpenApiOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.DocumentName))
            throw new InvalidOperationException("TecOpenApiOptions.DocumentName não pode ser vazio.");

        // O valor não entra na mensagem: a URL pode carregar dados que não devem ir para o log
        if (!string.IsNullOrWhiteSpace(options.OpenIdConnectUrl)
            && (!Uri.TryCreate(options.OpenIdConnectUrl, UriKind.Absolute, out var oidc)
                || oidc.Scheme != Uri.UriSchemeHttps
                || oidc.UserInfo.Length > 0))
        {
            throw new InvalidOperationException(
                "TecOpenApiOptions.OpenIdConnectUrl deve ser uma URL absoluta HTTPS, sem usuário e senha " +
                "(ex.: https://auth.exemplo.com/realms/corporativo/.well-known/openid-configuration).");
        }

        if (options.DocumentationAuthorizationPolicy is { } policy && string.IsNullOrWhiteSpace(policy))
            throw new InvalidOperationException("TecOpenApiOptions.DocumentationAuthorizationPolicy não pode ser vazio: informe o nome da política ou deixe null.");
    }
}
