#if NET9_0_OR_GREATER || NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using TEC.OpenApi.Transformers;

namespace TEC.OpenApi.Tests;

public class TecOpenApiTransformersTests
{
    private sealed class FakeAuthorizeData : IAuthorizeData
    {
        public string? Policy { get; set; }
        public string? Roles { get; set; }
        public string? AuthenticationSchemes { get; set; }
    }

    private static IServiceProvider CreateServices() => new ServiceCollection().BuildServiceProvider();

    [Test]
    public async Task DocumentTransformer_populates_metadata_and_security_schemes()
    {
        var options = new TecOpenApiOptions
        {
            Title = "Sistema de Vendas",
            Version = "v1.2",
            Description = "Documentação da API de vendas",
            ContactName = "Time Backend",
            ContactEmail = "backend@tudoemcodigo.com.br",
            LicenseName = "Proprietary",
            LicenseUrl = new Uri("https://tudoemcodigo.com.br/license"),
            EnableJwtBearer = true,
            JwtBearerSchemeName = "Bearer",
            OpenIdConnectUrl = "https://auth.tudoemcodigo.com.br/.well-known/openid-configuration"
        };

        var transformer = new TecOpenApiDocumentTransformer(options);
        var document = new OpenApiDocument();
        var context = new OpenApiDocumentTransformerContext
        {
            DocumentName = "v1",
            DescriptionGroups = [],
            ApplicationServices = CreateServices()
        };

        await transformer.TransformAsync(document, context, CancellationToken.None);

        await Assert.That(document.Info).IsNotNull();
        await Assert.That(document.Info!.Title).IsEqualTo("Sistema de Vendas");
        await Assert.That(document.Info.Version).IsEqualTo("v1.2");
        await Assert.That(document.Info.Description).IsEqualTo("Documentação da API de vendas");
        await Assert.That(document.Info.Contact?.Name).IsEqualTo("Time Backend");
        await Assert.That(document.Info.Contact?.Email).IsEqualTo("backend@tudoemcodigo.com.br");
        await Assert.That(document.Info.License?.Name).IsEqualTo("Proprietary");

        await Assert.That(document.Components).IsNotNull();
        await Assert.That(document.Components!.SecuritySchemes).IsNotNull();
        await Assert.That(document.Components.SecuritySchemes!.ContainsKey("Bearer")).IsTrue();
        await Assert.That(document.Components.SecuritySchemes.ContainsKey("OpenID")).IsTrue();

        var bearerScheme = document.Components.SecuritySchemes["Bearer"] as OpenApiSecurityScheme;
        await Assert.That(bearerScheme).IsNotNull();
        await Assert.That(bearerScheme!.Scheme).IsEqualTo("bearer");
        await Assert.That(bearerScheme.BearerFormat).IsEqualTo("JWT");
    }

    [Test]
    public async Task DocumentTransformer_when_jwt_disabled_does_not_add_security_schemes()
    {
        var options = new TecOpenApiOptions
        {
            EnableJwtBearer = false
        };

        var transformer = new TecOpenApiDocumentTransformer(options);
        var document = new OpenApiDocument();
        var context = new OpenApiDocumentTransformerContext
        {
            DocumentName = "v1",
            DescriptionGroups = [],
            ApplicationServices = CreateServices()
        };

        await transformer.TransformAsync(document, context, CancellationToken.None);

        var schemes = document.Components?.SecuritySchemes;
        await Assert.That(schemes == null || schemes.Count == 0).IsTrue();
    }

    [Test]
    public async Task AuthorizeTransformer_adds_security_requirement_when_endpoint_has_authorize_metadata()
    {
        var options = new TecOpenApiOptions
        {
            EnableJwtBearer = true,
            JwtBearerSchemeName = "Bearer"
        };

        var transformer = new TecAuthorizeOperationTransformer(options);
        var operation = new OpenApiOperation();
        var metadata = new object[] { new FakeAuthorizeData() };
        var context = new OpenApiOperationTransformerContext
        {
            DocumentName = "v1",
            ApplicationServices = CreateServices(),
            Description = new Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription
            {
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor
                {
                    EndpointMetadata = metadata
                }
            }
        };

        await transformer.TransformAsync(operation, context, CancellationToken.None);

        await Assert.That(operation.Security).IsNotNull();
        await Assert.That(operation.Security!.Count).IsEqualTo(1);
        await Assert.That(operation.Responses).IsNotNull();
        await Assert.That(operation.Responses!.ContainsKey("401")).IsTrue();
        await Assert.That(operation.Responses.ContainsKey("403")).IsTrue();
    }

    [Test]
    public async Task AuthorizeTransformer_does_nothing_when_endpoint_has_no_authorize_metadata()
    {
        var options = new TecOpenApiOptions
        {
            EnableJwtBearer = true
        };

        var transformer = new TecAuthorizeOperationTransformer(options);
        var operation = new OpenApiOperation();
        var context = new OpenApiOperationTransformerContext
        {
            DocumentName = "v1",
            ApplicationServices = CreateServices(),
            Description = new Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription
            {
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor
                {
                    EndpointMetadata = Array.Empty<object>()
                }
            }
        };

        await transformer.TransformAsync(operation, context, CancellationToken.None);

        var security = operation.Security;
        await Assert.That(security == null || security.Count == 0).IsTrue();
        await Assert.That(operation.Responses == null || !operation.Responses.ContainsKey("401")).IsTrue();
    }
}
#endif
