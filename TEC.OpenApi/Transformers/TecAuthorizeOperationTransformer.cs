using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TEC.OpenApi.Transformers;

/// <summary>
/// Vincula requisitos de segurança JWT Bearer a operações que exigem autorização.
/// </summary>
public sealed class TecAuthorizeOperationTransformer(TecOpenApiOptions options) : IOpenApiOperationTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (!options.EnableJwtBearer)
            return Task.CompletedTask;

        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var hasAuthorize = metadata.OfType<IAuthorizeData>().Any();
        var hasAllowAnonymous = metadata.OfType<IAllowAnonymous>().Any();

        var requiresAuth = (options.RequireAuthorizationByDefault || hasAuthorize) && !hasAllowAnonymous;

        if (requiresAuth)
        {
            operation.Security ??= [];
            // Sem o documento hospedeiro a referência não resolve e o requisito sai vazio ("security": [{}]) no JSON
            var schemeReference = new OpenApiSecuritySchemeReference(options.JwtBearerSchemeName, context.Document);

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [schemeReference] = []
            });

            operation.Responses ??= new OpenApiResponses();
            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses["401"] = new OpenApiResponse
                {
                    Description = "Não autenticado: token JWT ausente, expirado ou inválido."
                };
            }

            if (!operation.Responses.ContainsKey("403"))
            {
                operation.Responses["403"] = new OpenApiResponse
                {
                    Description = "Acesso negado: o usuário autenticado não possui as permissões necessárias."
                };
            }
        }

        return Task.CompletedTask;
    }
}
