using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
#if NET9_0_OR_GREATER || NET10_0_OR_GREATER
using Microsoft.AspNetCore.OpenApi;
using TEC.OpenApi.Transformers;
#endif

namespace TEC.OpenApi;

/// <summary>
/// Métodos de extensão para registro e mapeamento canônico de OpenAPI e Scalar.
/// </summary>
public static class TecOpenApiExtensions
{
    /// <summary>
    /// Registra os serviços de geração de documentação OpenAPI e configurações do Scalar.
    /// </summary>
    /// <param name="services">A coleção de serviços da aplicação.</param>
    /// <param name="configure">Ação opcional para customizar as opções do <see cref="TecOpenApiOptions"/>.</param>
    /// <returns>A mesma coleção de serviços para encadeamento fluente.</returns>
    public static IServiceCollection AddTecOpenApi(
        this IServiceCollection services,
        Action<TecOpenApiOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new TecOpenApiOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IOptions<TecOpenApiOptions>>(new OptionsWrapper<TecOpenApiOptions>(options));

#if NET9_0_OR_GREATER || NET10_0_OR_GREATER
        services.AddOpenApi(options.DocumentName, openApiOptions =>
        {
            openApiOptions.AddDocumentTransformer(new TecOpenApiDocumentTransformer(options));
            if (options.EnableJwtBearer)
            {
                openApiOptions.AddOperationTransformer(new TecAuthorizeOperationTransformer(options));
            }
        });
#endif

        return services;
    }

    /// <summary>
    /// Mapeia o endpoint do documento OpenAPI em formato JSON (ex.: <c>/openapi/v1.json</c>).
    /// </summary>
    /// <param name="app">O roteador de endpoints da aplicação.</param>
    /// <param name="pattern">Padrão opcional de rota (padrão: <c>/openapi/{documentName}.json</c>).</param>
    /// <returns>O roteador de endpoints para encadeamento.</returns>
    public static IEndpointRouteBuilder MapTecOpenApi(
        this IEndpointRouteBuilder app,
        string? pattern = null)
    {
        ArgumentNullException.ThrowIfNull(app);

#if NET9_0_OR_GREATER || NET10_0_OR_GREATER
        if (string.IsNullOrWhiteSpace(pattern))
        {
            app.MapOpenApi();
        }
        else
        {
            app.MapOpenApi(pattern);
        }
#endif

        return app;
    }

    /// <summary>
    /// Mapeia a interface interativa de documentação Scalar (ex.: <c>/docs</c>).
    /// </summary>
    /// <param name="app">O roteador de endpoints da aplicação.</param>
    /// <param name="configureScalar">Ação opcional de customização adicional das opções do Scalar.</param>
    /// <returns>O roteador de endpoints para encadeamento.</returns>
    public static IEndpointRouteBuilder MapTecScalar(
        this IEndpointRouteBuilder app,
        Action<ScalarOptions>? configureScalar = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = app.ServiceProvider.GetService<TecOpenApiOptions>() ?? new TecOpenApiOptions();

        app.MapScalarApiReference(options.ScalarRoutePrefix, scalarOptions =>
        {
            scalarOptions
                .WithTitle(options.ScalarTitle)
                .WithTheme(options.ScalarTheme)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

            if (options.EnableJwtBearer)
            {
                scalarOptions.AddPreferredSecuritySchemes(options.JwtBearerSchemeName);
            }

            configureScalar?.Invoke(scalarOptions);
        });

        return app;
    }

    /// <summary>
    /// Mapeia o documento JSON do OpenAPI e a interface interativa Scalar de forma combinada.
    /// </summary>
    /// <param name="app">O roteador de endpoints da aplicação.</param>
    /// <param name="configureScalar">Ação opcional de customização adicional das opções do Scalar.</param>
    /// <returns>O roteador de endpoints para encadeamento.</returns>
    public static IEndpointRouteBuilder MapTecApiDocs(
        this IEndpointRouteBuilder app,
        Action<ScalarOptions>? configureScalar = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapTecOpenApi();
        app.MapTecScalar(configureScalar);

        return app;
    }
}
