using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TEC.OpenApi.Transformers;

/// <summary>
/// Injeta metadados corporativos e esquemas de autenticação JWT/Keycloak no documento OpenAPI.
/// </summary>
public sealed class TecOpenApiDocumentTransformer(TecOpenApiOptions options) : IOpenApiDocumentTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info ??= new OpenApiInfo();
        document.Info.Title = options.Title;
        document.Info.Version = options.Version;
        document.Info.Description = options.Description;

        if (!string.IsNullOrWhiteSpace(options.ContactName) || !string.IsNullOrWhiteSpace(options.ContactEmail))
        {
            document.Info.Contact = new OpenApiContact
            {
                Name = options.ContactName,
                Email = options.ContactEmail
            };
        }

        if (!string.IsNullOrWhiteSpace(options.LicenseName))
        {
            document.Info.License = new OpenApiLicense
            {
                Name = options.LicenseName,
                Url = options.LicenseUrl
            };
        }

        if (options.EnableJwtBearer)
        {
            var components = document.Components ??= new OpenApiComponents();
            var schemes = components.SecuritySchemes ??= new System.Collections.Generic.Dictionary<string, IOpenApiSecurityScheme>();

            schemes[options.JwtBearerSchemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = options.JwtBearerDescription
            };

            if (!string.IsNullOrWhiteSpace(options.OpenIdConnectUrl))
            {
                schemes["OpenID"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OpenIdConnect,
                    OpenIdConnectUrl = new System.Uri(options.OpenIdConnectUrl),
                    Description = "Descoberta automática OpenID Connect (ex.: Keycloak)"
                };
            }
        }

        return Task.CompletedTask;
    }
}
